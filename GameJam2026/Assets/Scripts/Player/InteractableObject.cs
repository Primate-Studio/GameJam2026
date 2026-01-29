using UnityEngine;

/// <summary>
/// Enum para los tipos de objetos interactuables
/// Mejor que usar tags porque es más eficiente y type-safe
/// </summary>
public enum ObjectType
{
    None, Hilo, Red, Espejo, Sandalia, Alas, Brújula, Tridente, CascoA, Rayo, Arco ,Mandoble, CascoH, Escudo, Mascaras, Odre, CoronaL, Cinturon       // Para bolsillos vacíos
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
    public float respawnTime = 12f;

    [Header("Effects")]
    [Tooltip("Prefab de partícules que apareixerà en fer respawn")]
    [SerializeField] private GameObject respawnParticlePrefab;
    
    [Tooltip("Si es verdadero, el objeto se regenerará infinitamente")]
    public bool infiniteRespawn = true;
    public ItemIconController myIcon;
    
    // Referencia al Rigidbody si tiene física
    private Rigidbody rb;
    private Collider col;
    private Renderer rend;
    private Renderer[] childRenderers;
    private Collider[] childColliders;
    
    // Posición inicial del objeto para respawn
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private GameObject[] childGO;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        childRenderers = GetComponentsInChildren<Renderer>(true);
        childColliders = GetComponentsInChildren<Collider>(true);

        // Guardar posición inicial para respawn
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }
    void Start()
    {
        if(myIcon != null)
        {
            myIcon.SetupIcon(this);
        }
    }
    /// <summary>
    /// Desactiva el objeto del mundo (cuando se recoge)
    /// </summary>
    public void PickUp()
    {
        AudioManager.Instance.PlaySFX(SFXType.PickUp, true);
        if (infiniteRespawn)
        {
            // Sistema de respawn infinito - deshabilitar componentes pero mantener GameObject activo
            Debug.Log($"<color=yellow>Iniciando respawn de {objectType} en {respawnTime} segundos...</color>");
            if(myIcon != null) myIcon.StartTimer(respawnTime);
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
            DespawnObject();
        }
    }

    private void DespawnObject()
    {
        gameObject.GetComponentInChildren<Transform>().gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Corrutina que maneja el respawn del objeto
    /// </summary>
    private System.Collections.IEnumerator RespawnAfterDelay()
    {
        // Deshabilitar componentes para "ocultar" el objeto sin desactivar el GameObject
        foreach (var c in childColliders) c.enabled = false;
        foreach (var r in childRenderers) r.enabled = false;
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
        foreach (var c in childColliders) c.enabled = true;
        foreach (var r in childRenderers) r.enabled = true;

        if (respawnParticlePrefab != null)
        {
            Vector3 spawnPosition = initialPosition;
            RaycastHit hit;

            // Disparem un raig des de la posició de l'objecte cap avall (màxim 2 unitats)
            // Posem initialPosition + Vector3.up * 0.1f per evitar que el raig comenci "dins" del terra
            if (Physics.Raycast(initialPosition + Vector3.up * 0.1f, Vector3.down, out hit, 2.0f))
            {
                // Si trobem una superfície (terra, taula o prestatgeria), posem les partícules allà
                spawnPosition = hit.point;
            }

            Instantiate(respawnParticlePrefab, spawnPosition, Quaternion.identity);
        }
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

        foreach (var c in childColliders) c.enabled = true;
        foreach (var r in childRenderers) r.enabled = true;
    }
}
