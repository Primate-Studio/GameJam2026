using UnityEngine;

public class ClientBodyCreator : MonoBehaviour
{
    [Header("Contenidors de la jerarquia FBX")]
    public Transform headsParent;
    public Transform facesParent;
    public Transform clothesParent;
    public Transform accessoriesParent;

    public void ApplyRandomLook()
    {
        // Randomitzem cada categoria
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
    
    // Per testetjar (opcional)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ApplyRandomLook();
    }
}