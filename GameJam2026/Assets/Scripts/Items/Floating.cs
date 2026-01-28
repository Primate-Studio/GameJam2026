using UnityEngine;

public class Floating : MonoBehaviour
{
    [Header("Breathing Settings")]
    [SerializeField] float speed = 2f;        // Velocidad de respiración
    [SerializeField] float scaleAmount = 0.05f; // Cuánto se expande
    [SerializeField] bool startActive = false;

    Vector3 baseScale;
    bool isActive;

    void Awake()
    {
        baseScale = transform.localScale;
        isActive = startActive;
    }

    void Update()
    {
        if (!isActive) return;

        float pulse = Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = baseScale + Vector3.one * pulse;
    }

    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
        transform.localScale = baseScale; // volver a escala original
    }
}
