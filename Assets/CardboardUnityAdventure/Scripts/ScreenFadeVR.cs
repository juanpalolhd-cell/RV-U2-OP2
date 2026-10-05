using System.Collections;
using UnityEngine;
public class ScreenFadeVR : MonoBehaviour
{
 [SerializeField] private CanvasGroup panel;
 [SerializeField, Min(0.05f)] private float duracion = 0.45f;
 private Coroutine transicion;
 private void Awake()
 {
 if (panel == null) { Debug.LogError("Asigna FadeCanvas en Panel.", this); enabled = false; return; }
 panel.alpha = 1f;
 panel.interactable = false;
 panel.blocksRaycasts = false;
 }
 private void Start() { Abrir(); }
 public void Abrir() { Iniciar(0f); } // negro a escena
 public void Cerrar() { Iniciar(1f); } // escena a negro
 public IEnumerator CerrarYEsperar()
 {
 Iniciar(1f);
 while (transicion != null) yield return null;
 }
 public IEnumerator AbrirYEsperar()
 {
 Iniciar(0f);
 while (transicion != null) yield return null;
 }
 private void Iniciar(float destino)
 {
 if (!isActiveAndEnabled) return;
 if (transicion != null) StopCoroutine(transicion);
 transicion = StartCoroutine(Animar(destino));
 }
 private IEnumerator Animar(float destino)
 {
 float origen = panel.alpha;
 float tiempo = 0f;
 while (tiempo < duracion)
 {
 tiempo += Mathf.Min(Time.unscaledDeltaTime, 0.033f);
 panel.alpha = Mathf.Lerp(origen, destino, Mathf.Clamp01(tiempo / duracion));
 yield return null;
 }
 panel.alpha = destino;
 transicion = null;
 }
}