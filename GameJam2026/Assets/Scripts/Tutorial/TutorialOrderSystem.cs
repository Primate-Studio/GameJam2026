using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema de gestión de pedidos para el tutorial
/// Crea pedidos específicos para los clientes del tutorial y gestiona las entregas
/// </summary>
public class TutorialOrderSystem : MonoBehaviour
{
    public static TutorialOrderSystem Instance { get; private set; }

    [Header("Active Orders")]
    private Dictionary<int, TutorialOrder> activeOrders = new Dictionary<int, TutorialOrder>();

    [Header("Delivery Tracking")]
    private TutorialOrder currentDeliveryOrder = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Crea un pedido para un cliente del tutorial
    /// </summary>
    public TutorialOrder CreateOrder(TutorialClient client)
    {
        if (client == null) return null;

        TutorialOrder order = new TutorialOrder();
        order.clientID = client.clientID;
        order.requirement1 = client.requirement1;
        order.requirement2 = client.requirement2;
        order.requirement3 = client.requirement3;
        order.itemsNeeded = client.GetRequirementCount();

        activeOrders[client.clientID] = order;

        Debug.Log($"Pedido creado para cliente {client.clientID} con {order.itemsNeeded} items");

        return order;
    }
    
    /// <summary>
    /// Crea un pedido directamente con requirements (sobrecarga)
    /// </summary>
    public TutorialOrder CreateOrder(RequirementData req1, RequirementData req2, RequirementData req3 = null)
    {
        TutorialOrder order = new TutorialOrder();
        order.clientID = 1; // Default
        order.requirement1 = req1;
        order.requirement2 = req2;
        order.requirement3 = req3;
        order.itemsNeeded = (req3 != null) ? 3 : 2;

        activeOrders[order.clientID] = order;

        Debug.Log($"Pedido creado con {order.itemsNeeded} requirements");

        return order;
    }

    /// <summary>
    /// Inicia la entrega de un pedido
    /// </summary>
    public void StartDelivery(int clientID)
    {
        if (activeOrders.ContainsKey(clientID))
        {
            currentDeliveryOrder = activeOrders[clientID];
            Debug.Log($"Iniciando entrega para cliente {clientID}");
        }
    }

    /// <summary>
    /// Entrega un objeto al pedido actual
    /// </summary>
    public bool DeliverItem(ObjectType itemType)
    {
        if (currentDeliveryOrder == null)
        {
            Debug.LogWarning("No hay pedido activo para entregar");
            return false;
        }

        // Verificar si ya está completo
        if (currentDeliveryOrder.IsComplete())
        {
            Debug.LogWarning("El pedido ya está completo");
            return false;
        }

        // Agregar el item al pedido
        currentDeliveryOrder.deliveredItems.Add(itemType);
        Debug.Log($"Item {itemType} entregado. Total: {currentDeliveryOrder.deliveredItems.Count}/{currentDeliveryOrder.itemsNeeded}");

        return true;
    }

    /// <summary>
    /// Verifica si el pedido actual está completo
    /// </summary>
    public bool IsCurrentOrderComplete()
    {
        if (currentDeliveryOrder == null) return false;
        return currentDeliveryOrder.IsComplete();
    }
    
    /// <summary>
    /// Verifica si hay algún pedido completado (alias para uso general)
    /// </summary>
    public bool IsOrderCompleted()
    {
        return IsCurrentOrderComplete();
    }

    /// <summary>
    /// Finaliza la entrega del pedido actual
    /// </summary>
    public void CompleteDelivery()
    {
        if (currentDeliveryOrder != null)
        {
            Debug.Log($"Pedido completado para cliente {currentDeliveryOrder.clientID}");
            currentDeliveryOrder = null;
        }
    }

    /// <summary>
    /// Obtiene el pedido de un cliente específico
    /// </summary>
    public TutorialOrder GetOrder(int clientID)
    {
        if (activeOrders.ContainsKey(clientID))
        {
            return activeOrders[clientID];
        }
        return null;
    }

    /// <summary>
    /// Limpia todos los pedidos
    /// </summary>
    public void ClearAllOrders()
    {
        activeOrders.Clear();
        currentDeliveryOrder = null;
    }

    /// <summary>
    /// Obtiene el número de items entregados en el pedido actual
    /// </summary>
    public int GetDeliveredItemsCount()
    {
        if (currentDeliveryOrder == null) return 0;
        return currentDeliveryOrder.deliveredItems.Count;
    }

    /// <summary>
    /// Verifica si un objeto es ideal para alguno de los requisitos del pedido actual
    /// </summary>
    public bool IsItemIdealForOrder(ObjectType itemType)
    {
        if (currentDeliveryOrder == null) return false;

        // TODO: Implementar lógica de verificación contra los requirements
        // Por ahora, siempre retorna true
        return true;
    }
}

/// <summary>
/// Clase que representa un pedido del tutorial
/// </summary>
[System.Serializable]
public class TutorialOrder
{
    public int clientID;
    public RequirementData requirement1;
    public RequirementData requirement2;
    public RequirementData requirement3; // Puede ser null

    public int itemsNeeded;
    public List<ObjectType> deliveredItems = new List<ObjectType>();

    /// <summary>
    /// Verifica si el pedido está completo
    /// </summary>
    public bool IsComplete()
    {
        return deliveredItems.Count >= itemsNeeded;
    }

    /// <summary>
    /// Obtiene el porcentaje de completitud
    /// </summary>
    public float GetCompletionPercentage()
    {
        if (itemsNeeded == 0) return 0f;
        return (float)deliveredItems.Count / itemsNeeded;
    }
}
