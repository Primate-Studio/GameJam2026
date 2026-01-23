using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{

    public Button newGameButton;
    public Button settingsButton;
    public Button exitButton;

    public void OnNewGamePressed()
    {
        GameManager.Instance.StartNewGame();
    }

    public void OnSettingsPressed()
    {
        
    }

    public void OnExitPressed()
    {
        Application.Quit();
    }

}