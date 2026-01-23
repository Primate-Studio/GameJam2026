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
    public class ClientOrderData
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
    [Header("Order Configuration")]
    [Tooltip("Packs disponibles para generar pedidos")]
    [SerializeField] private PackData[] availablePacks;
    [Header("Prefabs")]
    [Tooltip("Prefab de la caja donde se entregan los objetos")]
    public GameObject deliveryBoxPrefab;
    
    
    
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
            OrderEvaluator.Instance.ProcessCompletedOrder(clientOrderData);
        }
        
        return true;
    }
    
   
    
    /// <summary>
    /// Determina inmediatamente si el NPC sobrevive o muere
    /// </summary>
    public void DetermineOrderOutcomeImmediate(ClientOrderData clientOrderData, float survivalRate)
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
                        OrderEvaluator.Instance.ProcessCompletedOrder(clientOrderData);
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
