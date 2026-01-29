using UnityEngine;

/// <summary>
/// Script de ejemplo para integrar el tutorial con los sistemas existentes
/// Estos métodos deben añadirse a tus scripts actuales (InputManager, InventoryManager, etc.)
/// </summary>
public class TutorialIntegrationExamples : MonoBehaviour
{
    // ============= EJEMPLO PARA InputManager.cs =============
    
    /// <summary>
    /// Añadir esta verificación al inicio de los métodos de input de movimiento
    /// </summary>
    void Example_MovementInput()
    {
        // Verificar si el tutorial permite movimiento
        if (TutorialPlayerRestrictions.Instance != null && 
            !TutorialPlayerRestrictions.Instance.canMove)
        {
            return; // Bloquear movimiento
        }

        // ... tu código de movimiento existente
    }

    /// <summary>
    /// Añadir esta verificación al input de la cámara
    /// </summary>
    void Example_CameraInput()
    {
        // Verificar si el tutorial permite mover la cámara
        if (TutorialPlayerRestrictions.Instance != null && 
            !TutorialPlayerRestrictions.Instance.canMoveCamera)
        {
            return; // Bloquear cámara
        }

        // ... tu código de cámara existente
    }

    /// <summary>
    /// Añadir esta verificación al abrir el manual
    /// </summary>
    void Example_ManualInput()
    {
        // Verificar si el tutorial permite abrir el manual
        if (TutorialPlayerRestrictions.Instance != null && 
            !TutorialPlayerRestrictions.Instance.canOpenManual)
        {
            return; // Bloquear manual
        }

        // Notificar al tutorial que se abrió el manual
        if (TutorialStateManager.Instance != null)
        {
            TutorialStateManager.Instance.hasOpenedManual = true;
        }

        // ... tu código de abrir manual existente
    }

    // ============= EJEMPLO PARA InventoryManager.cs =============
    
    /// <summary>
    /// Añadir esta verificación al cambiar de slot
    /// </summary>
    void Example_ChangeInventorySlot()
    {
        // Verificar si el tutorial permite usar el inventario
        if (TutorialPlayerRestrictions.Instance != null && 
            !TutorialPlayerRestrictions.Instance.canUseInventory)
        {
            return; // Bloquear cambio de slot
        }

        // ... tu código de inventario existente
    }

    // ============= EJEMPLO PARA Interacción con Objetos =============
    
    /// <summary>
    /// Añadir esta verificación al recoger objetos
    /// </summary>
    void Example_PickupObject(ObjectType objectType)
    {
        // Verificar si el tutorial permite interactuar
        if (TutorialPlayerRestrictions.Instance != null && 
            !TutorialPlayerRestrictions.Instance.canInteract)
        {
            Debug.Log("No puedes interactuar en este momento del tutorial");
            return; // Bloquear interacción
        }

        // Verificar si el objeto está permitido (restricciones de tipo)
        if (TutorialPlayerRestrictions.Instance != null && 
            TutorialPlayerRestrictions.Instance.restrictObjectTypes)
        {
            if (!TutorialPlayerRestrictions.Instance.IsObjectAllowed(objectType))
            {
                Debug.Log($"No puedes recoger {objectType} en este momento del tutorial");
                return; // Este tipo de objeto no está permitido
            }
        }

        // ... tu código de pickup existente
    }

    // ============= EJEMPLO PARA DeliveryBox o Sistema de Entregas =============
    
    /// <summary>
    /// Añadir esta lógica al entregar objetos al cliente
    /// </summary>
    void Example_DeliverItemToClient(ObjectType itemType)
    {
        // Si estamos en tutorial, usar el sistema de pedidos del tutorial
        if (GameManager.Instance != null && 
            GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            if (TutorialOrderSystem.Instance != null)
            {
                bool delivered = TutorialOrderSystem.Instance.DeliverItem(itemType);
                
                if (delivered)
                {
                    Debug.Log($"Item {itemType} entregado al pedido del tutorial");
                    
                    // Verificar si el pedido está completo
                    if (TutorialOrderSystem.Instance.IsCurrentOrderComplete())
                    {
                        Debug.Log("Pedido del tutorial completado!");
                        TutorialOrderSystem.Instance.CompleteDelivery();
                    }
                }
                
                return;
            }
        }

        // ... tu código de entrega normal (fuera del tutorial)
    }

    // ============= EJEMPLO PARA ClientManager.cs =============
    
    /// <summary>
    /// Modificar el Update para no spawnear clientes durante el tutorial
    /// </summary>
    void Example_ClientManagerUpdate()
    {
        // En tutorial, no spawnear clientes automáticamente
        if (GameManager.Instance != null && 
            GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            return; // Los clientes del tutorial están predefinidos en la escena
        }

        // ... tu código de spawning normal
    }

    // ============= EJEMPLO PARA OrderGenerator.cs =============
    
    /// <summary>
    /// Bloquear generación automática de pedidos durante el tutorial
    /// </summary>
    void Example_GenerateOrder()
    {
        // No generar pedidos automáticos en tutorial
        if (GameManager.Instance != null && 
            GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            return; // Los pedidos del tutorial se crean manualmente
        }

        // ... tu código de generación de pedidos normal
    }

    // ============= EJEMPLO PARA DayCycleManager o TimeManager =============
    
    /// <summary>
    /// Pausar o modificar el tiempo durante el tutorial
    /// </summary>
    void Example_TimeUpdate()
    {
        // En tutorial, el tiempo puede ir más lento o pausado
        if (GameManager.Instance != null && 
            GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            return; // No avanzar el tiempo, o avanzar muy lento
        }

        // ... tu código de tiempo normal
    }

    // ============= HELPERS PARA DETECTAR EVENTOS =============

    /// <summary>
    /// Ejemplo de cómo detectar cuando el jugador se acerca a un objeto
    /// Esto lo necesitas implementar para completar los TODOs en NewTutorial.cs
    /// </summary>
    bool Example_IsPlayerNearObject(GameObject targetObject, float distance = 2f)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || targetObject == null) return false;

        float dist = Vector3.Distance(player.transform.position, targetObject.transform.position);
        return dist <= distance;
    }

    /// <summary>
    /// Ejemplo de cómo verificar si el jugador tiene un objeto en el inventario
    /// </summary>
    bool Example_PlayerHasObjectInInventory(ObjectType objectType)
    {
        if (InventoryManager.Instance == null) return false;

        // Iterar sobre los slots del inventario
        // Este es un ejemplo - adapta según tu implementación
        for (int i = 0; i < InventoryManager.Instance.slots.Length; i++)
        {
            // Tu lógica para verificar el tipo de objeto en el slot i
            // Por ejemplo:
            // if (InventoryManager.Instance.GetItemInSlot(i) == objectType)
            //     return true;
        }

        return false;
    }

    /// <summary>
    /// Ejemplo de cómo verificar cuántos objetos se han entregado
    /// Útil para los TODOs en NewTutorial.cs
    /// </summary>
    int Example_GetDeliveredItemsCount()
    {
        if (TutorialOrderSystem.Instance != null)
        {
            return TutorialOrderSystem.Instance.GetDeliveredItemsCount();
        }
        return 0;
    }
}

/*
 * INSTRUCCIONES DE INTEGRACIÓN:
 * 
 * 1. InputManager.cs
 *    - Añade las verificaciones de canMove, canMoveCamera, canOpenManual
 *    - En el método que detecta TAB para abrir el manual, notifica al TutorialStateManager
 * 
 * 2. InventoryManager.cs
 *    - Añade la verificación de canUseInventory al cambiar slots
 *    - Añade la verificación de restrictObjectTypes al recoger objetos
 * 
 * 3. ClientManager.cs
 *    - Modifica el Update para no spawnear clientes en tutorial
 *    - Los clientes del tutorial están predefinidos en la escena
 * 
 * 4. OrderGenerator.cs
 *    - Bloquea la generación automática de pedidos en tutorial
 * 
 * 5. DeliveryBox.cs (o sistema de entregas)
 *    - Integra el TutorialOrderSystem para las entregas en tutorial
 *    - Detecta cuando un pedido del tutorial se completa
 * 
 * 6. TimeManager/DayCycleManager
 *    - Opcional: Pausa o ralentiza el tiempo durante el tutorial
 * 
 * 7. ManualUI.cs
 *    - Notifica al TutorialStateManager cuando se abre
 * 
 * NOTA: Todos estos cambios deben verificar primero si están en modo tutorial
 *       usando: GameManager.Instance.CurrentState == GameState.Tutorial
 */
