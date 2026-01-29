using UnityEngine;

public class AnimacionManos : MonoBehaviour
{    [Header("Configuració del Moviment")]
    [SerializeField] private float velocidadMovimiento = 1f;
    [SerializeField] private float amplitudVertical = 0.05f;
    
    [Header("Configuració de Rotació")]
    [SerializeField] private float amplitudRotacionX = 2f;
    [SerializeField] private float amplitudRotacionZ = 1f;
    [SerializeField] private float velocidadRotacionX = 0.8f;
    [SerializeField] private float velocidadRotacionZ = 1.2f;

    [Header("Offsets per varietat")]
    [SerializeField] private float offsetTiempo = 0f;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    private void Start()
    {
        // Guardar la posición y rotación inicial
        posicionInicial = transform.localPosition;
        rotacionInicial = transform.localRotation;
    }

    private void Update()
    {
        AnimarIdle();
    }

    private void AnimarIdle()
    {
        // Calcular el tiempo con offset para crear variación
        float tiempo = Time.time * velocidadMovimiento + offsetTiempo;

        // Movimiento vertical suave (arriba y abajo)
        float movimientoY = Mathf.Sin(tiempo) * amplitudVertical;
        Vector3 nuevaPosicion = posicionInicial + new Vector3(0f, movimientoY, 0f);        // Rotación sutil en X y Z para dar más naturalidad
        float rotacionX = Mathf.Sin(tiempo * velocidadRotacionX) * amplitudRotacionX;
        float rotacionZ = Mathf.Cos(tiempo * velocidadRotacionZ) * amplitudRotacionZ;
        Quaternion nuevaRotacion = rotacionInicial * Quaternion.Euler(rotacionX, 0f, rotacionZ);

        // Aplicar las transformaciones
        transform.localPosition = nuevaPosicion;
        transform.localRotation = nuevaRotacion;
    }

    // Método para resetear a la posición inicial si es necesario
    public void ResetearPosicion()
    {
        transform.localPosition = posicionInicial;
        transform.localRotation = rotacionInicial;
    }
}
