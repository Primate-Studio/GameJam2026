using UnityEngine;

/// <summary>
/// Sistema de hints visuales para objetos en el tutorial
/// Muestra luces o efectos visuales para guiar al jugador
/// </summary>
public class TutorialHint : MonoBehaviour
{
    [Header("Visual Effects")]
    public GameObject highlightEffect; // Efecto de resaltado (luz, partículas, etc.)
    public Color highlightColor = Color.yellow;
    
    [Header("Object Reference")]
    public GameObject targetObject; // El objeto que se está resaltando
    public ObjectType objectType; // Tipo de objeto para verificaciones

    private Light hintLight;
    private bool isActive = false;

    private void Awake()
    {
        // Si no hay efecto asignado, crear una luz simple
        if (highlightEffect == null)
        {
            CreateDefaultLight();
        }
        else
        {
            // Obtener componentes del efecto
            hintLight = highlightEffect.GetComponent<Light>();
        }

        // Ocultar al inicio
        HideHint();
    }

    /// <summary>
    /// Crea una luz por defecto si no hay efecto asignado
    /// </summary>
    private void CreateDefaultLight()
    {
        highlightEffect = new GameObject("HintLight");
        highlightEffect.transform.SetParent(transform);
        highlightEffect.transform.localPosition = Vector3.up * 0.5f;

        hintLight = highlightEffect.AddComponent<Light>();
        hintLight.type = LightType.Point;
        hintLight.color = highlightColor;
        hintLight.intensity = 2f;
        hintLight.range = 3f;
    }

    /// <summary>
    /// Muestra el hint visual
    /// </summary>
    public void ShowHint()
    {
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(true);
            isActive = true;
        }
    }

    /// <summary>
    /// Oculta el hint visual
    /// </summary>
    public void HideHint()
    {
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
            isActive = false;
        }
    }

    /// <summary>
    /// Alterna la visibilidad del hint
    /// </summary>
    public void ToggleHint()
    {
        if (isActive)
        {
            HideHint();
        }
        else
        {
            ShowHint();
        }
    }

    /// <summary>
    /// Verifica si el hint está activo
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }

    /// <summary>
    /// Cambia el color del highlight
    /// </summary>
    public void SetColor(Color color)
    {
        highlightColor = color;
        
        if (hintLight != null)
        {
            hintLight.color = color;
        }
    }

    /// <summary>
    /// Animación de pulso para el hint (opcional)
    /// </summary>
    private void Update()
    {
        if (!isActive || hintLight == null) return;

        // Efecto de pulso suave
        float pulse = Mathf.PingPong(Time.time * 2f, 1f);
        hintLight.intensity = 1.5f + pulse * 0.5f;
    }
}
