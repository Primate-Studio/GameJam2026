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
    
    // Clase para vincular cliente, order y box
    [System.Serializable]
    private class ClientOrderData
    {
        public GameObject client;
        public Order order;
        public GameObject box;
        public ClientTimer clientTimer;
        public int slotIndex;
    }
    
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
    [SerializeField] private List<ClientOrderData> activeClientOrders = new List<ClientOrderData>();
    
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
    
    void Start()
    {
        // Suscribirse a la llegada de clientes desde ClientManager
        if (ClientManager.Instance != null)
        {
            // El ClientManager debe llamar a GenerateOrderForClient cuando un cliente llega
            Debug.Log("<color=cyan>OrderSystem listo para recibir clientes</color>");
        }
    }
    
    void Update()
    {
        // Actualizar timers de pedidos activos y verificar si expiran
        UpdateOrderTimers();
    }
    
    /// <summary>
    /// Genera un pedido cuando un cliente llega a su posición
    /// Llamado por ClientManager cuando el cliente llega
    /// </summary>
    public void GenerateOrderForClient(GameObject client, int slotIndex)
    {
        // Verificar si hay espacio (máximo 3 pedidos)
        if (activeClientOrders.Count >= 3)
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
        
        // Obtener o crear el ClientTimer
        ClientTimer clientTimer = client.GetComponent<ClientTimer>();
        if (clientTimer == null)
        {
            clientTimer = client.AddComponent<ClientTimer>();
        }
        clientTimer.StartNewOrderTimer();
        
        // Crear el ClientOrderData y añadirlo
        ClientOrderData clientOrderData = new ClientOrderData
        {
            client = client,
            order = newOrder,
            box = box,
            clientTimer = clientTimer,
            slotIndex = slotIndex
        };
        activeClientOrders.Add(clientOrderData);
        
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
        if (activeClientOrders.Count == 0) return spawnBox1;
        if (activeClientOrders.Count == 1) return spawnBox2;
        if (activeClientOrders.Count == 2) return spawnBox3;
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
            // Encontrar el ClientOrderData correspondiente
            ClientOrderData clientOrderData = activeClientOrders.Find(cod => cod.order == order);
            ProcessCompletedOrder(clientOrderData);
        }
        
        return true;
    }
    
    /// <summary>
    /// Procesa un pedido completado y determina si el NPC sobrevive
    /// </summary>
    private void ProcessCompletedOrder(ClientOrderData clientOrderData)
    {
        if (clientOrderData == null) return;
        
        Order order = clientOrderData.order;
        Debug.Log($"<color=yellow>📦 Pedido #{order.orderID} completado! Procesando...</color>");
        
        // Calcular objetos correctos
        int correctItems = order.GetCorrectItemsCount(allItems);
        
        // Calcular % de fallo base según objetos correctos
        float baseFailRate = CalculateBaseFailRate(correctItems, order.itemsNeeded);
        
        // Calcular penalización por tiempo usando el ClientTimer
        float timePenalty = CalculateTimePenalty(clientOrderData.clientTimer);
        
        // % de fallo total
        float totalFailRate = baseFailRate + timePenalty;
        
        // Calcular % de supervivencia
        float survivalRate = 100f - totalFailRate;
        
        Debug.Log($"<color=cyan>📊 Objetos correctos: {correctItems}/{order.itemsNeeded}</color>");
        Debug.Log($"<color=cyan>📊 % Fallo base: {baseFailRate}%</color>");
        Debug.Log($"<color=cyan>📊 Penalización tiempo: {timePenalty}%</color>");
        Debug.Log($"<color=cyan>📊 % Supervivencia: {survivalRate}%</color>");
        
        // Determinar resultado inmediatamente (ya no esperamos 20 segundos)
        DetermineOrderOutcomeImmediate(clientOrderData, survivalRate);
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
    /// Calcula la penalización por tiempo según el ClientTimer
    /// </summary>
    private float CalculateTimePenalty(ClientTimer clientTimer)
    {
        if (clientTimer == null) return 0f;
        
        // Obtener el nivel de desesperación del ClientTimer a través de reflexión
        // (porque desperationLevel es privado)
        float timePercentage = clientTimer.GetComponent<ClientTimer>() != null ? 
            (clientTimer.timeRemaining / clientTimer.orderDuration) : 1f;
        
        // Calcular penalización basada en el porcentaje de tiempo restante
        if (timePercentage >= 0.60f)
        {
            return 0f; // None
        }
        else if (timePercentage >= 0.30f && timePercentage < 0.60f)
        {
            return penalty_Nervioso; // Low
        }
        else if (timePercentage >= 0.10f && timePercentage < 0.30f)
        {
            return penalty_Impaciente; // Medium
        }
        else if (timePercentage > 0f && timePercentage < 0.10f)
        {
            return penalty_Desesperado; // High
        }
        else
        {
            return penalty_Abandonado; // Abandon
        }
    }
    
    /// <summary>
    /// Determina inmediatamente si el NPC sobrevive o muere
    /// </summary>
    private void DetermineOrderOutcomeImmediate(ClientOrderData clientOrderData, float survivalRate)
    {
        Order order = clientOrderData.order;
        
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
        RemoveOrder(clientOrderData);
    }
    
    /// <summary>
    /// Remueve un pedido del sistema, destruye su caja y despide al cliente
    /// </summary>
    private void RemoveOrder(ClientOrderData clientOrderData)
    {
        if (clientOrderData == null) return;
        
        // Destruir la caja
        if (clientOrderData.box != null)
        {
            Destroy(clientOrderData.box);
        }
        
        // Despedir al cliente
        if (ClientManager.Instance != null)
        {
            ClientManager.Instance.DismissClientInSlot(clientOrderData.slotIndex);
        }
        
        // Remover de la lista activa
        activeClientOrders.Remove(clientOrderData);
    }
    
    /// <summary>
    /// Actualiza los timers de todos los pedidos activos y verifica si expiran
    /// </summary>
    private void UpdateOrderTimers()
    {
        // Verificar si algún timer ha expirado
        for (int i = activeClientOrders.Count - 1; i >= 0; i--)
        {
            ClientOrderData clientOrderData = activeClientOrders[i];
            
            if (clientOrderData.clientTimer != null)
            {
                // Verificar si el tiempo se acabó
                if (clientOrderData.clientTimer.timeRemaining <= 0f)
                {
                    Debug.Log($"<color=red>⏱️ Tiempo agotado para pedido #{clientOrderData.order.orderID}!</color>");
                    
                    // Si hay objetos entregados, calcular resultado parcial
                    if (clientOrderData.order.deliveredItems.Count > 0)
                    {
                        ProcessCompletedOrder(clientOrderData);
                    }
                    else
                    {
                        // Sin objetos = abandono total
                        Debug.Log($"<color=red>✗ NPC del pedido #{clientOrderData.order.orderID} ha ABANDONADO (0 objetos entregados)</color>");
                        RemoveOrder(clientOrderData);
                    }
                }
            }
        }
    }
}
