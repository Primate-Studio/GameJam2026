using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ClientVisualPool", menuName = "Shop/Visual Pool")]
public class ClientVisualPool : ScriptableObject
{
    [Header("Variants Pool")]
    //get them from a folder
    public GameObject[] headAccessories;
    public GameObject[] faces;
    public GameObject[] clothes;
    public GameObject[] bodyAccessories;

    void OnEnable()
    {
        headAccessories = Resources.LoadAll<GameObject>("Prefabs/Client/HeadAccessories");
        faces = Resources.LoadAll<GameObject>("Prefabs/Client/Faces");
        clothes = Resources.LoadAll<GameObject>("Prefabs/Client/Clothes");
        bodyAccessories = Resources.LoadAll<GameObject>("Prefabs/Client/BodyAccessories");
        if (headAccessories.Length == 0 || faces.Length == 0 || clothes.Length == 0 || bodyAccessories.Length == 0)
        {
            Debug.LogWarning("One or more client visual pools are empty!");
        }
    }

}
