using UnityEngine;
using TMPro;
public class DialogManager : MonoBehaviour
{
    [Header("Referencias")]
    public string dialogoTexto = "Hola, ¡funcionó!";
    public TextMeshProUGUI dialogText; // arrastra BubbleText
    public GameObject burbuja; // arrastra el Canvas o el contenedor UI

    void Start() { OnPointerExit(); } // oculta al iniciar
    
    public void OnPointerEnter()
    {
        burbuja.SetActive(true);
        dialogText.text = dialogoTexto;
        Debug.Log("<color=green><b>Dialogo ENTER</b></color>");
    }

    public void OnPointerExit()
    {
        burbuja.SetActive(false);
        Debug.Log("<color=red><b>Dialogo EXIT</b></color>");
    }
}