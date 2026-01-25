using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Button newGameButton;
    public Button continueGameButton;
    public Button settingsButton;
    public Button exitButton;
    private SettingsButtons settingsButtons;

    void Start()
    {
        settingsButtons = FindAnyObjectByType<SettingsButtons>();
        continueGameButton.interactable = PlayerPrefs.HasKey("CurrentDay");
    }
    public void OnNewGamePressed()
    {
        GameManager.Instance.StartNewGame();
    }

    public void OnSettingsPressed()
    {
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