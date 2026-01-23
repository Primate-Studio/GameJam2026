using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Camera")]
    public Camera playerCamera;
    
    [Header("Movement")]
    public float movementSpeed = 5f;
    public float acceleration = 10f;
    
    [Header("Look")]
    public float mouseSensitivity = 2f;
    public float verticalLookLimit = 90f;
    
    private Rigidbody playerRigidbody;
    private float cameraVerticalRotation = 0f;
    private Vector3 moveDirection;

    void Start()
    {
        // Configurar cámara
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // Configurar rigidbody
        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            playerRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        playerRigidbody.freezeRotation = true;
        playerRigidbody.useGravity = true;
        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        
        // Bloquear cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Rotación de cámara (en Update para suavidad)
        HandleMouseLook();
        
        // Capturar input de movimiento
        CaptureMovementInput();
    }
    
    void FixedUpdate()
    {
        // Aplicar movimiento (en FixedUpdate para física)
        ApplyMovement();
    }

    /// <summary>
    /// Maneja la rotación del jugador y la cámara con el ratón
    /// </summary>
    private void HandleMouseLook()
    {
        if (InputManager.Instance == null || GameManager.Instance.CurrentState == GameState.Paused) return;
        
        // Rotación horizontal del jugador
        float mouseX = InputManager.Instance.MouseX * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
        
        // Rotación vertical de la cámara
        float mouseY = InputManager.Instance.MouseY * mouseSensitivity;
        cameraVerticalRotation -= mouseY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -verticalLookLimit, verticalLookLimit);
        
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);
        }
    }

    /// <summary>
    /// Captura el input de movimiento WASD
    /// </summary>
    private void CaptureMovementInput()
    {
        if (InputManager.Instance == null)
        {
            moveDirection = Vector3.zero;
            return;
        }
        
        float horizontal = InputManager.Instance.Horizontal;
        float vertical = InputManager.Instance.Vertical;
        
        // Calcular dirección relativa al jugador
        Vector3 forward = transform.forward * vertical;
        Vector3 right = transform.right * horizontal;
        
        moveDirection = (forward + right).normalized;
    }

    /// <summary>
    /// Aplica el movimiento al Rigidbody
    /// </summary>
    private void ApplyMovement()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            // Calcular velocidad objetivo
            Vector3 targetVelocity = moveDirection * movementSpeed;
            
            // Mantener velocidad vertical actual (gravedad)
            targetVelocity.y = playerRigidbody.linearVelocity.y;
            
            // Interpolar suavemente hacia la velocidad objetivo
            playerRigidbody.linearVelocity = Vector3.Lerp(
                playerRigidbody.linearVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            // Detener movimiento horizontal instantáneamente, mantener vertical
            Vector3 velocity = playerRigidbody.linearVelocity;
            velocity.x = 0f;
            velocity.z = 0f;
            playerRigidbody.linearVelocity = velocity;
        }
    }


    /// <summary>
    /// Método público para mover el jugador (compatibilidad)
    /// </summary>
    public void Move(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }
}
