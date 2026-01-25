using UnityEngine;
using UnityEngine.UI;

public class DayCycleUI : MonoBehaviour
{
    [Header("Components UI")]
    public Slider timerSlider;
    public Image fillImage;        // La imatge de la barra que s'omple
    public Image iconImage;        // La icona que està dins del Handle

    [Header("Configuració Visual")]
    public Gradient timeGradient;  // El degradat (Taronja -> Groc -> Lila)
    public Sprite sunSprite;
    public Sprite moonSprite;
    
    [Range(0, 1)]
    public float transitionThreshold = 0.7f; // Punt on canvia a Lluna

    void Update()
    {
        if (DayCycleManager.Instance == null) return;

        // 1. Obtenir el progrés real del dia
        float progress = DayCycleManager.Instance.GetProgress();
        
        // 2. Actualitzar el valor del Slider
        timerSlider.value = progress;

        // 3. COLOR TRANSITIU: Avaluem el degradat segons el progrés
        fillImage.color = timeGradient.Evaluate(progress);

        // 4. CANVI D'ICONA
        if (progress < transitionThreshold)
        {
            iconImage.sprite = sunSprite;
        }
        else
        {
            iconImage.sprite = moonSprite;
        }
    }
}