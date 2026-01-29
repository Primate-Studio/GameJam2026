using UnityEngine;
using UnityEngine.UI;

public class WinScene : MonoBehaviour
{
    public Button mainMenuButton;
    public Button eternalModeButton;
    public Button nextSceneButton;

    private void OnEnable()
    {
        if (mainMenuButton) mainMenuButton.onClick.AddListener(HandleMainMenu);
        if (eternalModeButton) eternalModeButton.onClick.AddListener(HandleEternalMode);
        if( nextSceneButton) nextSceneButton.onClick.AddListener(HandleNextScene);
    }
    private void OnDisable()
    {
        if (mainMenuButton) mainMenuButton.onClick.RemoveListener(HandleMainMenu);
        if (eternalModeButton) eternalModeButton.onClick.RemoveListener(HandleEternalMode);
        if (nextSceneButton) nextSceneButton.onClick.RemoveListener(HandleNextScene);
    }

    public void HandleMainMenu()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }
    public void HandleEternalMode()
    {
        GameManager.Instance.StartEternalMode();
    }
    public void HandleNextScene()
    {
        GameManager.Instance.ChangeState(GameState.Win);
    }
}
