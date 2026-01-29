using UnityEngine;

/// <summary>
/// Gestiona las restricciones del jugador durante el tutorial
/// Bloquea movimiento, cámara, interacciones, inventario y manual según sea necesario
/// </summary>
public class TutorialPlayerRestrictions : MonoBehaviour
{
    public static TutorialPlayerRestrictions Instance { get; private set; }

    [Header("Movement Restrictions")]
    public bool canMove = true;
    public bool canMoveCamera = true;

    [Header("Interaction Restrictions")]
    public bool canInteract = true;
    public bool canOpenManual = true;
    public bool canUseInventory = true;

    [Header("Object Restrictions")]
    public bool restrictObjectTypes = false;
    public ObjectType[] allowedObjectTypes;

    [Header("References")]
    private Transform cameraTransform;
    private Transform playerTransform;

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

    private void Start()
    {
        // Obtener referencias
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            cameraTransform = Camera.main.transform;
        }

        // Por defecto, todo permitido
        EnableAll();
    }

    /// <summary>
    /// Activa o desactiva TODAS las restricciones
    /// </summary>
    public void SetFullRestriction(bool restricted)
    {
        canMove = !restricted;
        canMoveCamera = !restricted;
        canInteract = !restricted;
        canOpenManual = !restricted;
        canUseInventory = !restricted;

        // Notificar a otros sistemas
        if (InputManager.Instance != null)
        {
            // El InputManager usará estas flags para bloquear inputs
        }
    }

    /// <summary>
    /// Permite todas las acciones
    /// </summary>
    public void EnableAll()
    {
        canMove = true;
        canMoveCamera = true;
        canInteract = true;
        canOpenManual = true;
        canUseInventory = true;
        restrictObjectTypes = false;
    }

    /// <summary>
    /// Bloquea todas las acciones
    /// </summary>
    public void DisableAll()
    {
        canMove = false;
        canMoveCamera = false;
        canInteract = false;
        canOpenManual = false;
        canUseInventory = false;
    }

    /// <summary>
    /// Permite solo movimiento y cámara
    /// </summary>
    public void AllowMovementOnly()
    {
        canMove = true;
        canMoveCamera = true;
        canInteract = false;
        canOpenManual = false;
        canUseInventory = false;
    }

    /// <summary>
    /// Restringe los tipos de objetos que el jugador puede recoger
    /// </summary>
    public void SetAllowedObjects(params ObjectType[] types)
    {
        restrictObjectTypes = true;
        allowedObjectTypes = types;
    }
    
    /// <summary>
    /// Configura los tipos de objetos permitidos (alias)
    /// </summary>
    public void SetAllowedObjectTypes(ObjectType[] types, bool restricted)
    {
        Debug.Log($"<color=magenta>=== SetAllowedObjectTypes llamado ===</color>");
        Debug.Log($"<color=magenta>restricted={restricted}, types={(types != null ? string.Join(", ", types) : "NULL")}</color>");
        
        if (types == null || types.Length == 0 || !restricted)
        {
            Debug.Log($"<color=magenta>Limpiando restricciones de objetos</color>");
            ClearObjectRestrictions();
        }
        else
        {
            Debug.Log($"<color=magenta>Estableciendo objetos permitidos: {string.Join(", ", types)}</color>");
            SetAllowedObjects(types);
        }
        
        Debug.Log($"<color=magenta>Estado final: restrictObjectTypes={restrictObjectTypes}, allowedObjectTypes.Length={allowedObjectTypes?.Length ?? 0}</color>");
    }

    /// <summary>
    /// Elimina las restricciones de tipos de objetos
    /// </summary>
    public void ClearObjectRestrictions()
    {
        restrictObjectTypes = false;
        allowedObjectTypes = null;
    }

    /// <summary>
    /// Verifica si un tipo de objeto está permitido
    /// </summary>
    public bool IsObjectAllowed(ObjectType type)
    {
        Debug.Log($"<color=cyan>IsObjectAllowed({type}): restrictObjectTypes={restrictObjectTypes}</color>");
        
        if (!restrictObjectTypes)
        {
            Debug.Log($"<color=cyan>No hay restricciones activas, objeto {type} permitido</color>");
            return true;
        }

        if (allowedObjectTypes == null || allowedObjectTypes.Length == 0)
        {
            Debug.Log($"<color=yellow>Lista de permitidos vacía, permitiendo {type} por defecto</color>");
            return true;
        }

        foreach (ObjectType allowedType in allowedObjectTypes)
        {
            if (allowedType == type)
            {
                Debug.Log($"<color=green>✓ {type} está en la lista de permitidos!</color>");
                return true;
            }
        }

        Debug.Log($"<color=red>✗ {type} NO está en la lista permitida: [{string.Join(", ", allowedObjectTypes)}]</color>");
        return false;
    }

    /// <summary>
    /// Hace que el jugador mire a un objetivo específico
    /// </summary>
    public void LookAtTarget(Transform target)
    {
        if (cameraTransform == null || target == null) return;

        StartCoroutine(LookAtTargetCoroutine(target));
    }

    private System.Collections.IEnumerator LookAtTargetCoroutine(Transform target)
    {
        if (cameraTransform == null) yield break;

        float duration = 1.5f;
        float elapsed = 0f;

        // NO guardar la rotación inicial - empezar desde donde está ahora
        Vector3 direction = target.position - cameraTransform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion startRotation = cameraTransform.rotation; // Tomar rotación actual justo antes de empezar

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        // Asegurarse de que termina exactamente mirando al objetivo
        cameraTransform.rotation = targetRotation;
        
        // ESPERAR un frame para que la rotación se aplique completamente
        yield return null;
        
        // NOTA: NO sincronizamos la rotación al final para evitar "teleports"
        // La cámara se queda donde el jugador la dejó durante la conversación
        Debug.Log($"<color=green>✓ LookAt completado - Cámara libre</color>");
    }

    /// <summary>
    /// Permite el movimiento
    /// </summary>
    public void EnableMovement()
    {
        canMove = true;
    }

    /// <summary>
    /// Bloquea el movimiento
    /// </summary>
    public void DisableMovement()
    {
        canMove = false;
    }

    /// <summary>
    /// Permite mover la cámara
    /// </summary>
    public void EnableCameraMovement()
    {
        canMoveCamera = true;
    }

    /// <summary>
    /// Bloquea mover la cámara
    /// </summary>
    public void DisableCameraMovement()
    {
        canMoveCamera = false;
    }

    /// <summary>
    /// Permite interactuar con objetos
    /// </summary>
    public void EnableInteraction()
    {
        canInteract = true;
    }

    /// <summary>
    /// Bloquea interactuar con objetos
    /// </summary>
    public void DisableInteraction()
    {
        canInteract = false;
    }

    /// <summary>
    /// Permite abrir el manual
    /// </summary>
    public void EnableManual()
    {
        canOpenManual = true;
    }

    /// <summary>
    /// Bloquea abrir el manual
    /// </summary>
    public void DisableManual()
    {
        canOpenManual = false;
    }

    /// <summary>
    /// Permite usar el inventario
    /// </summary>
    public void EnableInventory()
    {
        canUseInventory = true;
    }

    /// <summary>
    /// Bloquea usar el inventario
    /// </summary>
    public void DisableInventory()
    {
        canUseInventory = false;
    }
}
