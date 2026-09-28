using UnityEngine;
public class ObjetoInteractivoMirada : MonoBehaviour
{
 [SerializeField] private float factorEscala = 1.08f;
 [SerializeField] private float velocidadRespuesta = 10f;
 [SerializeField] private float giroAlActivar = 30f;
 private Vector3 escalaOriginal;
 private Vector3 escalaObjetivo;
 private bool observado;
 private void Awake()
 {
 escalaOriginal = transform.localScale;
 escalaObjetivo = escalaOriginal;
 }
 private void Update()
 {
 transform.localScale = Vector3.Lerp(
 transform.localScale,
 escalaObjetivo,
 velocidadRespuesta * Time.deltaTime
 );
 }
 // =====================================================
 // MÉTODOS QUE GOOGLE CARDBOARD BUSCA AUTOMÁTICAMENTE
 // =====================================================
 public void OnPointerEnter()
 {
 EntrarMirada();
 }
 public void OnPointerExit()
 {
 SalirMirada();
 }
 public void OnPointerClick()
 {
 Activar();
 }
 // =====================================================
 // COMPORTAMIENTO DEL OBJETO
 // =====================================================
 public void EntrarMirada()
 {
 if (observado)
 return;
 observado = true;
 escalaObjetivo = escalaOriginal * factorEscala;
 }
 public void SalirMirada()
 {
 observado = false;
 escalaObjetivo = escalaOriginal;
 }
 public void Activar()
 {
 transform.Rotate(
 0f,
 giroAlActivar,
 0f,
 Space.Self
 );
 }
}
