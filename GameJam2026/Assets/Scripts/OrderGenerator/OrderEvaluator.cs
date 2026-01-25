using UnityEngine;
using System;

public class OrderEvaluator : MonoBehaviour
{
    public static OrderEvaluator Instance { get; private set; }
    
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Procesa un pedido completado y determina si el NPC sobrevive
    /// Usa el nuevo sistema: P(total) = P(misión) - P(desesperación)
    /// </summary>
    public void ProcessCompletedOrder(OrderSystem.ClientOrderData clientOrderData)
    {
        if (clientOrderData == null) return;
        
        Order order = clientOrderData.order;
        
        // Calcular P(misión) usando el nuevo sistema de categorías
        float missionSuccessRate = order.CalculateMissionSuccessRate();
        
        // Calcular P(desesperación) usando el ClientTimer
        float desperationPenalty = CalculateTimePenalty(clientOrderData.clientTimer, order);
        
        // Obtener el nivel de desesperación para mostrar
        DesperationLevel level = clientOrderData.clientTimer != null ? 
            clientOrderData.clientTimer.GetDesperationLevel() : DesperationLevel.None;
                
        // P(total) = P(misión) - P(desesperación)
        float totalSuccessRate = missionSuccessRate - desperationPenalty;
        
        // Asegurar que esté entre 0-100
        totalSuccessRate = Mathf.Clamp(totalSuccessRate, 0f, 100f);
        
        // Mostrar cálculo final (ESTE LO MANTENEMOS)
        Debug.Log($"<color=cyan>📊 Pedido #{order.orderID}: P(misión)={missionSuccessRate:F1}% - P(desesperación)={desperationPenalty:F1}% = P(total)={totalSuccessRate:F1}%</color>");
        
        // Determinar resultado inmediatamente
        OrderSystem.Instance.DetermineOrderOutcomeImmediate(clientOrderData, totalSuccessRate);
    }
    
    /// <summary>
    /// Calcula la penalización por tiempo según el ClientTimer
    /// Usa directamente el nivel de desesperación calculado por ClientTimer
    /// </summary>
    private float CalculateTimePenalty(ClientTimer clientTimer, Order order = null)
    {
        if (clientTimer == null) return 0f;
        
        // Obtener el nivel de desesperación directamente desde ClientTimer
        DesperationLevel level = clientTimer.GetDesperationLevel();
        bool isGood = level != DesperationLevel.Abandon;
        // Triggear animación una sola vez
        if (order != null && order.animationController != null)
        {
            if (isGood)
                order.animationController.TriggerGood();
            else
                order.animationController.TriggerBad();
        }
        // Mapear nivel a penalización
        return level switch
        {
            DesperationLevel.None => 0f,
            DesperationLevel.Low => penalty_Nervioso,
            DesperationLevel.Medium => penalty_Impaciente,
            DesperationLevel.High => penalty_Desesperado,
            DesperationLevel.Abandon => penalty_Abandonado,
            _ => 0f
        };
    }
}