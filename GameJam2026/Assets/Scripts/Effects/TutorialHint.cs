using UnityEngine;

public class TutorialHint : MonoBehaviour
{
    private Transform camTransform;
    
    [Header("Animació")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.1f;
    
    private Vector3 initialLocalPos;
    private bool isActive = false;

    void Awake()
    {
        // Guardem la posició local que hagis posat a l'editor com a base
        initialLocalPos = transform.localPosition;
        if (Camera.main != null) camTransform = Camera.main.transform;
        
        // Ens assegurem que comenci amagat
        gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (camTransform == null || !isActive) return;

        // 1. Billboarding: Que la fletxa miri sempre a la càmera
        // Copiem la rotació perquè el text o la imatge no es vegin invertits
        transform.rotation = camTransform.rotation;

        // 2. Bobbing: Moviment amunt i avall respecte la posició inicial
        float newY = initialLocalPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = new Vector3(initialLocalPos.x, newY, initialLocalPos.z);
    }

    /// <summary>
    /// Activa o desactiva la pista visual.
    /// </summary>
    public void ShowHint(bool state)
    {
        isActive = state;
        gameObject.SetActive(state);
    }
}