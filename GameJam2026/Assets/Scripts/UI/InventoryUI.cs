using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la interfaz visual del inventario
/// Muestra los 3 slots y resalta el slot activo
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Referencias a los 3 slots del inventario en el UI")]
    [SerializeField] private InventorySlotUI[] slotUIs = new InventorySlotUI[3];
    
    [Header("Visual Settings")]
    [Tooltip("Color del borde cuando el slot está seleccionado")]
    [SerializeField] private Color selectedColor = Color.yellow;
    
    [Tooltip("Color del borde cuando el slot NO está seleccionado")]
    [SerializeField] private Color normalColor = Color.gray;
    
    
    void Start()
    {
        // Suscribirse al evento de cambio de slot
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnSlotChanged += OnSlotChanged;
            InventoryManager.Instance.OnInventoryUpdated += OnInventoryUpdated;
        }
        
        // Inicializar la UI
        UpdateAllSlots();
        UpdateSelection(0); // Slot inicial es el 0
    }
    
    void OnDestroy()
    {
        // Desuscribirse del evento
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnSlotChanged -= OnSlotChanged;
            InventoryManager.Instance.OnInventoryUpdated -= OnInventoryUpdated;
        }
    }
    
    /// <summary>
    /// Callback cuando cambia el slot seleccionado
    /// </summary>
    private void OnSlotChanged(ObjectType objectType)
    {
        UpdateAllSlots();
    }
    
    /// <summary>
    /// Callback cuando el inventario se actualiza (añadir, cambiar o entregar objeto)
    /// </summary>
    private void OnInventoryUpdated()
    {
        UpdateAllSlots();
    }
    
    /// <summary>
    /// Actualiza todos los slots del inventario
    /// </summary>
    public void UpdateAllSlots()
    {
        if (InventoryManager.Instance == null) return;
        
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] != null)
            {
                // Obtener el slot del inventario
                InventorySlot slot = InventoryManager.Instance.slots[i];
                
                // Actualizar el icono
                UpdateSlotIcon(i, slot);
                
                // Actualizar el indicador de selección
                bool isSelected = (i == GetCurrentSlotIndex());
                UpdateSlotSelection(i, isSelected);
            }
        }
    }
    
    /// <summary>
    /// Actualiza el icono de un slot específico
    /// </summary>
    private void UpdateSlotIcon(int slotIndex, InventorySlot slot)
    {
        if (slotUIs[slotIndex] == null) return;
        
        if (slot.IsEmpty())
        {
            // Slot vacío - ocultar el icono
            slotUIs[slotIndex].iconImage.enabled = false;
        }
        else
        {
            // Slot con objeto - mostrar el icono
            slotUIs[slotIndex].iconImage.enabled = true;
            slotUIs[slotIndex].iconImage.sprite = GetSpriteForObjectType(slot.objectType);
            slotUIs[slotIndex].iconImage.color = Color.black;
        }
    }
    
    /// <summary>
    /// Actualiza la selección visual de un slot
    /// </summary>
    private void UpdateSlotSelection(int slotIndex, bool isSelected)
    {
        if (slotUIs[slotIndex] == null) return;
        
        // Cambiar color del borde
        if (slotUIs[slotIndex].borderImage != null)
        {
            slotUIs[slotIndex].borderImage.color = isSelected ? selectedColor : normalColor;
        }
        
        // No tocar la escala - se mantiene la de la escena
    }
    
    /// <summary>
    /// Actualiza qué slot está seleccionado
    /// </summary>
    private void UpdateSelection(int slotIndex)
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            UpdateSlotSelection(i, i == slotIndex);
        }
    }
    
    /// <summary>
    /// Obtiene el sprite correspondiente al tipo de objeto
    /// </summary>
    private Sprite GetSpriteForObjectType(ObjectType objectType)
    {
        // Buscar en la base de datos de objetos
        InteractableObjectDatabase database = InteractableObjectDatabase.Instance;
        if (database != null)
        {
            return database.GetIconForObjectType(objectType);
        }
        
        return null;
    }
    
    /// <summary>
    /// Obtiene el índice del slot actual del InventoryManager
    /// </summary>
    private int GetCurrentSlotIndex()
    {
        if (InventoryManager.Instance == null) return 0;
        return InventoryManager.Instance.GetCurrentSlotIndex();
    }
}

/// <summary>
/// Clase helper que representa un slot de UI
/// Guarda las referencias a los componentes visuales de cada slot
/// </summary>
[System.Serializable]
public class InventorySlotUI
{
    [Tooltip("Transform raíz del slot")]
    public Transform transform;
    
    [Tooltip("Imagen del icono del objeto")]
    public Image iconImage;
    
    [Tooltip("Imagen del borde/fondo del slot")]
    public Image borderImage;
    
    [Tooltip("Texto opcional para mostrar el número del slot")]
    public UnityEngine.UI.Text slotNumberText;
}
