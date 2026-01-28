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
        // Deshabilitar generación manual en tutorial
        if (GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            return;
        }

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
        if (GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            return null;
        }
        //if (!TutorialManager.Instance.canGenerateOrder) return null;
        Order order = new Order();
        order.orderID = nextOrderID++;
        
        int requiredCount = Random.Range(2, 4); // 2 o 3 objetos

        int activePacks, activeActivities1, activeActivities2;
        CalculateActivities(out activePacks, out activeActivities1, out activeActivities2);
        // Random Pack
        PackData selectedPack = availablePacks[activePacks > 1 ? Random.Range(0, activePacks) : 0];

        ActivityData selectedActivity;
        // Random Activity from Pack
        if(activeActivities2 == 2 && selectedPack.name == "Pack2")
        {
            selectedActivity = selectedPack.activities[Random.Range(0, activeActivities2)];
        }
        else if(activeActivities2 == 3 && selectedPack.name == "Pack2")
        {
            selectedActivity = selectedPack.activities[Random.Range(0, activeActivities2)];
        }
        else {selectedActivity = selectedPack.activities[Random.Range(0, activeActivities1)];}
        

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

    private void CalculateActivities(out int activePacks, out int activeActivities1, out int activeActivities2)
    {
        MoneyManager.Instance.CalculateDebtLevel();

        if (MoneyManager.Instance.DebtLevel == DebtLevel.High)
        {
            activePacks = 1;
            activeActivities1 = 1;
            activeActivities2 = 0;
        }
        else if (MoneyManager.Instance.DebtLevel == DebtLevel.Medium)
        {
            activePacks = 1;
            activeActivities1 = 2;
            activeActivities2 = 0;
        }
        else if (MoneyManager.Instance.DebtLevel == DebtLevel.Low)
        {
            activePacks = 1;
            activeActivities1 = 3;
            activeActivities2 = 0;
        }
        else if (MoneyManager.Instance.DebtLevel == DebtLevel.LowLow)
        {
            activePacks = 1;
            activeActivities1 = 4;
            activeActivities2 = 0;
        }
        else if(MoneyManager.Instance.isEternalMode)
        {
            activePacks = 2;
            activeActivities1 = 4;
            activeActivities2 = 4;
        }
        else
        {
            activePacks = 1;
            activeActivities1 = 2;
            activeActivities2 = 0;
        }
    }
}
