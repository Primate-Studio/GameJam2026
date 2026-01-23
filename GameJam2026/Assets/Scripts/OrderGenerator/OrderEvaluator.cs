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
        //Debug.Log($"<color=yellow>📦 Pedido #{order.orderID} completado! Procesando...</color>");
        
        // Calcular P(misión) usando el nuevo sistema de categorías (CON LOGS comentados)
        float missionSuccessRate = order.CalculateMissionSuccessRate(false); // Cambiar a true para ver logs detallados
        
        // Calcular P(desesperación) usando el ClientTimer
        float desperationPenalty = CalculateTimePenalty(clientOrderData.clientTimer);
        
        // Obtener el nivel de desesperación para mostrar
        DesperationLevel level = clientOrderData.clientTimer != null ? 
            clientOrderData.clientTimer.GetDesperationLevel() : DesperationLevel.None;
        
        //Debug.Log($"<color=magenta>\n⏱️ Nivel de Desesperación: {level} → Penalización: {desperationPenalty}%</color>");
        
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
    private float CalculateTimePenalty(ClientTimer clientTimer)
    {
        if (clientTimer == null) return 0f;
        
        // Obtener el nivel de desesperación directamente desde ClientTimer
        DesperationLevel level = clientTimer.GetDesperationLevel();
        
        // Mapear nivel a penalización
        switch (level)
        {
            case DesperationLevel.None:
                return 0f;
            case DesperationLevel.Low:
                return penalty_Nervioso;
            case DesperationLevel.Medium:
                return penalty_Impaciente;
            case DesperationLevel.High:
                return penalty_Desesperado;
            case DesperationLevel.Abandon:
                return penalty_Abandonado;
            default:
                return 0f;
        }
    }
}