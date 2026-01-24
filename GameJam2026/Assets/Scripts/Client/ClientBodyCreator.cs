using UnityEngine;

public class ClientBodyCreator : MonoBehaviour
{
    [Header("Contenidors de la jerarquia FBX")]
    public Transform headsParent;
    public Transform facesParent;
    public Transform clothesParent;
    public Transform accessoriesParent;
    public Transform bodyParent;
    public Material skinMat;
    public Material hairMat;
    public Color[] RandomSkinColors;
    public Color[] RandomHairColors;

    public void ApplyRandomLook()
    {
        // Randomitzem cada categoria
        ApplyRandomColor();
        RandomizeChild(headsParent);
        RandomizeChild(facesParent);
        RandomizeChild(clothesParent);
        RandomizeChild(accessoriesParent);
    }

    private void RandomizeChild(Transform parent)
    {
        if (parent == null || parent.childCount == 0) return;

        // 1. Triem un índex a l'atzar
        int randomIndex = Random.Range(0, parent.childCount);

        // 2. Recorrem tots els fills (objectes del fbx) i només activem el triat
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.SetActive(i == randomIndex);
        }
    }
    
    private void ApplyRandomColor()
    {
        if (RandomSkinColors != null && RandomSkinColors.Length > 0)
        {
            var skinColor = RandomSkinColors[Random.Range(0, RandomSkinColors.Length)];
            ApplyColorToRenderers(bodyParent, skinColor, skinMat);
            ApplyColorToRenderers(facesParent, skinColor, skinMat);

        }

        if (RandomHairColors != null && RandomHairColors.Length > 0)
        {
            var hairColor = RandomHairColors[Random.Range(0, RandomHairColors.Length)];
            ApplyColorToRenderers(headsParent, hairColor, hairMat);
            ApplyColorToRenderers(facesParent, hairColor, hairMat);
        }
    }

    // Instancia material solo si el renderer tiene el material template asignado
    private void ApplyColorToRenderers(Transform parent, Color color, Material template)
    {
        if (parent == null || template == null) return;

        var renderers = parent.GetComponentsInChildren<Renderer>(false); // solo activos
        foreach (var r in renderers)
        {
            if (r == null) continue;

            // Solo aplicar si el renderer tiene el material plantilla asignado
            if (r.sharedMaterial == template)
            {
                var instancedMat = new Material(template);
                instancedMat.color = color;
                r.material = instancedMat; // instancia única para este renderer
            }
        }
    }
}