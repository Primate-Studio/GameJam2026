using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base de datos centralizada de información de objetos
/// Guarda los sprites/iconos para cada tipo de objeto
/// </summary>
public class InteractableObjectDatabase : MonoBehaviour
{
    public static InteractableObjectDatabase Instance { get; private set; }
    
    [Header("Object Icons")]
    [Tooltip("Lista de iconos para cada tipo de objeto")]
    [SerializeField] private List<ObjectIconData> objectIcons = new List<ObjectIconData>();
    
    // Diccionario para búsqueda rápida
    private Dictionary<ObjectType, Sprite> iconDictionary;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Construir el diccionario
        BuildDictionary();
    }
    
    /// <summary>
    /// Construye el diccionario de iconos para búsqueda rápida
    /// </summary>
    private void BuildDictionary()
    {
        iconDictionary = new Dictionary<ObjectType, Sprite>();
        
        foreach (var data in objectIcons)
        {
            if (!iconDictionary.ContainsKey(data.objectType))
            {
                iconDictionary.Add(data.objectType, data.icon);
            }
        }
    }
    
    /// <summary>
    /// Obtiene el sprite/icono para un tipo de objeto
    /// </summary>
    public Sprite GetIconForObjectType(ObjectType objectType)
    {
        if (iconDictionary != null && iconDictionary.ContainsKey(objectType))
        {
            return iconDictionary[objectType];
        }
        
        Debug.LogWarning($"No se encontró icono para el objeto: {objectType}");
        return null;
    }
}

/// <summary>
/// Datos de un objeto: tipo y su icono correspondiente
/// </summary>
[System.Serializable]
public class ObjectIconData
{
    [Tooltip("Tipo de objeto")]
    public ObjectType objectType;
    
    [Tooltip("Sprite que representa este objeto en el UI")]
    public Sprite icon;
}
