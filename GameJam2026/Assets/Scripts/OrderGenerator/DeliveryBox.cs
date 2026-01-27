using UnityEngine;
using TMPro;

/// <summary>
/// Script para las cajas/sacos donde se entregan los objetos
/// Debe ir en el prefab de la caja de entrega junto con InteractableObject
/// </summary>
[RequireComponent(typeof(InteractableObject))]
public class DeliveryBox : MonoBehaviour
{
    [Header("Visual")]
    [Tooltip("Texto opcional para mostrar info del pedido")]
    public TextMeshProUGUI orderInfoText;
    
    [Header("Animation")]
    [Tooltip("Escala máxima del efecto (1.2 = 20% más grande)")]
    [SerializeField] private float scaleAmount = 1.2f;
    [Tooltip("Duración del efecto en segundos")]
    [SerializeField] private float scaleDuration = 0.3f;
    
    [Header("Assigned Order")]
    [SerializeField] private Order assignedOrder;
    
    private InteractableObject interactableObj;
    private Vector3 originalScale;
    private bool isScaling = false;
    
    void Awake()
    {
        // Obtener y configurar el InteractableObject
        interactableObj = GetComponent<InteractableObject>();
        
        if (interactableObj != null)
        {
            // Marcar como zona de entrega
            interactableObj.isDeliveryZone = true;
            interactableObj.objectType = ObjectType.None;
        }
        
        // Guardar escala original
        originalScale = transform.localScale;
    }
    
    /// <summary>
    /// Asigna un pedido a esta caja
    /// </summary>
    public void AssignOrder(Order order)
    {
        assignedOrder = order;
        UpdateVisuals();
    }
    
    /// <summary>
    /// Obtiene el pedido asignado
    /// </summary>
    public Order GetOrder()
    {
        return assignedOrder;
    }
    
    /// <summary>
    /// Verifica si esta caja tiene un pedido asignado
    /// </summary>
    public bool HasOrder()
    {
        return assignedOrder != null;
    }
    
    /// <summary>
    /// Intenta añadir un objeto entregado a este pedido
    /// </summary>
    public bool TryDeliverItem(ObjectType itemType)
    {
        if (assignedOrder == null)
        {
            Debug.LogWarning("Esta caja no tiene pedido asignado!");
            return false;
        }
        if (!isScaling)
        {
            StartCoroutine(ScaleEffect());
        }
        bool success = OrderSystem.Instance.TryDeliverItem(assignedOrder, itemType);
        
        if (success)
        {
            UpdateVisuals();
        }
        
        return success;
    }
    
    /// <summary>
    /// Actualiza la información visual de la caja
    /// </summary>
    private void UpdateVisuals()
    {
        if (assignedOrder == null) return;
        
        // Actualizar texto si existe
        if (orderInfoText != null)
        {
            string requirements = $"Monster: {assignedOrder.monster.requirementName}\n" +
                                 $"Condition: {assignedOrder.condition.requirementName}\n";
            
            if (assignedOrder.environment != null)
            {
                requirements += $"Environment: {assignedOrder.environment.requirementName}\n";
            }
            
            orderInfoText.text = $"Pedido #{assignedOrder.orderID}\n" +
                                $"{assignedOrder.deliveredItems.Count}/{assignedOrder.itemsNeeded} objetos\n" +
                                requirements;
        }
        
        // TODO: Añadir más feedback visual (colores, efectos, etc.)
        
    }

    private System.Collections.IEnumerator ScaleEffect()
    {
        isScaling = true;
        float elapsed = 0f;
        float halfDuration = scaleDuration / 2f;
        
        // Crecer
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleAmount, t);
            yield return null;
        }
        
        elapsed = 0f;
        
        // Volver al tamaño original
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(originalScale * scaleAmount, originalScale, t);
            yield return null;
        }
        
        transform.localScale = originalScale;
        isScaling = false;
    }
    
    void OnDrawGizmos()
    {
        // Dibujar un cubo en la vista de escena para debug
        Gizmos.color = assignedOrder != null ? Color.green : Color.gray;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}
