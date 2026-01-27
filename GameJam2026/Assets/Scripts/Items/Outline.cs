using UnityEngine;

public class Outline : MonoBehaviour
{
    //if player is looking the object, outline it
    public Material outlineMaterial;
    private Material[][] originalMaterials;
    private MeshRenderer[] objectRenderers;
    
    void Start()
    {
        // Obtener todos los MeshRenderers (incluidos los de hijos)
        objectRenderers = GetComponentsInChildren<MeshRenderer>();
        originalMaterials = new Material[objectRenderers.Length][];
        
        // Guardar todos los materiales originales de cada renderer
        for (int i = 0; i < objectRenderers.Length; i++)
        {
            originalMaterials[i] = objectRenderers[i].materials;
        }
    }

    public void EnableOutline()
    {
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
        // Restaurar los materiales originales en todos los renderers
        for (int i = 0; i < objectRenderers.Length; i++)
        {
            objectRenderers[i].materials = originalMaterials[i];
        }
    }
}