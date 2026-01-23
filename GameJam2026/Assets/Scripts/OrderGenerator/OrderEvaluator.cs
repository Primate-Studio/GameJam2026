using UnityEngine;
using System;

public class OrderEvaluator : MonoBehaviour
{
    public static OrderEvaluator Instance { get; private set; }
    
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


    void Awake()
    {
        Instance = this;    
    }

     /// <summary>
    /// Procesa un pedido completado y determina si el NPC sobrevive
    /// </summary>
    public void ProcessCompletedOrder(OrderSystem.ClientOrderData clientOrderData)
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
        OrderSystem.Instance.DetermineOrderOutcomeImmediate(clientOrderData, survivalRate);
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
}