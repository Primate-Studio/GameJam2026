using UnityEngine;
using UnityEngine.UI;

public class ItemIconController : MonoBehaviour
{
    [Header("Seguiment")]
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Alçada sobre l'objecte

    [Header("UI References")]
    [SerializeField] private Image iconImage; 
    [SerializeField] private Image fillImage; // El contingut del canvas
    private ObjectType itemType;

    private Transform camTransform;
    private bool isTimerActive = false;
    private float timer = 0f;
    private float duration = 1f;

    void Start()
    {
        camTransform = Camera.main.transform;
        if (fillImage != null) fillImage.fillAmount = 1;
    }

    void Update()
    {

        // 2. LÒGICA DEL TIMER (FILL)
        if (isTimerActive)
        {
            timer += Time.deltaTime;
            fillImage.fillAmount = timer / duration;

            if (timer >= duration)
            {
                StopTimer();
            }
        }
    }
    public void SetupIcon(InteractableObject interactable)
    {
        itemType = interactable.objectType;
        iconImage.sprite = InteractableObjectDatabase.Instance.GetIconForObjectType(itemType);
    }

    void LateUpdate()
    {
        // 3. BILLBOARDING (Mirar al player)
        if (camTransform != null)
        {
            transform.LookAt(transform.position + camTransform.rotation * Vector3.forward,
                             camTransform.rotation * Vector3.up);
        }
    }

    public void StartTimer(float time)
    {
        duration = time;
        timer = 0f;
        isTimerActive = true;
    }

    public void StopTimer()
    {
        isTimerActive = false;
    }
}