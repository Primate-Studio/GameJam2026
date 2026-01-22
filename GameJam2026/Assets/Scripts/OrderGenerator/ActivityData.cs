using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu( fileName = "New Activity", menuName = "Shop/Activity" )]
public class ActivityData : ScriptableObject
{
    public string activityName;
    
    [Header("Pools (3 of each)")]
    public List<RequirementData> monsters;
    public List<RequirementData> conditions;
    public List<RequirementData> environments;

    /// <summary>
    /// Gets a random combination of monster, condition, and environment from the pools.
    /// </summary>
    public void GetRandomCombo(out RequirementData monster, out RequirementData condition, out RequirementData environment)
    {
        monster = monsters[Random.Range(0, monsters.Count)];
        condition = conditions[Random.Range(0, conditions.Count)];
        environment = environments[Random.Range(0, environments.Count)];
    }
}
