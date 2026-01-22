using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Shop/Item")]
public class ItemData : ScriptableObject
{
    public ObjectType type;
    
    [Header("Works With")]
    public RequirementData[] compatibleMonsters;
    public RequirementData[] compatibleConditions;
    public RequirementData[] compatibleEnvironments;

    [TextArea]
    public string itemDescription;
}
