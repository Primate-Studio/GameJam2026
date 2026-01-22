using UnityEngine;
using UnityEngine.UI;

public class ClientTimer : MonoBehaviour
{
    public float orderDuration = 60f;
    private Image TimerFillImage;
    public Sprite spriteTimerFill;
    private DesperationLevel desperationLevel;

    private float timeRemaining;
    private bool isOrderActive = false;

    void Start()
    {
        StartNewOrderTimer();
    }

    void Update()
    {
        if (isOrderActive)
        {
            //Color change based on time remaining
            timeRemaining -= Time.deltaTime;
            TimerFillImage.fillAmount = timeRemaining / orderDuration;
            TimerFillImage.color = Color.Lerp(Color.red, Color.green, timeRemaining / orderDuration);
            CalculateDesperationLevel();

            if (timeRemaining <= 0f)
            {
                isOrderActive = false;
                TimerFillImage.fillAmount = 0f;
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

    public void StartNewOrderTimer()
    {
        CreateTimer();
        timeRemaining = orderDuration;
        isOrderActive = true;
        TimerFillImage.fillAmount = 1f;
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
        canvasGO.transform.localPosition = new Vector3(0, 1.65f, 0);
        GameObject imageGO = new GameObject("TimerFillImage");
        imageGO.transform.SetParent(canvasGO.transform);
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
}