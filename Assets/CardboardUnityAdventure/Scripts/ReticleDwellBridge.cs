using UnityEngine;

/// <summary>
/// ReticleDwellBridge
/// Compatible con Unity 6 + Google Cardboard XR.
///
/// Responsabilidad:
/// - Detectar objetos interactivos mediante Raycast.
/// - Iniciar/cancelar el progreso de permanencia (dwell) usando GazeManager.
/// - Enviar OnPointerClick() una sola vez al completar la permanencia.
/// - No modifica los scripts del SDK de Google Cardboard.
/// - No duplica OnPointerEnter()/OnPointerExit(): esos eventos pueden seguir
///   siendo gestionados por CardboardReticlePointer.
/// </summary>
[DisallowMultipleComponent]
public class ReticleDwellBridge : MonoBehaviour
{
    [Header("Gaze Manager")]
    [SerializeField] private GazeManager gazeManager;

    [Header("Filtro de interacción")]
    [SerializeField] private LayerMask interactionMask;
    [SerializeField] private bool useTagFilter = false;
    [SerializeField] private string interactableTag = "Interactable";
    [SerializeField] private float maxDistance = 20f;

    [Header("Dwell / Auto-Click")]
    [Min(0.1f)]
    [SerializeField] private float dwellTime = 1.2f;

    [SerializeField] private bool clickOncePerGaze = true;

    [Tooltip("Tolerancia breve cuando el Raycast pierde el objetivo para evitar reinicios por microcortes.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float lostGazeGrace = 0.12f;

    [Header("Pointer visual opcional")]
    [SerializeField] private GameObject pointer;

    [Range(0f, 1f)]
    [SerializeField] private float pointerT = 0.95f;

    [SerializeField] private float pointerScalePerMeter = 0.025f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;

    private GameObject currentTarget;
    private Vector3 lastHitPoint;
    private float lostTimer;
    private bool hasClickedThisGaze;
    private bool subscribed;

    private void OnEnable()
    {
        TryResolveGazeManager();
        TrySubscribe();

        if (pointer != null)
        {
            pointer.SetActive(false);
        }
    }

    private void OnDisable()
    {
        Unsubscribe();

        if (gazeManager != null)
        {
            gazeManager.CancelGazeSelection();
        }

        currentTarget = null;
        hasClickedThisGaze = false;
        lostTimer = 0f;

        if (pointer != null)
        {
            pointer.SetActive(false);
        }
    }

    private void TryResolveGazeManager()
    {
        if (gazeManager != null)
        {
            return;
        }

        gazeManager =
            GazeManager.Instance ??
            FindAnyObjectByType<GazeManager>(FindObjectsInactive.Include);
    }
    
    private void TrySubscribe()
    {
        if (subscribed || gazeManager == null)
        {
            return;
        }

        gazeManager.OnGazeSelection += OnGazeComplete;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed)
        {
            return;
        }

        if (gazeManager != null)
        {
            gazeManager.OnGazeSelection -= OnGazeComplete;
        }

        subscribed = false;
    }

    private void Update()
    {
        if (gazeManager == null)
        {
            TryResolveGazeManager();
        }

        if (!subscribed)
        {
            TrySubscribe();
        }

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (drawDebugRay)
        {
            Debug.DrawRay(origin, direction * maxDistance, Color.green);
        }

        if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                maxDistance,
                interactionMask,
                QueryTriggerInteraction.Ignore))
        {
            GameObject hitObject = hit.collider.gameObject;

            bool tagValido =
                !useTagFilter ||
                hitObject.CompareTag(interactableTag);

            if (tagValido)
            {
                lostTimer = 0f;

                if (hitObject != currentTarget)
                {
                    ChangeTarget(hitObject, hit.point);
                }
                else
                {
                    lastHitPoint = hit.point;
                }

                UpdatePointer(origin, lastHitPoint);
                return;
            }
        }

        HandleLostTarget(origin);
    }

    private void ChangeTarget(GameObject newTarget, Vector3 hitPoint)
    {
        if (currentTarget != null && gazeManager != null)
        {
            gazeManager.CancelGazeSelection();
        }

        currentTarget = newTarget;
        lastHitPoint = hitPoint;
        lostTimer = 0f;
        hasClickedThisGaze = false;

        if (gazeManager != null)
        {
            gazeManager.SetUpGaze(dwellTime);
            gazeManager.StartGazeSelection();
        }
    }

    private void HandleLostTarget(Vector3 cameraPosition)
    {
        if (currentTarget == null)
        {
            if (pointer != null && pointer.activeSelf)
            {
                pointer.SetActive(false);
            }

            return;
        }

        lostTimer += Time.deltaTime;

        if (lostTimer >= lostGazeGrace)
        {
            ResetTargetAndCancel();
        }
        else
        {
            UpdatePointer(cameraPosition, lastHitPoint);
        }
    }

    private void UpdatePointer(Vector3 cameraPosition, Vector3 hitPoint)
    {
        if (pointer == null)
        {
            return;
        }

        if (!pointer.activeSelf)
        {
            pointer.SetActive(true);
        }

        Vector3 position =
            Vector3.Lerp(cameraPosition, hitPoint, pointerT);

        pointer.transform.position = position;

        float distance =
            Vector3.Distance(cameraPosition, hitPoint);

        pointer.transform.localScale =
            Vector3.one * (pointerScalePerMeter * distance);

        Vector3 lookDirection =
            pointer.transform.position - cameraPosition;

        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            pointer.transform.rotation =
                Quaternion.LookRotation(lookDirection);
        }
    }

    private void ResetTargetAndCancel()
    {
        if (gazeManager != null)
        {
            gazeManager.CancelGazeSelection();
        }

        currentTarget = null;
        hasClickedThisGaze = false;
        lostTimer = 0f;

        if (pointer != null)
        {
            pointer.SetActive(false);
        }
    }

    private void OnGazeComplete()
    {
        if (currentTarget == null)
        {
            return;
        }

        if (clickOncePerGaze && hasClickedThisGaze)
        {
            return;
        }

        Debug.Log(
            $"[ReticleDwellBridge] Gaze complete on: {currentTarget.name}"
        );

        currentTarget.SendMessage(
            "OnPointerClick",
            SendMessageOptions.DontRequireReceiver
        );

        hasClickedThisGaze = true;

        // Detiene el progreso, pero conserva currentTarget.
        // No vuelve a iniciar mientras la mirada siga en el mismo objeto.
        if (gazeManager != null)
        {
            gazeManager.CancelGazeSelection();
        }
    }
}
