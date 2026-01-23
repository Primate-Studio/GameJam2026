using UnityEngine;

[CreateAssetMenu(fileName = "New Requirement", menuName = "Shop/Requirement")]
public class RequirementData : ScriptableObject
{
    public enum RequirementType { Monster, Condition, Environment }

    public ObjectType[] Good;
    public ObjectType[] Mid;
    public ObjectType[] Bad;

    public RequirementType type;
    public string requirementName;
}
