using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Sistema de diálogos del tutorial que gestiona las conversaciones, las restricciones 
/// durante los diálogos y el foco de la cámara
/// </summary>
public class TutorialDialogueSystem : MonoBehaviour
{
    public static TutorialDialogueSystem Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI dialogueText;
    public UnityEngine.UI.Image characterImage;
    public RawImage characterRawImage; // Para mostrar RenderTexture (cliente)
    public UnityEngine.UI.Image tutorialImage;
    public Button continueButton;
    public GameObject dialoguePanel;
    
    [Header("Character Sprites")]
    public Sprite dogSprite;
    public RenderTexture clientSprite;

    [Header("Tutorial Images")]
    public Sprite wasdSprite;
    public Sprite mouseSprite;
    public Sprite tabSprite;
    public Sprite manualSprite;
    public Sprite desperationSprite;
    public Sprite interactionSprite;

    private bool isWaitingForContinue = false;
    private bool isDialogueActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinuePressed);
            continueButton.gameObject.SetActive(false);
        }
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Permitir continuar con Enter
        if (isWaitingForContinue && Input.GetKeyDown(KeyCode.Return))
        {
            OnContinuePressed();
        }
    }

    /// <summary>
    /// Muestra un diálogo con texto, personaje opcional e imagen opcional
    /// </summary>
    public IEnumerator ShowDialogue(string text, Sprite character = null, Sprite image = null, bool waitForContinue = true, Transform lookAtTarget = null)
    {
        // IMPORTANTE: Bloquear TODO inmediatamente ANTES de cualquier otra cosa
        if (TutorialPlayerRestrictions.Instance != null)
        {
            TutorialPlayerRestrictions.Instance.SetFullRestriction(true);
        }
        
        // Activar cursor SOLO si hay botón de continuar
        if (waitForContinue)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Si no hay botón, mantener cursor bloqueado
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        isDialogueActive = true;

        // Hacer que el jugador mire al objetivo si se especifica
        if (lookAtTarget != null)
        {
            TutorialPlayerRestrictions.Instance?.LookAtTarget(lookAtTarget);
        }

        // Activar el panel de diálogo
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // Configurar el sprite del personaje
        if (characterImage != null)
        {
            if (character != null)
            {
                characterImage.sprite = character;
                characterImage.gameObject.SetActive(true);
                // Desactivar RawImage si está activo
                if (characterRawImage != null) characterRawImage.gameObject.SetActive(false);
            }
            else
            {
                characterImage.gameObject.SetActive(false);
            }
        }

        // Configurar la imagen tutorial
        if (tutorialImage != null)
        {
            if (image != null)
            {
                tutorialImage.sprite = image;
                tutorialImage.gameObject.SetActive(true);
            }
            else
            {
                tutorialImage.gameObject.SetActive(false);
            }
        }

        // Mostrar el texto con efecto de escritura
        if (dialogueText != null)
        {
            dialogueText.text = "";
            yield return StartCoroutine(TypeWritterEffect.TypeText(dialogueText, text, 0.05f));
        }

        // Esperar al botón de continuar si es necesario
        if (waitForContinue)
        {
            yield return StartCoroutine(WaitForContinue());
        }

        isDialogueActive = false;
    }

    /// <summary>
    /// Sobrecarga para mostrar RenderTexture (como el cliente)
    /// </summary>
    public IEnumerator ShowDialogue(string text, RenderTexture characterTexture, Sprite image = null, bool waitForContinue = true, Transform lookAtTarget = null)
    {
        // IMPORTANTE: Bloquear TODO inmediatamente ANTES de cualquier otra cosa
        if (TutorialPlayerRestrictions.Instance != null)
        {
            TutorialPlayerRestrictions.Instance.SetFullRestriction(true);
        }
        
        // Activar cursor SOLO si hay botón de continuar
        if (waitForContinue)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        isDialogueActive = true;

        // Hacer que el jugador mire al objetivo si se especifica
        if (lookAtTarget != null)
        {
            TutorialPlayerRestrictions.Instance?.LookAtTarget(lookAtTarget);
        }

        // Activar el panel de diálogo
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // Configurar el RenderTexture del personaje
        if (characterRawImage != null)
        {
            if (characterTexture != null)
            {
                characterRawImage.texture = characterTexture;
                characterRawImage.gameObject.SetActive(true);
                // Desactivar Image si está activo
                if (characterImage != null) characterImage.gameObject.SetActive(false);
            }
            else
            {
                characterRawImage.gameObject.SetActive(false);
            }
        }

        // Configurar la imagen tutorial
        if (tutorialImage != null)
        {
            if (image != null)
            {
                tutorialImage.sprite = image;
                tutorialImage.gameObject.SetActive(true);
            }
            else
            {
                tutorialImage.gameObject.SetActive(false);
            }
        }

        // Mostrar el texto con efecto de escritura
        if (dialogueText != null)
        {
            dialogueText.text = "";
            yield return StartCoroutine(TypeWritterEffect.TypeText(dialogueText, text, 0.05f));
        }

        // Esperar al botón de continuar si es necesario
        if (waitForContinue)
        {
            yield return StartCoroutine(WaitForContinue());
        }

        isDialogueActive = false;
    }

    /// <summary>
    /// Oculta el diálogo y reactiva al jugador
    /// </summary>
    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (tutorialImage != null)
        {
            tutorialImage.gameObject.SetActive(false);
        }

        // Restaurar suavemente la rotación de la cámara
        if (TutorialPlayerRestrictions.Instance != null)
        {
            TutorialPlayerRestrictions.Instance.RestoreCameraRotation();
        }

        // Desactivar restricciones del jugador
        if (TutorialPlayerRestrictions.Instance != null)
        {
            TutorialPlayerRestrictions.Instance.SetFullRestriction(false);
        }
        
        // Bloquear cursor de nuevo
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isDialogueActive = false;
    }

    /// <summary>
    /// Espera a que el jugador presione el botón de continuar
    /// </summary>
    private IEnumerator WaitForContinue()
    {
        isWaitingForContinue = true;

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }

        // Esperar hasta que se presione el botón
        while (isWaitingForContinue)
        {
            yield return null;
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }
    }

    private void OnContinuePressed()
    {
        if (isWaitingForContinue)
        {
            isWaitingForContinue = false;
        }
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinuePressed);
        }
    }
}
