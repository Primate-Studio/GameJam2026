using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{

    public Button newGameButton;
    public Button settingsButton;
    public Button exitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnNewGamePressed()
    {
        GameManager.Instance.StartNewGame();
    }

    public void OnSettingsPressed()
    {
        // Implement settings logic here
    }

    public void OnExitPressed()
    {
        Application.Quit();
    }

}