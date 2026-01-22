using UnityEngine;

/// <summary>
/// SCRIPT DE EJEMPLO - Muestra cómo configurar objetos específicos
/// Puedes crear scripts similares para añadir comportamiento personalizado a cada tipo de objeto
/// </summary>
public class SwordBehavior : MonoBehaviour
{
    [Header("Sword Properties")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private ParticleSystem slashEffect;
    
    private InteractableObject interactableObj;
    
    void Awake()
    {
        // Obtener el componente InteractableObject
        interactableObj = GetComponent<InteractableObject>();
        
        if (interactableObj == null)
        {
            Debug.LogError("SwordBehavior requiere un InteractableObject en el mismo GameObject!");
        }
    }
    
    void Start()
    {
        // Verificar que es una espada
        if (interactableObj != null && interactableObj.objectType != ObjectType.Espada)
        {
            Debug.LogWarning("Este objeto debería ser tipo Espada!");
        }
    }
    
    /// <summary>
    /// Método de ejemplo que podrías llamar cuando el jugador use la espada
    /// Por ahora solo es un ejemplo, en el futuro podrías añadir input para atacar
    /// </summary>
    public void Attack()
    {
        Debug.Log($"¡Espada usada! Daño: {damage}");
        
        if (slashEffect != null)
        {
            slashEffect.Play();
        }
        
        // Aquí podrías añadir:
        // - Raycast para detectar enemigos
        // - Aplicar daño
        // - Reproducir sonido
        // - Animación de ataque
    }
    
    /// <summary>
    /// Ejemplo de método que se podría llamar cuando se entrega
    /// </summary>
    public void OnDelivered()
    {
        Debug.Log("Espada entregada al cliente!");
        
        // Aquí podrías:
        // - Dar puntos específicos por entregar espada
        // - Desbloquear logro
        // - Activar diálogo del cliente
    }
}

/*
 * NOTAS DE USO:
 * 
 * 1. Este script es OPCIONAL - InteractableObject ya funciona solo
 * 2. Usa esto si quieres añadir comportamiento específico por tipo de objeto
 * 3. Ejemplo de estructura de GameObject:
 * 
 *    Espada (GameObject)
 *    ├── SwordModel (Mesh)
 *    ├── Box Collider
 *    ├── Rigidbody
 *    ├── InteractableObject.cs ← OBLIGATORIO
 *    └── SwordBehavior.cs ← OPCIONAL (comportamiento custom)
 * 
 * 4. Para crear comportamiento de Arco o Lanza, copia este script
 *    y renómbralo (BowBehavior.cs, SpearBehavior.cs, etc.)
 */
