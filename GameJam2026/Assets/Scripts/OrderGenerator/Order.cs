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
    public OrderState state = OrderState.Tranquilo;
    
    // ID único del pedido
    public int orderID;
    
    /// <summary>
    /// Verifica si el pedido está completo (tiene todos los objetos necesarios)
    /// </summary>
    public bool IsComplete()
    {
        return deliveredItems.Count >= itemsNeeded;
    }
    
    /// <summary>
    /// Calcula cuántos objetos son correctos según los requisitos
    /// </summary>
    public int GetCorrectItemsCount(ItemData[] allItems)
    {
        int correctCount = 0;
        
        foreach (ObjectType deliveredType in deliveredItems)
        {
            // Buscar el ItemData correspondiente
            ItemData itemData = System.Array.Find(allItems, item => item.type == deliveredType);
            
            if (itemData != null && IsItemCompatible(itemData))
            {
                correctCount++;
            }
        }
        
        return correctCount;
    }
    
    /// <summary>
    /// Verifica si un item es compatible con los requisitos del pedido
    /// Solo verifica los requisitos que no son null
    /// </summary>
    public bool IsItemCompatible(ItemData item)
    {
        bool monsterMatch = monster != null && System.Array.Exists(item.compatibleMonsters, m => m == monster);
        bool conditionMatch = condition != null && System.Array.Exists(item.compatibleConditions, c => c == condition);
        bool environmentMatch = environment == null || System.Array.Exists(item.compatibleEnvironments, e => e == environment);
        
        // Si monster o condition son requeridos pero no coinciden, es incorrecto
        if (monster != null && !monsterMatch) return false;
        if (condition != null && !conditionMatch) return false;
        
        // Environment es opcional (puede ser null), si existe debe coincidir
        if (environment != null && !environmentMatch) return false;
        
        return true;
    }
}

/// <summary>
/// Estados del pedido según el tiempo transcurrido
/// </summary>
public enum OrderState
{
    Tranquilo,      // 60s-36s
    Nervioso,       // 36s-18s
    Impaciente,     // 18s-6s
    Desesperado,    // 6s-0s
    Abandonado      // 0s
}
