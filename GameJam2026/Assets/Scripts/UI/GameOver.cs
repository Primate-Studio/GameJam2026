using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Button mainMenuButton;
    public Button retryButton;
    public Button nextSceneButton;

    private void OnEnable()
    {
        if (retryButton) retryButton.onClick.AddListener(HandleRetry);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(HandleMainMenu);
        if( nextSceneButton) nextSceneButton.onClick.AddListener(HandleNextScene);
    }
    private void OnDisable()
    {
        if (retryButton) retryButton.onClick.RemoveListener(HandleRetry);
        if (mainMenuButton) mainMenuButton.onClick.RemoveListener(HandleMainMenu);
        if (nextSceneButton) nextSceneButton.onClick.RemoveListener(HandleNextScene);
    }

    public void HandleMainMenu()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
        PlayerPrefs.DeleteAll();
    }
    public void HandleRetry()
    {
        GameManager.Instance.StartNewGame();
    }
    public void HandleNextScene()
    {
        GameManager.Instance.ChangeState(GameState.Lose);
    }
}
