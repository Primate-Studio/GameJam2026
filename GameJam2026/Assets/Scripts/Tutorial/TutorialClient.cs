using UnityEngine;

/// <summary>
/// Controlador para clientes predefinidos en el tutorial
/// Gestiona su estado, transform y su pedido asociado
/// </summary>
public class TutorialClient : MonoBehaviour
{
    [Header("Client Info")]
    public int clientID; // 1 o 2
    public string clientName;
    public Transform clientTransform;
    public Transform interactionZone; // Zona donde el jugador debe estar para interactuar

    [Header("Client State")]
    public bool hasBeenTalkedTo = false;
    public bool hasReceivedOrder = false;
    public bool orderCompleted = false;

    [Header("Order Data")]
    public RequirementData requirement1;
    public RequirementData requirement2;
    public RequirementData requirement3; // Puede ser null si solo tiene 2 requisitos

    [Header("Visual References")]
    public GameObject backpack; // Mochila para entregar objetos
    public TutorialHint[] objectHints; // Objetos iluminados para este cliente
    
    [Header("Animator")]
    public Animator clientAnimator;

    private void Start()
    {
        // Ocultar elementos inicialmente
        if (backpack != null)
        {
            backpack.SetActive(false);
        }

        // Ocultar hints de objetos
        HideObjectHints();
    }

    /// <summary>
    /// Verifica si el jugador está en la zona de interacción con este cliente
    /// </summary>
    public bool IsPlayerInZone()
    {
        if (interactionZone == null) return false;

        // Obtener la posición del jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;

        float distance = Vector3.Distance(player.transform.position, interactionZone.position);
        return distance < 2f; // Radio de interacción configurable
    }

    /// <summary>
    /// Marca al cliente como "hablado"
    /// </summary>
    public void SetTalkedTo()
    {
        hasBeenTalkedTo = true;
    }

    /// <summary>
    /// Muestra la mochila para entregar objetos
    /// </summary>
    public void ShowBackpack()
    {
        Debug.Log($"<color=yellow>=== [TutorialClient {clientID}] ShowBackpack() LLAMADO ===</color>");
        Debug.Log($"<color=yellow>Backpack GameObject: {(backpack != null ? backpack.name : "NULL")}</color>");
        
        if (backpack != null)
        {
            Debug.Log($"<color=yellow>Estado ANTES SetActive: activeSelf={backpack.activeSelf}, activeInHierarchy={backpack.activeInHierarchy}</color>");
            Debug.Log($"<color=yellow>Parent: {(backpack.transform.parent != null ? backpack.transform.parent.name : "null")}</color>");
            
            backpack.SetActive(true);
            
            Debug.Log($"<color=cyan>Estado DESPUÉS SetActive: activeSelf={backpack.activeSelf}, activeInHierarchy={backpack.activeInHierarchy}</color>");
            Debug.Log($"<color=green>✓ Backpack ACTIVADA para cliente {clientID}</color>");
        }
        else
        {
            Debug.LogError($"<color=red>✗✗✗ BACKPACK ES NULL para cliente {clientID}! ✗✗✗</color>");
            Debug.LogError($"<color=red>SOLUCIÓN: Ve al Inspector y asigna el GameObject Backpack al campo 'backpack' del TutorialClient {clientID}</color>");
        }
    }

    /// <summary>
    /// Oculta la mochila
    /// </summary>
    public void HideBackpack()
    {
        if (backpack != null)
        {
            backpack.SetActive(false);
        }
    }

    /// <summary>
    /// Muestra los hints de los objetos iluminados para este cliente
    /// </summary>
    public void ShowObjectHints()
    {
        if (objectHints == null) return;

        foreach (TutorialHint hint in objectHints)
        {
            if (hint != null)
            {
                hint.ShowHint();
            }
        }
    }

    /// <summary>
    /// Oculta los hints de objetos
    /// </summary>
    public void HideObjectHints()
    {
        if (objectHints == null) return;

        foreach (TutorialHint hint in objectHints)
        {
            if (hint != null)
            {
                hint.HideHint();
            }
        }
    }

    /// <summary>
    /// Marca el pedido como completado
    /// </summary>
    public void CompleteOrder()
    {
        orderCompleted = true;
        HideBackpack();
        HideObjectHints();
    }

    /// <summary>
    /// Obtiene el número de requisitos de este cliente (2 o 3)
    /// </summary>
    public int GetRequirementCount()
    {
        if (requirement3 != null)
        {
            return 3;
        }
        return 2;
    }

    /// <summary>
    /// Reproduce una animación del cliente si tiene animador
    /// </summary>
    public void PlayAnimation(string animationName)
    {
        if (clientAnimator != null)
        {
            clientAnimator.SetTrigger(animationName);
        }
    }
}
