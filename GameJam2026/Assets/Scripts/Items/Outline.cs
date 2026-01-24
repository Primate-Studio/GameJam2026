using UnityEngine;

public class Outline : MonoBehaviour
{
    //if player is looking the object, outline it
    public Material outlineMaterial;
    private Material originalMaterial;
    private MeshRenderer objectRenderer;
    void Start()
    {
        objectRenderer = GetComponent<MeshRenderer>();
        originalMaterial = objectRenderer.materials[0]; 
    }

    public void EnableOutline()
    {
        //add outline material to the object
        if(objectRenderer.materials.Length < 2) 
        {
            Material[] mats = new Material[2];
            mats[0] = originalMaterial;
            mats[1] = outlineMaterial;
            objectRenderer.materials = mats;
        }
        else
        {
            objectRenderer.materials[1] = outlineMaterial;
        }
    }

    public void DisableOutline()
    {
        //remove outline material from the object
        if(objectRenderer.materials.Length >= 2) 
        {
            Material[] mats = new Material[1];
            mats[0] = originalMaterial;
            objectRenderer.materials = mats;
        }
    }
}