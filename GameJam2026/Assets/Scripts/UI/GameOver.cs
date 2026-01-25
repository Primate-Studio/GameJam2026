using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Button mainMenuButton;
    public Button retryButton;

    private void OnEnable()
    {
        if (retryButton) retryButton.onClick.AddListener(HandleRetry);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(HandleMainMenu);
    }
    private void OnDisable()
    {
        if (retryButton) retryButton.onClick.RemoveListener(HandleRetry);
        if (mainMenuButton) mainMenuButton.onClick.RemoveListener(HandleMainMenu);
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
}
