using UnityEngine;
using System.Collections;

/// <summary>
/// Controla el comportamiento y movimiento del perro tutor durante el tutorial
/// </summary>
public class TutorialDog : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float hoverHeight = 1.5f;
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float hoverAmplitude = 0.2f;

    [Header("Animation")]
    [SerializeField] private Animator dogAnimator;
    
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float hoverTimer = 0f;
    private Vector3 basePosition;
    private Transform lookAtTarget = null;
    private bool shouldLookAtTarget = false;

    void Start()
    {
        basePosition = transform.position;
    }

    void Update()
    {
        if (isMoving)
        {
            MoveToTarget();
        }
        else HandleHovering();
        
        // Mantener la mirada en el objetivo si está configurado
        if (shouldLookAtTarget && lookAtTarget != null)
        {
            Vector3 direction = (lookAtTarget.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Hace que el perro se mueva a una posición específica
    /// </summary>
    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        basePosition = transform.position;
        isMoving = true;
        
        // if (dogAnimator != null)
        // {
        //     dogAnimator.SetBool("IsFlying", true);
        // }
    }

    /// <summary>
    /// Detiene el movimiento del perro
    /// </summary>
    public void StopMoving()
    {
        isMoving = false;
        basePosition = transform.position;
        // No desactivamos shouldLookAtTarget aquí para mantener la mirada
        
        // if (dogAnimator != null)
        // {
        //     dogAnimator.SetBool("IsFlying", false);
        // }
    }

    /// <summary>
    /// Mueve el perro hacia la posición objetivo
    /// </summary>
    private void MoveToTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        // Rotar hacia la dirección de movimiento
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Detener cuando llegue al destino
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            StopMoving();
        }
    }

    /// <summary>
    /// Efecto de flotación suave arriba y abajo
    /// </summary>
    private void HandleHovering()
    {
        if (!isMoving)
        {
            hoverTimer += Time.deltaTime * hoverSpeed;
            float hoverOffset = Mathf.Sin(hoverTimer) * hoverAmplitude;
            transform.position = basePosition + Vector3.up * hoverOffset;
        }
    }

    /// <summary>
    /// Hace que el perro mire hacia una posición específica
    /// </summary>
    public void LookAt(Vector3 position)
    {
        Vector3 direction = (position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    /// <summary>
    /// Hace que el perro mire hacia un transform específico y lo siga continuamente
    /// </summary>
    public void LookAt(Transform target)
    {
        lookAtTarget = target;
        shouldLookAtTarget = true;
    }
    
    /// <summary>
    /// Detiene el seguimiento de la mirada
    /// </summary>
    public void StopLookingAt()
    {
        shouldLookAtTarget = false;
        lookAtTarget = null;
    }

    /// <summary>
    /// Muestra el perro
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Oculta el perro
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Reproduce una animación específica si está disponible
    /// </summary>
    public void PlayAnimation(string animationName, bool active)
    {
        if (dogAnimator != null)
        {
            dogAnimator.SetBool(animationName, active);
        }
    }
}
