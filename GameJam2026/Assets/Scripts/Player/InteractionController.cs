using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRaycast raycastSystem; // Referència al nou sistema

    private void Start()
    {
        // Si no l'hem assignat manualment, el busquem al mateix objecte
        if (raycastSystem == null) raycastSystem = GetComponent<PlayerRaycast>();
    }
    
    void Update()
    {
        if (InputManager.Instance.InteractPressed)
        {
            HandleInteraction();
        }
    }
    
    private void HandleInteraction()
    {
        if (GameManager.Instance.CurrentState == GameState.Tutorial && 
            TutorialManager.Instance != null && 
            !TutorialManager.Instance.canPlayerInteract) return;

        // CANVI CLAU: Ara l'objecte en rang ve del Raycast, no del Trigger
        InteractableObject objectInRange = raycastSystem.CurrentTarget;

        bool hasObject = !InventoryManager.Instance.IsCurrentSlotEmpty();
        bool isNearObject = objectInRange != null;
        
        // La resta de la lògica es manté igual, però amb l'objecte del Raycast
        if (!hasObject && isNearObject && !objectInRange.isDeliveryZone)
        {
            PickUpObject(objectInRange);
        }
        else if (hasObject && isNearObject && !objectInRange.isDeliveryZone)
        {
            SwapObject(objectInRange);
        }
        else if (hasObject && isNearObject && objectInRange.isDeliveryZone)
        {
            AudioManager.Instance.PlaySFX(SFXType.ItemDrop, false);
            DeliverObject(objectInRange);
        }
    }
    
    public void PickUpObject(InteractableObject interactableObject)
    {
        // NUEVO: Verificar restricción de tipo de objeto en tutorial
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            TutorialPlayerRestrictions restrictions = FindAnyObjectByType<TutorialPlayerRestrictions>();
            if (restrictions != null && !restrictions.IsObjectAllowed(interactableObject.objectType))
            {
                Debug.Log($"<color=red>✗ No puedes coger {interactableObject.objectType} ahora (RestriccionTutorial)</color>");
                return;
            }
        }

        InventoryManager.Instance.TryAddToCurrentSlot(interactableObject);
        Debug.Log($"<color=green>✓ Objeto {interactableObject.objectType} recogido!</color>");
    }

    public void SwapObject(InteractableObject interactableObject)
    {
        // NUEVO: Verificar restricción de tipo de objeto en tutorial
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            TutorialPlayerRestrictions restrictions = FindAnyObjectByType<TutorialPlayerRestrictions>();
            if (restrictions != null && !restrictions.IsObjectAllowed(interactableObject.objectType))
            {
                Debug.Log($"<color=red>✗ No puedes intercambiar por {interactableObject.objectType} ahora (RestriccionTutorial)</color>");
                return;
            }
        }

        InventoryManager.Instance.SwapCurrentSlot(interactableObject);
        Debug.Log($"<color=cyan>↔ Objeto intercambiado por {interactableObject.objectType}</color>");
    }

    private void DeliverObject(InteractableObject obj)
    {
        ObjectType deliveredType = InventoryManager.Instance.GetCurrentObjectType();
        DeliveryBox deliveryBox = obj.GetComponent<DeliveryBox>();
        
        if (deliveryBox != null && deliveryBox.HasOrder())
        {
            if (deliveryBox.TryDeliverItem(deliveredType))
                InventoryManager.Instance.DeliverCurrentSlot();
        }
        else if (InventoryManager.Instance.DeliverCurrentSlot())
        {
            Debug.Log($"<color=yellow>✓ Objeto {deliveredType} entregado!</color>");
        }
    }

    // Pots esborrar o desactivar els mètodes OnTriggerEnter/Exit si ja no els fas servir
}