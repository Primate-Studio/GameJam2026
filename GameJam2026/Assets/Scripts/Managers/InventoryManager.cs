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
    public InteractableObject objectReference = null; // Referencia al objeto en el mundo
    
    public bool IsEmpty()
    {
        return objectType == ObjectType.None || objectReference == null;
    }
    
    public void Clear()
    {
        objectType = ObjectType.None;
        objectReference = null;
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
    [SerializeField] private InventorySlot[] slots = new InventorySlot[3];
    
    [Tooltip("Índice del bolsillo actualmente seleccionado (0, 1 o 2)")]
    [SerializeField] private int currentSlotIndex = 0;
    
    [Header("Hand Display")]
    [Tooltip("Transform donde se instanciarán los objetos en la mano")]
    public Transform handTransform;
    
    // Referencia al objeto actualmente instanciado en la mano
    private GameObject currentHandObject = null;
    
    // Evento que se dispara cuando cambia el objeto en la mano
    public event Action<ObjectType> OnSlotChanged;
    
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
        // Cambiar de bolsillo con la rueda del mouse
        float scroll = InputManager.Instance.MouseScrollDelta;
        
        if (scroll > 0)
        {
            // Scroll arriba - siguiente bolsillo
            ChangeSlot(1);
        }
        else if (scroll < 0)
        {
            // Scroll abajo - bolsillo anterior
            ChangeSlot(-1);
        }
    }
    
    /// <summary>
    /// Cambia el bolsillo seleccionado
    /// </summary>
    /// <param name="direction">1 para siguiente, -1 para anterior</param>
    private void ChangeSlot(int direction)
    {
        currentSlotIndex += direction;
        
        // Wrap around (si llegas al final, vuelves al principio)
        if (currentSlotIndex > 2)
            currentSlotIndex = 0;
        else if (currentSlotIndex < 0)
            currentSlotIndex = 2;
        
        Debug.Log($"Bolsillo cambiado a: {currentSlotIndex + 1}");
        
        // Actualizar el objeto en la mano
        UpdateHandObject();
        
        // Disparar evento
        OnSlotChanged?.Invoke(GetCurrentSlot().objectType);
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
        
        // Si el slot no está vacío y tiene un objeto, instanciarlo
        if (!currentSlot.IsEmpty() && currentSlot.objectReference != null)
        {
            // Instanciar el prefab del objeto en la mano
            if (currentSlot.objectReference.handPrefab != null && handTransform != null)
            {
                currentHandObject = Instantiate(
                    currentSlot.objectReference.handPrefab,
                    handTransform.position,
                    handTransform.rotation,
                    handTransform
                );
                
                Debug.Log($"Objeto {currentSlot.objectType} instanciado en la mano");
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
            slot.objectType = obj.objectType;
            slot.objectReference = obj;
            obj.PickUp(); // Desactivar el objeto del mundo
            UpdateHandObject(); // Mostrar en la mano
            
            Debug.Log($"Objeto {obj.objectType} añadido al bolsillo {currentSlotIndex + 1}");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Reemplaza el objeto en el slot actual con otro objeto
    /// El objeto anterior se devuelve para ser colocado en el mundo
    /// </summary>
    public InteractableObject SwapCurrentSlot(InteractableObject newObj, Vector3 dropPosition)
    {
        InventorySlot slot = GetCurrentSlot();
        
        if (!slot.IsEmpty())
        {
            // Guardar referencia al objeto anterior
            InteractableObject oldObj = slot.objectReference;
            
            // Colocar el objeto anterior en el mundo
            oldObj.Drop(dropPosition);
            
            // Reemplazar con el nuevo objeto
            slot.objectType = newObj.objectType;
            slot.objectReference = newObj;
            newObj.PickUp();
            UpdateHandObject();
            
            Debug.Log($"Objeto {oldObj.objectType} cambiado por {newObj.objectType} en bolsillo {currentSlotIndex + 1}");
            
            return oldObj;
        }
        
        return null;
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
            
            // Destruir el objeto (ya que se entrega)
            if (slot.objectReference != null)
            {
                Destroy(slot.objectReference.gameObject);
            }
            
            // Limpiar el slot
            slot.Clear();
            UpdateHandObject();
            
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
}
