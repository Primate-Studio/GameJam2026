using UnityEngine;

public class OrderGenerator : MonoBehaviour
{
    public static OrderGenerator Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private PackData[] availablePacks;

    [Header("Actual State")]
    public RequirementData currentMonster;
    public RequirementData currentCondition;
    public RequirementData currentEnvironment;
    
    private int nextOrderID = 1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateNewClientOrder();
        }
    }
    
    /// <summary>
    /// Genera un pedido aleatorio con requisitos y objetos necesarios
    /// El número de requisitos (2 o 3) determina cuántos objetos se necesitan
    /// </summary>
    public Order GenerateNewClientOrder()
    {

        //if (!TutorialManager.Instance.canGenerateOrder) return null;
        Order order = new Order();
        order.orderID = nextOrderID++;
        
        int requiredCount = Random.Range(2, 4); // 2 o 3 objetos

        int activePacks, activeActivities;
        CalculateActivities(out activePacks, out activeActivities);
        // Random Pack
        PackData selectedPack = availablePacks[activePacks > 1 ? Random.Range(0, activePacks) : 0];

        ActivityData selectedActivity;
        // Random Activity from Pack
        if(activeActivities == 0 && selectedPack.name == "Pack1")
        {
            selectedActivity = selectedPack.activities[Random.Range(0, 1)];
        }
        else if(activeActivities == 0 && selectedPack.name == "Pack2")
        {
            selectedActivity = selectedPack.activities[0];
        }
        else {selectedActivity = selectedPack.activities[Random.Range(0, activeActivities)];}
        

        // Random Combo from Activity
        RequirementData tempMonster, tempCondition, tempEnvironment;
        selectedActivity.GetRandomCombo(out tempMonster, out tempCondition, out tempEnvironment);
        
        if (requiredCount == 2)
        {
            // Solo 2 requisitos: Monster y Condition
            order.monster = tempMonster;
            order.condition = tempCondition;
            order.environment = null; // Sin requisito de Environment
        }
        else // requiredCount == 3
        {
            // 3 requisitos: Monster, Condition y Environment
            order.monster = tempMonster;
            order.condition = tempCondition;
            order.environment = tempEnvironment;
        }
        
        // Los objetos necesarios = número de requisitos
        order.itemsNeeded = requiredCount;
        
        // Actualizar variables para debug visual
        currentMonster = order.monster;
        currentCondition = order.condition;
        currentEnvironment = order.environment;

        Debug.Log($"New Order Generated: Monster - {order.monster.requirementName}, Condition - {order.condition.requirementName}, Environment - {(order.environment != null ? order.environment.requirementName : "None")} ");
        Debug.Log($"(From Pack: {selectedPack.packName}, Activity: {selectedActivity.activityName})");
        
        return order;
    }

    public Order GenerateSpecificOrder(RequirementData monster, RequirementData condition, RequirementData environment)
    {
        Order order = new Order();
        order.orderID = nextOrderID++;
        order.monster = monster;
        order.condition = condition;
        order.environment = environment;

        // Determinar cuántos objetos se necesitan según los requisitos dados
        int requiredCount = 0;
        if (monster != null) requiredCount++;
        if (condition != null) requiredCount++;
        if (environment != null) requiredCount++;

        order.itemsNeeded = requiredCount;

        // Actualizar variables para debug visual
        currentMonster = order.monster;
        currentCondition = order.condition;
        currentEnvironment = order.environment;

        Debug.Log($"Specific Order Generated: Monster - {order.monster.requirementName}, Condition - {order.condition.requirementName}, Environment - {(order.environment != null ? order.environment.requirementName : "None")} ");

        return order;
    }

    private void CalculateActivities(out int activePacks, out int activeActivities)
    {
        MoneyManager.Instance.CalculateDebtLevel();

        if (MoneyManager.Instance.DebtLevel == DebtLevel.High)
        {
            activePacks = 1;
            activeActivities = 2;
        }
        else if (MoneyManager.Instance.DebtLevel == DebtLevel.Medium)
        {
            activePacks = 2;
            activeActivities = 0;
        }
        else if (MoneyManager.Instance.DebtLevel == DebtLevel.Low)
        {
            activePacks = 2;
            activeActivities = 2;
        }
        else if (MoneyManager.Instance.DebtLevel == DebtLevel.LowLow)
        {
            activePacks = 2;
            activeActivities = 3;
        }
        else if (MoneyManager.Instance.DebtLevel == DebtLevel.None)
        {
            activePacks = 2;
            activeActivities = 4;
        }
        else
        {
            activePacks = 1;
            activeActivities = 2;
        }
    }
}
