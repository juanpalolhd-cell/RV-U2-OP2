using System.Collections;
using UnityEngine;

public class TeleportVR : MonoBehaviour
{
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform headCamera;
    [SerializeField] private ScreenFadeVR fade;
    [SerializeField] private Transform destination;
    private bool ocupado;

    public void Teleportar()
    {
        if (ocupado) return;
        if (playerRoot == null || headCamera == null || fade == null || destination == null)
        {
            Debug.LogError("Faltan referencias de teletransporte.", this);
            return;
        }
        StartCoroutine(Secuencia());
    }

    private IEnumerator Secuencia()
    {
        ocupado = true;
        yield return fade.CerrarYEsperar();

        Vector3 offsetHorizontal = headCamera.position - playerRoot.position;
        offsetHorizontal.y = 0f;
        playerRoot.position = destination.position - offsetHorizontal;

        // Espera un frame para actualizar la vista en el destino.
        yield return null;
        yield return fade.AbrirYEsperar();
        ocupado = false;
    }

    public void OnPointerClick() { Teleportar(); }

    public void OnPointerEnter()
    {
        Debug.Log("Reticula sobre: " + gameObject.name);

 
    }

    public void OnPointerExit()
    {
        Debug.Log("Reticula salio de: " + gameObject.name);


    }



}
