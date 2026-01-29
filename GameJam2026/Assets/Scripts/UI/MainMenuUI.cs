using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Button newGameButton;
    public Button tutorialButton;
    public Button continueGameButton;
    public Button settingsButton;
    public Button exitButton;
    private SettingsButtons settingsButtons;

    void Start()
    {
        settingsButtons = FindAnyObjectByType<SettingsButtons>();
        continueGameButton.interactable = PlayerPrefs.HasKey("CurrentDay");
    }
    void OnEnable()
    {
        if (newGameButton) newGameButton.onClick.AddListener(OnNewGamePressed);
        if (tutorialButton) tutorialButton.onClick.AddListener(OnTutorialPressed);
        if (settingsButton) settingsButton.onClick.AddListener(OnSettingsPressed);
        if (continueGameButton) continueGameButton.onClick.AddListener(OnContinueGamePressed);
        if (exitButton) exitButton.onClick.AddListener(OnExitPressed);
    }
    public void OnNewGamePressed()
    {
        GameManager.Instance.StartNewGame();
    }

    public void OnTutorialPressed()
    {
        GameManager.Instance.TutorialMode();
    }
    public void OnSettingsPressed()
    {
        SettingsButtons.Instance.OnPause(true);
    }
    public void OnContinueGamePressed()
    {
        GameManager.Instance.ContinueGame();
    }

    public void OnExitPressed()
    {
        Application.Quit();
    }

}