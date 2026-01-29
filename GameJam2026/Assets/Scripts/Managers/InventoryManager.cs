using UnityEngine;
using System;

/// <summary>
/// Clase que representa un bolsillo del inventario
/// Guarda información sobre qué objeto contiene
/// </summary>
[Serializable]
public class InventorySlot
{
    public ObjectType objectType = ObjectType.None;
    public GameObject handPrefab = null; // Prefab que se instancia en la mano (INDEPENDIENTE del mundo)
    
    public bool IsEmpty()
    {
        return objectType == ObjectType.None || handPrefab == null;
    }
    
    public void Clear()
    {
        objectType = ObjectType.None;
        handPrefab = null;
    }
}

/// <summary>   
/// Gestiona el inventario del jugador con 3 bolsillos
/// Controla qué objetos tiene, cuál está seleccionado y los cambios de selección
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("Inventory Configuration")]
    [Tooltip("Los 3 bolsillos del inventario")]
    public InventorySlot[] slots = new InventorySlot[3];
    
    [Tooltip("Índice del bolsillo actualmente seleccionado (0, 1 o 2)")]
    [SerializeField] public int currentSlotIndex = 0;
    
    [Header("Hand Display")]
    [Tooltip("Transform donde se instanciarán los objetos en la mano")]
    public Transform handTransform;
    
    // Referencia al objeto actualmente instanciado en la mano
    private GameObject currentHandObject = null;
    
    // Evento que se dispara cuando cambia el objeto en la mano
    public event Action<ObjectType> OnSlotChanged;
    
    // Evento que se dispara cuando el inventario se actualiza (añadir, cambiar o entregar objeto)
    public event Action OnInventoryUpdated;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Inicializar los slots si no están inicializados
        if (slots == null || slots.Length != 3)
        {
            slots = new InventorySlot[3];
        }
        
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new InventorySlot();
            }
        }
    }
    
    void Update()
    {
        // En tutorial, solo permitir cambiar inventario si está habilitado
        if (GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            // Primero intentar con el nuevo sistema de tutorial
            if (TutorialPlayerRestrictions.Instance != null)
            {
                if (!TutorialPlayerRestrictions.Instance.canUseInventory)
                    return;
            }
            // Fallback al sistema antiguo si existe
            else if (TutorialManager.Instance != null && !TutorialManager.Instance.canPlayerUseInventory)
            {
                return;
            }
        }

        // Cambiar de bolsillo con la rueda del mouse
        float scroll = InputManager.Instance.MouseScrollDelta;
        
        if (scroll > 0)
        {
            // Scroll arriba - bolsillo anterior (índice baja)
            ChangeSlot(-1);
        }
        else if (scroll < 0)
        {
            // Scroll abajo - siguiente bolsillo (índice sube)
            ChangeSlot(1);
        }
    }
    
    /// <summary>
    /// Cambia el bolsillo seleccionado
    /// </summary>
    /// <param name="direction">1 para siguiente, -1 para anterior</param>
    private void ChangeSlot(int direction)
    {
        if (GameManager.Instance.CurrentState == GameState.Paused)
            return;
        AudioManager.Instance.PlaySFX(SFXType.InventoryChange);
        currentSlotIndex += direction;
        
        if (currentSlotIndex > 2)
            currentSlotIndex = 0;
        else if (currentSlotIndex < 0)
            currentSlotIndex = 2;
        
        // NUEVA VERIFICACIÓN: Comprobar si el tutorial permite este cambio de slot
        if (GameManager.Instance.CurrentState == GameState.Tutorial && 
            TutorialManager.Instance != null)
        {
            if (!TutorialManager.Instance.CanChangeToSlot(currentSlotIndex))
            {
                // Si no está permitido, revertir el cambio
                currentSlotIndex -= direction;
                
                if (currentSlotIndex > 2)
                    currentSlotIndex = 0;
                else if (currentSlotIndex < 0)
                    currentSlotIndex = 2;
                    
                return; // No actualizar la mano ni notificar el cambio
            }
        }
        
        // Actualizar el objeto en la mano
        UpdateHandObject();
        
        // Disparar evento
        OnSlotChanged?.Invoke(GetCurrentSlot().objectType);
        OnInventoryUpdated?.Invoke();
    }
    
    /// <summary>
    /// Actualiza el objeto que se muestra en la mano según el bolsillo seleccionado
    /// </summary>
    private void UpdateHandObject()
    {
        // Destruir el objeto actual en la mano si existe
        if (currentHandObject != null)
        {
            Destroy(currentHandObject);
            currentHandObject = null;
        }
        
        // Obtener el slot actual
        InventorySlot currentSlot = GetCurrentSlot();
        
        // Si el slot no está vacío y tiene un prefab, instanciarlo
        if (!currentSlot.IsEmpty() && currentSlot.handPrefab != null)
        {
            // Instanciar el prefab del objeto en la mano
            if (handTransform != null)
            {
                currentHandObject = Instantiate(
                    currentSlot.handPrefab,
                    handTransform
                );
                
                // Asegurar posición local correcta
                currentHandObject.transform.localPosition = Vector3.zero;
                currentHandObject.transform.localRotation = Quaternion.identity;
                currentHandObject.transform.localScale = Vector3.one;
                
                Debug.Log($"<color=green>Objeto {currentSlot.objectType} instanciado en la mano</color>");
            }
            else
            {
                Debug.LogError("<color=red>HandTransform no está asignado en InventoryManager!</color>");
            }
        }
    }
    
    /// <summary>
    /// Intenta añadir un objeto al slot actual
    /// </summary>
    /// <returns>True si se pudo añadir, false si el slot estaba ocupado</returns>
    public bool TryAddToCurrentSlot(InteractableObject obj)
    {
        InventorySlot slot = GetCurrentSlot();
        
        if (slot.IsEmpty())
        {
            // Guardar SOLO el tipo y el prefab, NO la referencia al objeto del mundo
            slot.objectType = obj.objectType;
            slot.handPrefab = obj.handPrefab;
            
            // PRIMERO: Mostrar en la mano (instanciar el prefab)
            UpdateHandObject();
            
            // DESPUÉS: El objeto del mundo se oculta/respawnea independientemente
            obj.PickUp();
            
            // Disparar evento de actualización
            OnInventoryUpdated?.Invoke();
            
            Debug.Log($"Objeto {obj.objectType} añadido al bolsillo {currentSlotIndex + 1}");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Reemplaza el objeto en el slot actual con otro objeto
    /// </summary>
    public void SwapCurrentSlot(InteractableObject newObj)
    {
        InventorySlot slot = GetCurrentSlot();
        
        if (!slot.IsEmpty())
        {
            ObjectType oldType = slot.objectType;
            
            // Simplemente reemplazar el contenido del slot
            slot.objectType = newObj.objectType;
            slot.handPrefab = newObj.handPrefab;
            
            // PRIMERO: Actualizar visual en la mano
            UpdateHandObject();
            
            // DESPUÉS: El objeto del mundo se oculta/respawnea independientemente
            newObj.PickUp();
            
            // Disparar evento de actualización
            OnInventoryUpdated?.Invoke();
            
            Debug.Log($"Objeto {oldType} cambiado por {newObj.objectType} en bolsillo {currentSlotIndex + 1}");
        }
    }
    
    /// <summary>
    /// Entrega el objeto del slot actual (lo elimina del inventario)
    /// </summary>
    public bool DeliverCurrentSlot()
    {
        InventorySlot slot = GetCurrentSlot();
        
        if (!slot.IsEmpty())
        {
            ObjectType deliveredType = slot.objectType;
            
            // Limpiar el slot (NO destruir nada del mundo, solo el inventario)
            slot.Clear();
            UpdateHandObject();
            
            // Disparar evento de actualización
            OnInventoryUpdated?.Invoke();
            
            Debug.Log($"Objeto {deliveredType} entregado desde bolsillo {currentSlotIndex + 1}");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Obtiene el slot actualmente seleccionado
    /// </summary>
    public InventorySlot GetCurrentSlot()
    {
        return slots[currentSlotIndex];
    }
    
    /// <summary>
    /// Verifica si el slot actual está vacío
    /// </summary>
    public bool IsCurrentSlotEmpty()
    {
        return GetCurrentSlot().IsEmpty();
    }
    
    /// <summary>
    /// Obtiene el tipo de objeto en el slot actual
    /// </summary>
    public ObjectType GetCurrentObjectType()
    {
        return GetCurrentSlot().objectType;
    }
    
    /// <summary>
    /// Obtiene el índice del slot actualmente seleccionado
    /// </summary>
    public int GetCurrentSlotIndex()
    {
        return currentSlotIndex;
    }
}
