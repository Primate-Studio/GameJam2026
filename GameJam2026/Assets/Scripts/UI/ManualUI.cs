using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ManualUI : MonoBehaviour
{
    public GameObject manualPanel;
    public static bool IsOpen { get; private set; } = false;
    [Header("Contingut")]
    [SerializeField] private Sprite[] pageSprites; // Pàgines normals (es veuen bé a escala 1)
    [SerializeField] private Sprite[] pageTurningSprites; // Pàgines invertides (es veuen bé a escala -1)
    
    [Header("Referències UI")]
    [SerializeField] private Image leftStaticPage;   
    [SerializeField] private Image rightStaticPage;  
    [SerializeField] private Image flippingPageImage; 
    [SerializeField] private RectTransform flippingPageTransform;

    [Header("Configuració")]
    [SerializeField] private float flipSpeed = 0.2f;

    private int currentLeftPageIndex = 0; 
    public bool pageHasChanged = false;
    private bool isAnimating = false;
    private bool isPopUpAnimating = false;

    void Start()
    {
        manualPanel.SetActive(false);
        flippingPageTransform.gameObject.SetActive(false);
        IsOpen = false;
        UpdateStaticPages();
    }

    void Update()
    {
        if (manualPanel.activeSelf && !isAnimating && !isPopUpAnimating)
        {
            // Verificació pel TutorialManager
            if (GameManager.Instance.CurrentState == GameState.Tutorial && 
                TutorialManager.Instance != null && !TutorialManager.Instance.canPlayerChangePage) return;

            // Comprovació d'input (GetAxis retorna float, mirem si és major a 0)
            if (InputManager.Instance.ManualNext) NextPage(); 
            else if (InputManager.Instance.ManualPrev) PrevPage(); 
        }
    }
    private IEnumerator PopUp()
    {
        isPopUpAnimating = true;
        float duration = 0.5f;
        Vector3 targetScale = Vector3.one;

        // Comencem des de zero
        manualPanel.transform.localScale = Vector3.zero;
        float elapsed = 0;

        // Creixem fins a 1.1 (menys rebote)
        while (elapsed < duration * 0.7f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.7f);
            manualPanel.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale * 1.02f, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return null;
        }

        // Tornem al tamany normal (1.0)
        elapsed = 0;
        Vector3 currentScale = manualPanel.transform.localScale;
        while (elapsed < duration * 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.3f);
            manualPanel.transform.localScale = Vector3.Lerp(currentScale, targetScale, t);
            yield return null;
        }
        
        manualPanel.transform.localScale = targetScale;
        isPopUpAnimating = false;
    }
    private IEnumerator PopDown()
    {
        isPopUpAnimating = true;
        float duration = 0.25f;

        // Disminuïm d'1.0 a 0
        float elapsed = 0;
        Vector3 currentScale = manualPanel.transform.localScale;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            manualPanel.transform.localScale = Vector3.Lerp(currentScale, Vector3.zero, t);
            yield return null;
        }

        manualPanel.transform.localScale = Vector3.zero;
        isPopUpAnimating = false;
        manualPanel.SetActive(false);
        IsOpen = false;
    }

    public void NextPage()
    {
        if (currentLeftPageIndex + 2 < pageSprites.Length && !isAnimating)
            StartCoroutine(FlipForward());
    }

    public void PrevPage()
    {
        if (currentLeftPageIndex - 2 >= 0 && !isAnimating)
            StartCoroutine(FlipBackward());
    }

    private IEnumerator FlipForward()
    {
        isAnimating = true;
        pageHasChanged = true; 

        // 1. Fulla que s'aixeca (Cara A - Pàgina de la Dreta actual)
        // Com que l'escala és 1, usem l'sprite NORMAL
        flippingPageImage.sprite = pageSprites[currentLeftPageIndex + 1];
        flippingPageTransform.gameObject.SetActive(true);
        flippingPageTransform.localScale = Vector3.one;

        // El fons dret ja mostra la següent parella (ex: Pg 4)
        rightStaticPage.sprite = pageSprites[currentLeftPageIndex + 3];

        float elapsed = 0;
        while (elapsed < flipSpeed)
        {
            elapsed += Time.deltaTime;
            flippingPageTransform.localScale = new Vector3(Mathf.Lerp(1, 0, elapsed / flipSpeed), 1, 1);
            yield return null;
        }

        // 2. CENTRE: Ara posem l'sprite GIRAT (Pàgina 3 o 5)
        // Com que ara l'escala anirà cap a -1, si l'sprite ja està girat de base, es veurà BÉ.
        flippingPageImage.sprite = pageTurningSprites[currentLeftPageIndex + 2]; 

        elapsed = 0;
        while (elapsed < flipSpeed)
        {
            elapsed += Time.deltaTime;
            flippingPageTransform.localScale = new Vector3(Mathf.Lerp(0, -1, elapsed / flipSpeed), 1, 1);
            yield return null;
        }

        currentLeftPageIndex += 2;
        UpdateStaticPages();
        flippingPageTransform.gameObject.SetActive(false);
        isAnimating = false;
    }

    private IEnumerator FlipBackward()
    {
        isAnimating = true;
        pageHasChanged = true;

        // 1. Fulla que s'aixeca des de l'esquerra (Escala -1)
        // Com que comencem a l'esquerra, hem d'usar l'sprite GIRAT per a què es vegi bé
        flippingPageImage.sprite = pageTurningSprites[currentLeftPageIndex];
        flippingPageTransform.gameObject.SetActive(true);
        flippingPageTransform.localScale = new Vector3(-1, 1, 1);

        // El fons esquerre canvia al passat (ex: Pg 1)
        leftStaticPage.sprite = pageSprites[currentLeftPageIndex - 2];

        float elapsed = 0;
        while (elapsed < flipSpeed)
        {
            elapsed += Time.deltaTime;
            flippingPageTransform.localScale = new Vector3(Mathf.Lerp(-1, 0, elapsed / flipSpeed), 1, 1);
            yield return null;
        }

        // 2. CENTRE: Ara posem l'sprite NORMAL (la que anirà a la dreta)
        // Com que l'escala anirà de 0 a 1, l'sprite normal es veurà BÉ.
        flippingPageImage.sprite = pageSprites[currentLeftPageIndex - 1];

        elapsed = 0;
        while (elapsed < flipSpeed)
        {
            elapsed += Time.deltaTime;
            flippingPageTransform.localScale = new Vector3(Mathf.Lerp(0, 1, elapsed / flipSpeed), 1, 1);
            yield return null;
        }

        currentLeftPageIndex -= 2;
        UpdateStaticPages();
        flippingPageTransform.gameObject.SetActive(false);
        isAnimating = false;
    }

    private void UpdateStaticPages()
    {
        leftStaticPage.sprite = pageSprites[currentLeftPageIndex];
        rightStaticPage.sprite = pageSprites[currentLeftPageIndex + 1];
    }

    public void OpenManual() 
    { 
        manualPanel.SetActive(true); 
        UpdateStaticPages();
        IsOpen = true;
        StartCoroutine(PopUp());
    }

    public void CloseManual() 
    { 
        StartCoroutine(PopDown());        
        IsOpen = false;
    }
}