using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Representa un pedido individual de un cliente NPC
/// Contiene los requisitos (Monster, Condition, Environment) y los objetos que necesita
/// </summary>
[System.Serializable]
public class Order
{
    [Header("Order Requirements")]
    public RequirementData monster;
    public RequirementData condition;
    public RequirementData environment;
    
    [Header("Items Needed")]
    public int itemsNeeded; // 2 o 3 objetos por pedido
    public List<ObjectType> deliveredItems = new List<ObjectType>(); // Objetos entregados
    
    [Header("Timer")]
    public float timeElapsed = 0f;
    public float maxTime = 60f; // Configurable por el OrderManager
    
    [Header("Status")]
    public DesperationLevel state = DesperationLevel.None;

    
    // ID único del pedido
    public int orderID;
    public ClientAnimationController animationController;
    
    /// <summary>
    /// Verifica si el pedido está completo (tiene todos los objetos necesarios)
    /// </summary>
    public bool IsComplete()
    {
        return deliveredItems.Count >= itemsNeeded;
    }
    
    /// <summary>
    /// Calcula el porcentaje de éxito para una categoría específica (Monstruo, Condición o Environment)
    /// Ideal = 65%, Decente = 35%, Neutro = 0%, Pésimo = -40% (mín 0%)
    /// </summary>
    public float CalculateCategoryScore(RequirementData requirement)
    {
        if (requirement == null) return 0f;
        
        float totalScore = 0f;
        
        foreach (ObjectType deliveredType in deliveredItems)
        {
            float scoreChange = 0f;
            
            // Verificar si el objeto es Ideal (Good)
            if (System.Array.Exists(requirement.Good, obj => obj == deliveredType))
            {
                scoreChange = 100f;
                MoneyManager.Instance.AddMoney(6f);
                totalScore += scoreChange;
            }
            // Verificar si el objeto es Decente (Mid)
            else if (System.Array.Exists(requirement.Mid, obj => obj == deliveredType))
            {
                scoreChange = 65f;
                MoneyManager.Instance.AddMoney(4f);
                totalScore += scoreChange;
            }
            // Verificar si el objeto es Pésimo (Bad)
            else if (System.Array.Exists(requirement.Bad, obj => obj == deliveredType))
            {
                scoreChange = 0f;
                MoneyManager.Instance.AddMoney(1f);
                totalScore += scoreChange;
            }
            else
            {
                scoreChange = 0f;
                MoneyManager.Instance.AddMoney(1f);
                totalScore += scoreChange;
            }

        }
        
        // La puntuación nunca puede ser negativa
        float finalScore = Mathf.Max(0f, totalScore);
        
        return finalScore;
    }
    
    /// <summary>
    /// Calcula el porcentaje total de éxito de la misión
    /// P(misión) = (V_monstruo + V_condición + V_entorno) / 3
    /// </summary>
    public float CalculateMissionSuccessRate()
    {
        
        float monsterScore = CalculateCategoryScore(monster);
        float conditionScore = CalculateCategoryScore(condition);
        float environmentScore = environment != null ? CalculateCategoryScore(environment) : 0f;
        
        float missionScore;
        
        // Si solo hay 2 requisitos (sin environment), calcular solo con esos 2
        if (environment == null)
        {
            missionScore = (monsterScore + conditionScore) / 2f;
        }
        else
        {
            // Si hay 3 requisitos, calcular con los 3
            missionScore = (monsterScore + conditionScore + environmentScore) / 3f;
        }
        
        return missionScore;
    }
    
    /// <summary>
    /// Obtiene información detallada de la evaluación para debug
    /// </summary>
    public string GetEvaluationDetails()
    {
        float monsterScore = CalculateCategoryScore(monster);
        float conditionScore = CalculateCategoryScore(condition);
        float environmentScore = environment != null ? CalculateCategoryScore(environment) : 0f;
        
        string details = $"Monstruo ({monster.requirementName}): {monsterScore}%\n";
        details += $"Condición ({condition.requirementName}): {conditionScore}%\n";
        
        if (environment != null)
        {
            details += $"Entorno ({environment.requirementName}): {environmentScore}%\n";
        }
        
        return details;
    }
}
