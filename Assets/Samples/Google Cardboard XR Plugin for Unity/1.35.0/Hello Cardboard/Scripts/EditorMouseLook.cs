using UnityEngine;
using UnityEngine.InputSystem;
public class EditorMouseLook : MonoBehaviour
{
 [SerializeField] private Transform camara;
 [SerializeField] private float sensibilidad = 0.12f;
 [SerializeField] private float limiteVertical = 75f;
 private float horizontal;
 private float vertical;
 private void Start()
 {
 horizontal = transform.localEulerAngles.y;
 }
 private void Update()
 {
#if UNITY_EDITOR
 if (Mouse.current == null || camara == null) return;
 if (!Mouse.current.rightButton.isPressed) return;
 Vector2 delta = Mouse.current.delta.ReadValue();
 horizontal += delta.x * sensibilidad;
 vertical = Mathf.Clamp(vertical - delta.y * sensibilidad,
 -limiteVertical, limiteVertical);
 transform.localRotation = Quaternion.Euler(0f, horizontal, 0f);
 camara.localRotation = Quaternion.Euler(vertical, 0f, 0f);
#endif
 }
}
