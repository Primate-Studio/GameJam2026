using UnityEngine;

public class Outline : MonoBehaviour
{
    //if player is looking the object, outline it
    public Material outlineMaterial;
    private Material[][] originalMaterials;
    private Renderer[] objectRenderers;
    private bool isInitialized = false;
    
    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;
        
        // Obtener todos los Renderers (incluidos los de hijos)
        objectRenderers = GetComponentsInChildren<Renderer>();
        
        if (objectRenderers.Length == 0)
        {
            Debug.LogWarning($"<color=red>No Renderers encontrados en {gameObject.name}</color>");
            return;
        }
        
        originalMaterials = new Material[objectRenderers.Length][];
        
        // Guardar todos los materiales originales de cada renderer
        for (int i = 0; i < objectRenderers.Length; i++)
        {
            originalMaterials[i] = objectRenderers[i].materials;
            Debug.Log($"Renderer encontrado: {objectRenderers[i].gameObject.name}");
        }
        
        isInitialized = true;
        Debug.Log($"<color=green>Outline inicializado para {gameObject.name}</color>");
    }

    public void EnableOutline()
    {
        Initialize(); // Asegurar que esté inicializado
        
        if (!isInitialized) return;
        
        for (int i = 0; i < objectRenderers.Length; i++)
        {
            // Crear un nuevo array con espacio para un material extra
            Material[] mats = new Material[originalMaterials[i].Length + 1];
            
            // Copiar todos los materiales originales
            for (int j = 0; j < originalMaterials[i].Length; j++)
            {
                mats[j] = originalMaterials[i][j];
            }
            
            // Añadir el outline al final
            mats[originalMaterials[i].Length] = outlineMaterial;
            objectRenderers[i].materials = mats;
        }
    }

    public void DisableOutline()
    {
        Initialize();
        
        if (!isInitialized) return;
        
        // Restaurar los materiales originales en todos los renderers
        for (int i = 0; i < objectRenderers.Length; i++)
        {
            objectRenderers[i].materials = originalMaterials[i];
        }
    }
}