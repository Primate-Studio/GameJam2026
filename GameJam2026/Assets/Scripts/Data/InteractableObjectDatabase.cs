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

    public Sprite GetIconForObjectType(ObjectType objectType)
    {
        if (iconDictionary != null && iconDictionary.TryGetValue(objectType, out Sprite icon))
        {
            return icon;
        }
        
        Debug.LogWarning($"No se encontró icono para el objeto: {objectType}");
        return null;
    }

    public Sprite GetEnvironmentIcon(ObjectType objectType)
    {

        Debug.LogWarning($"No se encontró icono Environment para el objeto: {objectType}");
        return null;
    }

    public Sprite GetConditionIcon(ObjectType objectType)
    {

        Debug.LogWarning($"No se encontró icono Condition para el objeto: {objectType}");
        return null;
    }

    public Sprite GetMonsterIcon(ObjectType objectType)
    {

        Debug.LogWarning($"No se encontró icono Monster para el objeto: {objectType}");
        return null;
    }
    

/// </summary>
[System.Serializable]
public class EnvironmentObjectIconData
{
    [Tooltip("Tipo de objeto Environment")]
    public ObjectType objectType;
    
    [Tooltip("Sprite que representa este objeto Environment en el UI")]
    public Sprite icon;
}

/// <summary>
/// Datos de un objeto de tipo Object
/// </summary>
[System.Serializable]
public class ObjectIconData
{
    [Tooltip("Tipo de objeto")]
    public ObjectType objectType;
    
    [Tooltip("Sprite que representa este objeto en el UI")]
    public Sprite icon;
}

}



    

