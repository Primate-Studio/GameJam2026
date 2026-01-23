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
        Order order = new Order();
        order.orderID = nextOrderID++;
        
        int requiredCount = Random.Range(2, 4); // 2 o 3 objetos
        
        // Random Pack
        PackData selectedPack = availablePacks[Random.Range(0, availablePacks.Length)];

        // Random Activity from Pack
        ActivityData selectedActivity = selectedPack.activities[Random.Range(0, selectedPack.activities.Length)];

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
}
