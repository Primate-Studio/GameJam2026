using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestor del estado del tutorial no lineal
/// Rastrea qué ha hecho el jugador, qué explicaciones ha visto y qué pasos están disponibles
/// </summary>
public class TutorialStateManager : MonoBehaviour
{
    public static TutorialStateManager Instance { get; private set; }

    [Header("Knowledge State - Explicaciones vistas")]
    public bool hasLearnedMovement = false;
    public bool hasLearnedCamera = false;
    public bool hasLearnedManual = false;
    public bool hasLearnedOrders = false;
    public bool hasLearnedItemQuality = false;
    public bool hasLearnedDesperation = false;
    public bool hasLearnedInteraction = false;
    public bool hasLearnedInventory = false;

    [Header("Manual State")]
    public bool hasOpenedManual = false;
    public bool hasSeenManualExplanation = false;

    [Header("Client Interaction State")]
    public bool hasApproachedClient1 = false;
    public bool hasApproachedClient2 = false;
    public bool hasTalkedWithClient1 = false;
    public bool hasTalkedWithClient2 = false;

    [Header("Object Interaction State")]
    public bool hasSeenClient1Objects = false;
    public bool hasSeenClient2Objects = false;
    public bool hasInteractedWithClient1Objects = false;
    public bool hasInteractedWithClient2Objects = false;

    [Header("Order State")]
    public bool hasReceivedClient1Order = false;
    public bool hasReceivedClient2Order = false;
    public bool hasCompletedClient1Order = false;
    public bool hasCompletedClient2Order = false;

    [Header("Tutorial Progress")]
    public TutorialPhase currentPhase = TutorialPhase.Introduction;
    public int clientsCompleted = 0; // 0, 1 o 2
    
    [Header("Active Client")]
    public TutorialClient activeClient = null; // Cliente con el que está interactuando actualmente
    public TutorialClient firstClientDone = null; // Primer cliente completado
    public TutorialClient secondClientDone = null; // Segundo cliente completado

    public enum TutorialPhase
    {
        Introduction,          // Explicaciones iniciales (movimiento, cámara, manual)
        FreeExploration,       // Jugador puede elegir cliente
        ClientInteraction,     // Hablando con un cliente
        ObjectsExplanation,    // Explicando los objetos
        OrderDelivery,         // Entregando el pedido
        SecondClient,          // Haciendo el segundo cliente
        Completed              // Tutorial completado
    }

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
    /// Cambia la fase actual del tutorial
    /// </summary>
    public void SetPhase(TutorialPhase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log($"Tutorial Phase changed to: {newPhase}");
    }

    /// <summary>
    /// Verifica si el jugador ha completado las explicaciones básicas
    /// </summary>
    public bool HasCompletedBasicExplanations()
    {
        return hasLearnedMovement && hasLearnedCamera && hasLearnedManual;
    }

    /// <summary>
    /// Verifica si el jugador puede avanzar con un cliente específico
    /// </summary>
    public bool CanProgressWithClient(int clientID)
    {
        if (clientID == 1)
        {
            // Para cliente 1: debe haber hablado y visto sus objetos
            return hasTalkedWithClient1 && hasSeenClient1Objects;
        }
        else if (clientID == 2)
        {
            // Para cliente 2: debe haber hablado y visto sus objetos
            return hasTalkedWithClient2 && hasSeenClient2Objects;
        }

        return false;
    }

    /// <summary>
    /// Verifica si es el primer cliente que completa
    /// </summary>
    public bool IsFirstClient()
    {
        return clientsCompleted == 0;
    }

    /// <summary>
    /// Verifica si ya completó un cliente
    /// </summary>
    public bool HasCompletedOneClient()
    {
        return clientsCompleted >= 1;
    }

    /// <summary>
    /// Verifica si el tutorial está completo
    /// </summary>
    public bool IsTutorialComplete()
    {
        return clientsCompleted >= 2;
    }

    /// <summary>
    /// Marca un cliente como completado
    /// </summary>
    public void CompleteClient(TutorialClient client)
    {
        clientsCompleted++;

        if (client.clientID == 1)
        {
            hasCompletedClient1Order = true;
        }
        else if (client.clientID == 2)
        {
            hasCompletedClient2Order = true;
        }

        if (firstClientDone == null)
        {
            firstClientDone = client;
        }
        else if (secondClientDone == null)
        {
            secondClientDone = client;
        }

        Debug.Log($"Cliente {client.clientID} completado. Total completados: {clientsCompleted}");
    }

    /// <summary>
    /// Establece el cliente activo actual
    /// </summary>
    public void SetActiveClient(TutorialClient client)
    {
        activeClient = client;
    }

    /// <summary>
    /// Limpia el cliente activo
    /// </summary>
    public void ClearActiveClient()
    {
        activeClient = null;
    }

    /// <summary>
    /// Verifica si debe mostrar la explicación del manual
    /// </summary>
    public bool ShouldExplainManual()
    {
        return !hasSeenManualExplanation;
    }

    /// <summary>
    /// Verifica si debe mostrar la explicación de pedidos
    /// </summary>
    public bool ShouldExplainOrders()
    {
        return !hasLearnedOrders;
    }

    /// <summary>
    /// Verifica si debe mostrar la explicación de objetos
    /// </summary>
    public bool ShouldExplainObjects()
    {
        return !hasLearnedItemQuality;
    }

    /// <summary>
    /// Obtiene el cliente que no ha sido completado aún
    /// </summary>
    public TutorialClient GetRemainingClient(TutorialClient client1, TutorialClient client2)
    {
        if (!hasCompletedClient1Order)
        {
            return client1;
        }
        else if (!hasCompletedClient2Order)
        {
            return client2;
        }

        return null;
    }

    /// <summary>
    /// Verifica si un cliente específico ha sido completado
    /// </summary>
    public bool IsClientCompleted(int clientID)
    {
        if (clientID == 1)
        {
            return hasCompletedClient1Order;
        }
        else if (clientID == 2)
        {
            return hasCompletedClient2Order;
        }

        return false;
    }

    /// <summary>
    /// Resetea el estado del tutorial (útil para debugging)
    /// </summary>
    public void ResetTutorial()
    {
        hasLearnedMovement = false;
        hasLearnedCamera = false;
        hasLearnedManual = false;
        hasLearnedOrders = false;
        hasLearnedItemQuality = false;
        hasLearnedDesperation = false;
        hasLearnedInteraction = false;
        hasLearnedInventory = false;

        hasOpenedManual = false;
        hasSeenManualExplanation = false;

        hasApproachedClient1 = false;
        hasApproachedClient2 = false;
        hasTalkedWithClient1 = false;
        hasTalkedWithClient2 = false;

        hasSeenClient1Objects = false;
        hasSeenClient2Objects = false;
        hasInteractedWithClient1Objects = false;
        hasInteractedWithClient2Objects = false;

        hasReceivedClient1Order = false;
        hasReceivedClient2Order = false;
        hasCompletedClient1Order = false;
        hasCompletedClient2Order = false;

        currentPhase = TutorialPhase.Introduction;
        clientsCompleted = 0;
        activeClient = null;
        firstClientDone = null;
        secondClientDone = null;
    }
}
