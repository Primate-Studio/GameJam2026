using UnityEngine;

/// <summary>
/// Enum para los tipos de objetos interactuables
/// Mejor que usar tags porque es más eficiente y type-safe
/// </summary>
public enum ObjectType
{
    None,       // Para bolsillos vacíos
    Sword,
    Bow,
    Spear,
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
    
    [Header("Respawn Settings")]
    [Tooltip("Tiempo en segundos antes de que el objeto reaparezca después de ser recogido")]
    public float respawnTime = 2f;
    
    [Tooltip("Si es verdadero, el objeto se regenerará infinitamente")]
    public bool infiniteRespawn = true;
    
    // Referencia al Rigidbody si tiene física
    private Rigidbody rb;
    private Collider col;
    private Renderer rend;
    
    // Posición inicial del objeto para respawn
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        
        // Guardar posición inicial para respawn
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }
    
    /// <summary>
    /// Desactiva el objeto del mundo (cuando se recoge)
    /// </summary>
    public void PickUp()
    {
        if (infiniteRespawn)
        {
            // Sistema de respawn infinito - deshabilitar componentes pero mantener GameObject activo
            Debug.Log($"<color=yellow>Iniciando respawn de {objectType} en {respawnTime} segundos...</color>");
            StartCoroutine(RespawnAfterDelay());
        }
        else
        {
            // Desactivar física
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            
            // Ocultar el objeto del mundo
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Corrutina que maneja el respawn del objeto
    /// </summary>
    private System.Collections.IEnumerator RespawnAfterDelay()
    {
        // Deshabilitar componentes para "ocultar" el objeto sin desactivar el GameObject
        if (col != null) col.enabled = false;
        if (rend != null) rend.enabled = false;
        if (rb != null) rb.isKinematic = true;
        
        // Esperar el tiempo de respawn
        yield return new WaitForSeconds(respawnTime);
        
        // Resetear posición
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        
        // Resetear física si tiene
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }
        
        // Reactivar componentes
        if (col != null) col.enabled = true;
        if (rend != null) rend.enabled = true;
        
        Debug.Log($"<color=cyan>✨ {objectType} regenerado!</color>");
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
