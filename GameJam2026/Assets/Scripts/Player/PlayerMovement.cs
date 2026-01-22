using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float movementSpeed = 10f;
    public float mouseSensitivity = 2f;
    public float rotationSpeed = 100f;
    private Rigidbody playerRigidbody;
    private float cameraVerticalRotation = 0f;



    public bool IsGrounded()
    {
        RaycastHit hit;
        float rayDistance = 1.1f; 
        Vector3 rayOrigin = transform.position;
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance))
        {
            return true;
        }
        return false;
    }

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }
        
        playerRigidbody.freezeRotation = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        // Rotación con mouse usando InputManager
        float mouseX = InputManager.Instance.MouseX * mouseSensitivity;
        float mouseY = InputManager.Instance.MouseY * mouseSensitivity;
        
        transform.Rotate(0, mouseX, 0);
        
        cameraVerticalRotation -= mouseY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);
        
        // Movimiento usando InputManager
        float horizontal = InputManager.Instance.Horizontal;
        float vertical = InputManager.Instance.Vertical;
        
        Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized;
        
        // Mover solo si hay input
        if (moveDirection.magnitude > 0)
        {
            Move(moveDirection);
        }
    }

    public void Move(Vector3 direction)
    {
        Vector3 movement = direction * movementSpeed * Time.deltaTime;
        playerRigidbody.MovePosition(transform.position + movement);
    }


   
}
