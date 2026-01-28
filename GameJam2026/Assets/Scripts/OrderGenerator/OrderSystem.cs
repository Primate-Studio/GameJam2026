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
        public GameObject uiElement;
        public GameObject bubbleElement;
    }
    
    [Header("Spawn Positions")]
    [Tooltip("Posiciones donde se instanciarán las cajas de pedido (máximo 3)")]
    public Transform spawnBox1;
    public Transform spawnBox2;
    public Transform spawnBox3;
    [Header("Prefabs")]
    [Tooltip("Prefab de la caja donde se entregan los objetos")]
    public GameObject deliveryBoxPrefab;
    
    [Header("Active Orders")]
    [SerializeField] private List<ClientOrderData> activeClientOrders = new List<ClientOrderData>();

    public Transform uiContainer;
    public GameObject orderUIPrefab;
    public GameObject orderBubblePrefab;
    public Vector3 bubbleOffset = new Vector3(0, 2.5f, 0);
    public bool playerInZone = false;
    private Dictionary<GameObject, int> clientsWaitingQueue = new Dictionary<GameObject, int>(); 

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
        
        if (GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            return;
        }

        // Verificar si hay espacio (máximo 3 pedidos)
        if (activeClientOrders.Count >= 3)
        {
            Debug.LogWarning("<color=orange>⚠️ No hay espacio para más pedidos! (Máximo 3)</color>");
            return;
        }
        
        // Verificar si ya hay un pedido para este slot
        if (activeClientOrders.Exists(cod => cod.slotIndex == slotIndex))
        {
            Debug.LogWarning($"<color=orange>⚠️ Ya existe un pedido activo para el slot {slotIndex}!</color>");
            return;
        }
        
        // Generar el pedido usando el OrderGenerator
        if (OrderGenerator.Instance == null)
        {
            Debug.LogError("<color=red>OrderGenerator.Instance es null! Asegúrate de que hay un OrderGenerator en la escena.</color>");
            return;
        }
        
        Order newOrder = OrderGenerator.Instance.GenerateNewClientOrder();
        ClientAnimationController animationController = client.GetComponentInChildren<ClientAnimationController>(true);
        if (animationController != null)
        {
            newOrder.animationController = animationController;
            StartCoroutine(animationController.SetTalking(true));
        }
        GameObject bubbleObj = Instantiate(orderBubblePrefab, client.transform.position + bubbleOffset, Quaternion.identity, client.transform);
        bubbleObj.SetActive(false);
        bubbleObj.GetComponent<OrderUIItem>().Setup(newOrder, null);
        bubbleObj.SetActive(true);
        StartCoroutine(DisableBubbleAfter(bubbleObj, 10f));
        
        // Usar el slotIndex proporcionado para determinar qué posición de spawn usar
        Transform spawnPos = GetSpawnPositionForSlot(slotIndex);
        
        if (spawnPos == null)
        {
            Debug.LogError($"No hay posición de spawn para el slot {slotIndex}!");
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
        // 1. Fem la "foto" cridant al Photo Booth
        Sprite clientPhoto = PortraitCamera.Instance.TakePortrait(client);

        // 2. Instanciem l'element de la UI al teu contenidor del Canvas
        // Necessites tenir 'public GameObject orderUIPrefab' i 'public Transform uiContainer' a l'OrderSystem
        GameObject uiObj = Instantiate(orderUIPrefab, uiContainer);
        uiObj.GetComponent<OrderUIItem>().Setup(newOrder, clientPhoto);

        UIOrderSlide slideEffect = uiObj.GetComponentInChildren<UIOrderSlide>();
        if (slideEffect != null) slideEffect.StartSlide();

        // 3. Creem el ClientOrderData incloent la UI
        ClientOrderData clientOrderData = new ClientOrderData
        {
            client = client,
            order = newOrder,
            box = box,
            clientTimer = clientTimer,
            slotIndex = slotIndex,
            uiElement = uiObj, 
            bubbleElement = bubbleObj

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
    /// Genera un pedido específico para el tutorial
    /// Similar a GenerateOrderForClient pero usa GenerateSpecificOrder del OrderGenerator
    /// </summary>
    public void GenerateTutorialOrderForClient(GameObject client, int slotIndex, RequirementData monster, RequirementData condition, RequirementData environment = null)
    {
        // Verificar si hay espacio (máximo 3 pedidos)
        if (activeClientOrders.Count >= 3)
        {
            Debug.LogWarning("<color=orange>⚠️ No hay espacio para más pedidos! (Máximo 3)</color>");
            return;
        }
        
        // Verificar si ya hay un pedido para este slot
        if (activeClientOrders.Exists(cod => cod.slotIndex == slotIndex))
        {
            Debug.LogWarning($"<color=orange>⚠️ Ya existe un pedido activo para el slot {slotIndex}!</color>");
            return;
        }
        
        // Generar el pedido específico usando el OrderGenerator
        if (OrderGenerator.Instance == null)
        {
            Debug.LogError("<color=red>OrderGenerator.Instance es null! Asegúrate de que hay un OrderGenerator en la escena.</color>");
            return;
        }
        
        Order newOrder = OrderGenerator.Instance.GenerateSpecificOrder(monster, condition, environment);
        
        ClientAnimationController animationController = client.GetComponentInChildren<ClientAnimationController>(true);
        if (animationController != null)
        {
            newOrder.animationController = animationController;
            StartCoroutine(animationController.SetTalking(true));
        }
        
        GameObject bubbleObj = Instantiate(orderBubblePrefab, client.transform.position + bubbleOffset, Quaternion.identity, client.transform);
        bubbleObj.SetActive(false);
        bubbleObj.GetComponent<OrderUIItem>().Setup(newOrder, null);
        bubbleObj.SetActive(true);
        StartCoroutine(DisableBubbleAfter(bubbleObj, 2f));
        
        // Usar el slotIndex proporcionado para determinar qué posición de spawn usar
        Transform spawnPos = GetSpawnPositionForSlot(slotIndex);
        
        if (spawnPos == null)
        {
            Debug.LogError($"No hay posición de spawn para el slot {slotIndex}!");
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
        
        // Crear el UI del pedido
        Sprite clientPhoto = PortraitCamera.Instance.TakePortrait(client);
        GameObject uiObj = Instantiate(orderUIPrefab, uiContainer);
        uiObj.GetComponent<OrderUIItem>().Setup(newOrder, clientPhoto);
        
        UIOrderSlide slideEffect = uiObj.GetComponentInChildren<UIOrderSlide>();
        if (slideEffect != null) slideEffect.StartSlide();
        
        // Crear el ClientOrderData
        ClientOrderData clientOrderData = new ClientOrderData
        {
            client = client,
            order = newOrder,
            box = box,
            clientTimer = clientTimer,
            slotIndex = slotIndex,
            uiElement = uiObj,
            bubbleElement = bubbleObj
        };
        activeClientOrders.Add(clientOrderData);
        
        Debug.Log($"<color=green>✓ Pedido de tutorial #{newOrder.orderID} generado!</color>");
        
        // Mostrar requisitos
        string requirements = $"{newOrder.monster.requirementName} | {newOrder.condition.requirementName}";
        if (newOrder.environment != null)
        {
            requirements += $" | {newOrder.environment.requirementName}";
        }
        Debug.Log($"<color=cyan>Requisitos ({newOrder.itemsNeeded}): {requirements}</color>");
    }
    private IEnumerator DisableBubbleAfter(GameObject bubble, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bubble != null)
        {
            bubble.SetActive(false);
        }
    }
    public void SetPlayerAtCounter(bool isAtCounter)
    {
        playerInZone = isAtCounter;

        if (playerInZone)
        {
            // Quan el jugador entra, recorrem el diccionari i generem les comandes pendents
            foreach (var entry in clientsWaitingQueue)
            {

                if(GameManager.Instance.CurrentState == GameState.Tutorial && TutorialManager.Instance.isWaitingForFirstClientOrder)
                {
                    GenerateTutorialOrderForClient(entry.Key, entry.Value, TutorialManager.Instance.ciclopeIntellectual, TutorialManager.Instance.estampidaOvejas);
                }else if(GameManager.Instance.CurrentState == GameState.Tutorial && TutorialManager.Instance.isWaitingForSecondClientOrder)
                {
                    GenerateTutorialOrderForClient(entry.Key, entry.Value, TutorialManager.Instance.ciclopeBebe, TutorialManager.Instance.muchoPolvo, TutorialManager.Instance.interiorCueva);
                }
                else
                {
                    GenerateOrderForClient(entry.Key, entry.Value);
                }
            }
            // Un cop generades totes, buidem la cua
            clientsWaitingQueue.Clear();
        }
    }
    public void RegisterClientArrival(GameObject client, int slotIndex)
    {
        if (playerInZone)
        {
            // Si ja hi ets, genera-la al moment
            GenerateOrderForClient(client, slotIndex);
        }
        else
        {
            // Si no hi ets, guarda el client i el seu slot a la cua
            if (!clientsWaitingQueue.ContainsKey(client))
            {
                clientsWaitingQueue.Add(client, slotIndex);
                Debug.Log($"<color=yellow>Slot {slotIndex}: Client esperant que el jugador s'apropi.</color>");
            }
        }
    }
    public void RemoveWaitingClient(GameObject client)
    {
        if (clientsWaitingQueue.ContainsKey(client))
            clientsWaitingQueue.Remove(client);
    }
    
    /// <summary>
    /// Obtiene la posición de spawn correspondiente a un slot específico
    /// </summary>
    private Transform GetSpawnPositionForSlot(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0: return spawnBox1;
            case 1: return spawnBox2;
            case 2: return spawnBox3;
            default: return null;
        }
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
            
            if (clientOrderData != null)
            {
                if (OrderEvaluator.Instance != null)
                {
                    OrderEvaluator.Instance.ProcessCompletedOrder(clientOrderData);
                }
                else
                {
                    Debug.LogError("<color=red>OrderEvaluator.Instance es null! Asegúrate de que hay un OrderEvaluator en la escena.</color>");
                }
            }
            else
            {
                Debug.LogError($"<color=red>No se encontró ClientOrderData para el pedido #{order.orderID}</color>");
            }
        }
        
        return true;
    }
        
    /// <summary>
    /// Determina inmediatamente si el NPC sobrevive o muere
    /// </summary>
    public void DetermineOrderOutcomeImmediate(ClientOrderData clientOrderData, float successRate)
    {
        Order order = clientOrderData.order;
        
        // Normalizar successRate: si está en formato decimal (0-1), convertir a porcentaje (0-100)
        float successPercentage = successRate;
        if (successRate <= 1f)
        {
            successPercentage = successRate * 100f;
        }
        
        // Generar número aleatorio
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        bool clientSurvives = randomValue <= successRate;
        
        if (clientSurvives)
        {
            // Cliente sobrevive
            MoneyManager.Instance.successMoney();
            Debug.Log($"<color=green>✓ NPC del pedido #{order.orderID} ha SOBREVIVIDO! (Winrate: {successPercentage:F1}%)</color>");
        }
        else
        {
            // Cliente muere
            MoneyManager.Instance.deadthPenaltyMoney();
            Debug.Log($"<color=red>✗ NPC del pedido #{order.orderID} ha MUERTO... (Winrate: {successPercentage:F1}%)</color>");
            
            // ⭐ AÑADE ESTA LÍNEA AQUÍ ⭐
            OrderEvaluator.Instance?.NotifyClientDeath();
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
        if (clientOrderData.order?.animationController != null)
        {
            clientOrderData.order.animationController.SetTalking(false);
        }

        if (clientOrderData.uiElement != null)
        {
            Destroy(clientOrderData.uiElement);
        }
                
        if (clientOrderData.bubbleElement != null)
        {
            Destroy(clientOrderData.bubbleElement);
        }

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
                        if (OrderEvaluator.Instance != null)
                        {
                            OrderEvaluator.Instance.ProcessCompletedOrder(clientOrderData);
                        }
                        else
                        {
                            Debug.LogError("<color=red>OrderEvaluator.Instance es null!</color>");
                            RemoveOrder(clientOrderData);
                        }
                    }
                    else
                    {
                        // Sin objetos = abandono total
                        Debug.Log($"<color=red>✗ NPC del pedido #{clientOrderData.order.orderID} ha ABANDONADO (0 objetos entregados)</color>");
                        OrderEvaluator.Instance.ProcessCompletedOrder(clientOrderData);
                        RemoveOrder(clientOrderData);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Obtiene la lista de pedidos activos (para el tutorial)
    /// </summary>
    public List<ClientOrderData> GetActiveClientOrders()
    {
        return activeClientOrders;
    }
}
