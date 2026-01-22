using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Sistema central de gestión de pedidos
/// Controla la generación, entrega y validación de pedidos de clientes
/// </summary>
public class OrderSystem : MonoBehaviour
{
    public static OrderSystem Instance { get; private set; }
    
    [Header("Spawn Positions")]
    [Tooltip("Posiciones donde se instanciarán las cajas de pedido (máximo 3)")]
    public Transform spawnBox1;
    public Transform spawnBox2;
    public Transform spawnBox3;
    
    [Header("Prefabs")]
    [Tooltip("Prefab de la caja donde se entregan los objetos")]
    public GameObject deliveryBoxPrefab;
    
    [Header("Order Configuration")]
    [Tooltip("Packs disponibles para generar pedidos")]
    [SerializeField] private PackData[] availablePacks;
    
    [Tooltip("Todas las ItemData del juego para verificar compatibilidad")]
    public ItemData[] allItems;
    
    [Header("Failure Rates (Configurable)")]
    [Tooltip("% de fallo si entregas 3/3 objetos correctos")]
    [Range(0, 100)] public float failRate_3of3 = 5f;
    
    [Tooltip("% de fallo si entregas 2/3 objetos correctos")]
    [Range(0, 100)] public float failRate_2of3 = 50f;
    
    [Tooltip("% de fallo si entregas 1/3 objetos correctos")]
    [Range(0, 100)] public float failRate_1of3 = 83f;
    
    [Tooltip("% de fallo si entregas 2/2 objetos correctos")]
    [Range(0, 100)] public float failRate_2of2 = 5f;
    
    [Tooltip("% de fallo si entregas 1/2 objetos correctos")]
    [Range(0, 100)] public float failRate_1of2 = 50f;
    
    [Header("Time Penalties (Configurable)")]
    [Tooltip("Penalización por Nerviosismo")]
    [Range(0, 100)] public float penalty_Nervioso = 5f;
    
    [Tooltip("Penalización por Impaciencia")]
    [Range(0, 100)] public float penalty_Impaciente = 15f;
    
    [Tooltip("Penalización por Desesperación")]
    [Range(0, 100)] public float penalty_Desesperado = 45f;
    
    [Tooltip("Penalización por Abandono")]
    [Range(0, 100)] public float penalty_Abandonado = 95f;
    
    [Header("Active Orders")]
    [SerializeField] private List<Order> activeOrders = new List<Order>();
    private List<GameObject> activeBoxes = new List<GameObject>();
    
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
        // Generar pedido al presionar ESPACIO
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryGenerateOrder();
        }
        
        // TODO: Actualizar timers de pedidos activos
        // Tu compañero se encargará de esto
        // UpdateOrderTimers();
    }
    
    /// <summary>
    /// Intenta generar un nuevo pedido si hay espacio disponible
    /// </summary>
    public void TryGenerateOrder()
    {
        // Verificar si hay espacio (máximo 3 pedidos)
        if (activeOrders.Count >= 3)
        {
            Debug.LogWarning("<color=orange>⚠️ No hay espacio para más pedidos! (Máximo 3)</color>");
            return;
        }
        
        // Generar el pedido
        Order newOrder = GenerateRandomOrder();
        
        // Obtener posición de spawn disponible
        Transform spawnPos = GetAvailableSpawnPosition();
        
        if (spawnPos == null)
        {
            Debug.LogError("No hay posición de spawn disponible!");
            return;
        }
        
        // Instanciar la caja de entrega
        GameObject box = Instantiate(deliveryBoxPrefab, spawnPos.position, spawnPos.rotation);
        DeliveryBox deliveryBox = box.GetComponent<DeliveryBox>();
        
        if (deliveryBox != null)
        {
            deliveryBox.AssignOrder(newOrder);
        }
        
        // Añadir a las listas
        activeOrders.Add(newOrder);
        activeBoxes.Add(box);
        
        Debug.Log($"<color=green>✓ Pedido #{newOrder.orderID} generado!</color>");
        
        // Mostrar solo los requisitos activos
        string requirements = $"{newOrder.monster.requirementName} | {newOrder.condition.requirementName}";
        if (newOrder.environment != null)
        {
            requirements += $" | {newOrder.environment.requirementName}";
        }
        Debug.Log($"<color=cyan>Requisitos ({newOrder.itemsNeeded}): {requirements}</color>");
        Debug.Log($"<color=yellow>Objetos necesarios: {newOrder.itemsNeeded}</color>");
    }
    
    /// <summary>
    /// Genera un pedido aleatorio con requisitos y objetos necesarios
    /// El número de requisitos (2 o 3) determina cuántos objetos se necesitan
    /// </summary>
    private Order GenerateRandomOrder()
    {
        Order order = new Order();
        order.orderID = nextOrderID++;
        
        // Generar requisitos aleatorios
        PackData selectedPack = availablePacks[Random.Range(0, availablePacks.Length)];
        ActivityData selectedActivity = selectedPack.activities[Random.Range(0, selectedPack.activities.Length)];
        
        // Decidir aleatoriamente cuántos requisitos (2 o 3)
        int requiredCount = Random.Range(2, 4); // 2 o 3
        
        // Generar todos los requisitos primero
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
        
        return order;
    }
    
    /// <summary>
    /// Obtiene la primera posición de spawn disponible
    /// </summary>
    private Transform GetAvailableSpawnPosition()
    {
        if (activeOrders.Count == 0) return spawnBox1;
        if (activeOrders.Count == 1) return spawnBox2;
        if (activeOrders.Count == 2) return spawnBox3;
        return null;
    }
    
    /// <summary>
    /// Intenta añadir un objeto a un pedido específico
    /// </summary>
    public bool TryDeliverItem(Order order, ObjectType itemType)
    {
        // Verificar si el pedido ya está completo
        if (order.IsComplete())
        {
            Debug.LogWarning($"<color=orange>⚠️ El pedido #{order.orderID} ya está completo!</color>");
            return false;
        }
        
        // Añadir el objeto
        order.deliveredItems.Add(itemType);
        Debug.Log($"<color=green>✓ Objeto {itemType} añadido al pedido #{order.orderID} ({order.deliveredItems.Count}/{order.itemsNeeded})</color>");
        
        // Si el pedido está completo, procesarlo
        if (order.IsComplete())
        {
            ProcessCompletedOrder(order);
        }
        
        return true;
    }
    
    /// <summary>
    /// Procesa un pedido completado y determina si el NPC sobrevive
    /// </summary>
    private void ProcessCompletedOrder(Order order)
    {
        Debug.Log($"<color=yellow>📦 Pedido #{order.orderID} completado! Procesando...</color>");
        
        // Calcular objetos correctos
        int correctItems = order.GetCorrectItemsCount(allItems);
        
        // Calcular % de fallo base según objetos correctos
        float baseFailRate = CalculateBaseFailRate(correctItems, order.itemsNeeded);
        
        // TODO: Calcular penalización por tiempo (tu compañero lo hará)
        // float timePenalty = CalculateTimePenalty(order);
        float timePenalty = 0f; // Por ahora sin penalización de tiempo
        
        // % de fallo total
        float totalFailRate = baseFailRate + timePenalty;
        
        // Calcular % de supervivencia
        float survivalRate = 100f - totalFailRate;
        
        Debug.Log($"<color=cyan>📊 Objetos correctos: {correctItems}/{order.itemsNeeded}</color>");
        Debug.Log($"<color=cyan>📊 % Fallo base: {baseFailRate}%</color>");
        Debug.Log($"<color=cyan>📊 Penalización tiempo: {timePenalty}%</color>");
        Debug.Log($"<color=cyan>📊 % Supervivencia: {survivalRate}%</color>");
        
        // Esperar 20 segundos y determinar resultado
        StartCoroutine(DetermineOrderOutcome(order, survivalRate));
    }
    
    /// <summary>
    /// Calcula el % de fallo base según objetos correctos/totales
    /// </summary>
    private float CalculateBaseFailRate(int correct, int total)
    {
        if (total == 3)
        {
            if (correct == 3) return failRate_3of3;
            if (correct == 2) return failRate_2of3;
            if (correct == 1) return failRate_1of3;
            return 100f; // 0/3 = muerte segura
        }
        else if (total == 2)
        {
            if (correct == 2) return failRate_2of2;
            if (correct == 1) return failRate_1of2;
            return 100f; // 0/2 = muerte segura
        }
        
        return 100f;
    }
    
    /// <summary>
    /// TODO: Calcula la penalización por tiempo según el estado del pedido
    /// Tu compañero implementará esto cuando añada el sistema de timers
    /// </summary>
    private float CalculateTimePenalty(Order order)
    {
        switch (order.state)
        {
            case OrderState.Tranquilo:
                return 0f;
            case OrderState.Nervioso:
                return penalty_Nervioso;
            case OrderState.Impaciente:
                return penalty_Impaciente;
            case OrderState.Desesperado:
                return penalty_Desesperado;
            case OrderState.Abandonado:
                return penalty_Abandonado;
            default:
                return 0f;
        }
    }
    
    /// <summary>
    /// Espera 20 segundos y determina si el NPC sobrevive o muere
    /// </summary>
    private IEnumerator DetermineOrderOutcome(Order order, float survivalRate)
    {
        yield return new WaitForSeconds(20f);
        
        // Tirar dado
        float randomValue = Random.Range(0f, 100f);
        bool survived = randomValue < survivalRate;
        
        if (survived)
        {
            Debug.Log($"<color=green>✓ NPC del pedido #{order.orderID} ha SOBREVIVIDO! (Winrate: {survivalRate:F1}%)</color>");
        }
        else
        {
            Debug.Log($"<color=red>✗ NPC del pedido #{order.orderID} ha MUERTO... (Winrate: {survivalRate:F1}%)</color>");
        }
        
        // Remover el pedido del sistema
        RemoveOrder(order);
    }
    
    /// <summary>
    /// Remueve un pedido del sistema y destruye su caja
    /// </summary>
    private void RemoveOrder(Order order)
    {
        int index = activeOrders.IndexOf(order);
        
        if (index >= 0)
        {
            activeOrders.RemoveAt(index);
            
            if (index < activeBoxes.Count)
            {
                Destroy(activeBoxes[index]);
                activeBoxes.RemoveAt(index);
                ClientManager.Instance.DismissClientInSlot(index);
            }
        }
    }
    
    /// <summary>
    /// TODO: Actualiza los timers de todos los pedidos activos
    /// Tu compañero implementará esto
    /// </summary>
    private void UpdateOrderTimers()
    {
        // foreach (Order order in activeOrders)
        // {
        //     order.timeElapsed += Time.deltaTime;
        //     
        //     // Actualizar estado según tiempo
        //     float timeRemaining = order.maxTime - order.timeElapsed;
        //     
        //     if (timeRemaining <= 0)
        //         order.state = OrderState.Abandonado;
        //     else if (timeRemaining <= 6)
        //         order.state = OrderState.Desesperado;
        //     else if (timeRemaining <= 18)
        //         order.state = OrderState.Impaciente;
        //     else if (timeRemaining <= 36)
        //         order.state = OrderState.Nervioso;
        //     else
        //         order.state = OrderState.Tranquilo;
        // }
    }
}
