using UnityEngine;

/// <summary>
/// Controla las interacciones del jugador con objetos del mundo
/// Gestiona el trigger delante de las manos, detecta objetos y maneja coger/cambiar/entregar
/// Este script debe ir en el GameObject del jugador que tiene el trigger "InteractTrigger"
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractionController : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Distancia delante del jugador donde aparecerán objetos soltados")]
    [SerializeField] private float dropDistance = 1.5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Objeto actualmente en contacto con el trigger
    private InteractableObject objectInRange = null;
    
    // Referencia al collider del trigger (debe ser configurado como Trigger en el Inspector)
    private Collider triggerCollider;
    
    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        
        // Asegurar que es un trigger
        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning("El collider de InteractionController debe ser un Trigger! Configurando automáticamente...");
            triggerCollider.isTrigger = true;
        }
    }
    
    void Update()
    {
        // Detectar cuando se presiona E
        if (InputManager.Instance.InteractPressed)
        {
            HandleInteraction();
        }
    }
    
    /// <summary>
    /// Maneja la lógica de interacción cuando se presiona E
    /// </summary>
    private void HandleInteraction()
    {
        bool hasObject = !InventoryManager.Instance.IsCurrentSlotEmpty();
        bool isNearObject = objectInRange != null;
        
        // CASO 1: No tengo objeto Y estoy cerca de uno -> COGER
        if (!hasObject && isNearObject && !objectInRange.isDeliveryZone)
        {
            PickUpObject();
        }
        // CASO 2: Tengo objeto Y estoy cerca de otro objeto -> CAMBIAR
        else if (hasObject && isNearObject && !objectInRange.isDeliveryZone)
        {
            SwapObject();
        }
        // CASO 3: Tengo objeto Y estoy cerca de zona de entrega -> ENTREGAR
        else if (hasObject && isNearObject && objectInRange.isDeliveryZone)
        {
            DeliverObject();
        }
        // CASO 4: No hay nada que hacer
        else
        {
            if (showDebugInfo)
            {
                if (!hasObject && !isNearObject)
                    Debug.Log("No tienes objeto y no hay nada cerca para coger");
                else if (hasObject && !isNearObject)
                    Debug.Log("Tienes un objeto pero no hay nada cerca para interactuar");
            }
        }
    }
    
    /// <summary>
    /// Recoge un objeto del mundo y lo añade al inventario
    /// </summary>
    private void PickUpObject()
    {
        if (InventoryManager.Instance.TryAddToCurrentSlot(objectInRange))
        {
            Debug.Log($"<color=green>✓ Objeto {objectInRange.objectType} recogido!</color>");
            objectInRange = null; // Ya no está en rango porque está en el inventario
        }
        else
        {
            Debug.Log("<color=yellow>! El bolsillo actual está lleno. Cambia de bolsillo con la rueda del mouse.</color>");
        }
    }
    
    /// <summary>
    /// Cambia el objeto actual por otro del mundo
    /// </summary>
    private void SwapObject()
    {
        ObjectType oldType = InventoryManager.Instance.GetCurrentObjectType();
        InventoryManager.Instance.SwapCurrentSlot(objectInRange);
        
        Debug.Log($"<color=cyan>↔ Objeto {oldType} cambiado por {objectInRange.objectType}</color>");
    }
    
    /// <summary>
    /// Entrega el objeto actual a la zona de entrega
    /// </summary>
    private void DeliverObject()
    {
        ObjectType deliveredType = InventoryManager.Instance.GetCurrentObjectType();
        
        if (InventoryManager.Instance.DeliverCurrentSlot())
        {
            Debug.Log($"<color=yellow>✓ Objeto {deliveredType} entregado al cliente!</color>");
            
            // Aquí puedes añadir lógica adicional:
            // - Dar puntos al jugador
            // - Actualizar UI
            // - Reproducir sonido
            // - etc.
        }
    }
    
    /// <summary>
    /// Calcula la posición donde se debe soltar un objeto
    /// </summary>
    private Vector3 GetDropPosition()
    {
        // Posición delante del jugador
        return transform.position + transform.forward * dropDistance;
    }
    
    /// <summary>
    /// Detecta cuando un objeto entra en el trigger
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        InteractableObject obj = other.GetComponent<InteractableObject>();
        
        if (obj != null && other.gameObject.activeInHierarchy)
        {
            objectInRange = obj;
            
            if (showDebugInfo)
            {
                if (obj.isDeliveryZone)
                {
                    Debug.Log($"<color=yellow>[Trigger] Zona de entrega detectada</color>");
                }
                else
                {
                    Debug.Log($"<color=cyan>[Trigger] Objeto {obj.objectType} detectado - Presiona E para interactuar</color>");
                }
            }
        }
    }
    
    /// <summary>
    /// Detecta cuando un objeto sale del trigger
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        InteractableObject obj = other.GetComponent<InteractableObject>();
        
        if (obj != null && obj == objectInRange)
        {
            if (showDebugInfo)
            {
                Debug.Log($"<color=gray>[Trigger] Objeto {obj.objectType} fuera de rango</color>");
            }
            
            objectInRange = null;
        }
    }
    
    /// <summary>
    /// Dibuja el trigger en la vista de escena para debug
    /// </summary>
    private void OnDrawGizmos()
    {
        if (showDebugInfo && triggerCollider != null)
        {
            Gizmos.color = objectInRange != null ? Color.green : Color.yellow;
            
            // Dibujar el área del trigger
            if (triggerCollider is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (triggerCollider is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
            }
        }
    }
}
