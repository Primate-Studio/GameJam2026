using UnityEngine;
using UnityEngine.UI;

public class ClientTimer : MonoBehaviour
{
    public static ClientTimer Instance { get; private set; }
    public float orderDuration = 60f;
    private Image TimerFillImage;
    public Sprite spriteTimerFill;
    private DesperationLevel desperationLevel;

    public float timeRemaining;  // Público para que OrderSystem pueda acceder
    private bool isOrderActive = false;
    private bool timerUICreated = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Update()
    {
        if (isOrderActive)
        {
            //Color change based on time remaining
            timeRemaining -= Time.deltaTime;
            
            if (TimerFillImage != null)
            {
                TimerFillImage.fillAmount = timeRemaining / orderDuration;
                TimerFillImage.color = Color.Lerp(Color.red, Color.green, timeRemaining / orderDuration);
            }
            
            CalculateDesperationLevel();

            if (timeRemaining <= 0f)
            {
                isOrderActive = false;
                if (TimerFillImage != null)
                {
                    TimerFillImage.fillAmount = 0f;
                }
            }
        }
    }
    
    private void CalculateDesperationLevel()
    {
        float percentage = timeRemaining / orderDuration;
        if (percentage >= 0.60f)
        {
            desperationLevel = DesperationLevel.None;
        }
        else if (percentage >= 0.30f && percentage < 0.60f)
        {
            desperationLevel = DesperationLevel.Low;
        }
        else if (percentage >= 0.10f && percentage < 0.30f)
        {
            desperationLevel = DesperationLevel.Medium;
        }
        else if (percentage > 0f && percentage < 0.10f)
        {
            desperationLevel = DesperationLevel.High;
        }
        else if (percentage <= 0f)
        {
            desperationLevel = DesperationLevel.Abandon;
        }
    }
    
    /// <summary>
    /// Obtiene el nivel de desesperación actual
    /// </summary>
    public DesperationLevel GetDesperationLevel()
    {
        return desperationLevel;
    }
    
    /// <summary>
    /// Obtiene el porcentaje de tiempo restante (0-1)
    /// </summary>
    public float GetTimePercentage()
    {
        return timeRemaining / orderDuration;
    }

    public void StartNewOrderTimer()
    {
        // Solo crear la UI la primera vez que se inicia un pedido
        if (!timerUICreated)
        {
            CreateTimer();
            timerUICreated = true;
        }
        
        timeRemaining = orderDuration;
        isOrderActive = true;
        
        if (TimerFillImage != null)
        {
            TimerFillImage.fillAmount = 1f;
        }
    }
    
    private void CreateTimer()
    {
        // Crear un canvas con una Imagen con un Sprite en concreto
        GameObject canvasGO = new GameObject("TimerCanvas");
        canvasGO.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.transform.SetParent(this.transform);
        canvasGO.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
        canvasGO.transform.localPosition = new Vector3(0, 2.5f, 0);
        canvasGO.transform.localRotation = Quaternion.Euler(0, 0, 0);
        canvasGO.transform.localScale = Vector3.one * 0.5f;
        GameObject imageGO = new GameObject("TimerFillImage");
        imageGO.transform.SetParent(canvasGO.transform);
        imageGO.transform.localRotation = Quaternion.Euler(0, 0, 0);
        TimerFillImage = imageGO.AddComponent<Image>();
        TimerFillImage.sprite = spriteTimerFill;

        TimerFillImage.type = Image.Type.Filled;
        TimerFillImage.fillMethod = Image.FillMethod.Radial360;
        TimerFillImage.fillOrigin = (int)Image.Origin360.Top;
        TimerFillImage.fillClockwise = false;
        TimerFillImage.fillAmount = 1f;

        RectTransform imageRect = TimerFillImage.GetComponent<RectTransform>();
        imageRect.sizeDelta = new Vector2(1, 1);
        imageRect.localPosition = Vector3.zero;
    }

    public void CalculateTimer()
    {
        if (MoneyManager.Instance.Debt >= 250)
        {
            orderDuration = 50f;
        }
        else if (MoneyManager.Instance.Debt >= 150)
        {
            orderDuration = 40f;
        }
        else if (MoneyManager.Instance.Debt >= 50)
        {
            orderDuration = 30f;
        }
        else if (MoneyManager.Instance.Debt > 0)
        {
            orderDuration = 20f;
        }

    }
}