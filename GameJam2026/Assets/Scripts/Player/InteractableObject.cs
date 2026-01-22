using UnityEngine;

/// <summary>
/// Enum para los tipos de objetos interactuables
/// Mejor que usar tags porque es más eficiente y type-safe
/// </summary>
public enum ObjectType
{
    None,       // Para bolsillos vacíos
    Espada,
    Arco,
    Lanza,
    // Añade más tipos aquí según necesites
}

/// <summary>
/// Script para objetos que se pueden recoger y guardar en el inventario
/// Debe estar en los prefabs de objetos interactuables
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("Object Configuration")]
    [Tooltip("Tipo de objeto - define qué es")]
    public ObjectType objectType = ObjectType.None;
    
    [Tooltip("Prefab que se instanciará en las manos del jugador")]
    public GameObject handPrefab;
    
    [Tooltip("Si es verdadero, este objeto es una zona de entrega")]
    public bool isDeliveryZone = false;
    
    // Referencia al Rigidbody si tiene física
    private Rigidbody rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    /// <summary>
    /// Desactiva el objeto del mundo (cuando se recoge)
    /// </summary>
    public void PickUp()
    {
        // Desactivar física
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Ocultar el objeto del mundo
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Reactiva el objeto en el mundo (cuando se suelta)
    /// </summary>
    public void Drop(Vector3 position)
    {
        // Reactivar el objeto
        gameObject.SetActive(true);
        transform.position = position;
        
        // Reactivar física
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
}
