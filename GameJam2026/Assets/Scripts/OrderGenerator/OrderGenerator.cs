using UnityEngine;

public class OrderGenerator : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PackData[] availablePacks;

    [Header("Actual State")]
    public RequirementData currentMonster;
    public RequirementData currentCondition;
    public RequirementData currentEnvironment;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateNewClientOrder();
        }
    }
    public void GenerateNewClientOrder()
    {
        // Random Pack
        PackData selectedPack = availablePacks[Random.Range(0, availablePacks.Length)];

        // Random Activity from Pack
        ActivityData selectedActivity = selectedPack.activities[Random.Range(0, selectedPack.activities.Length)];

        // Random Combo from Activity
        selectedActivity.GetRandomCombo(out currentMonster, out currentCondition, out currentEnvironment);

        Debug.Log($"New Order Generated: Monster - {currentMonster.requirementName}, Condition - {currentCondition.requirementName}, Environment - {currentEnvironment.requirementName} ");
        Debug.Log($"(From Pack: {selectedPack.packName}, Activity: {selectedActivity.activityName})");
    }
}
