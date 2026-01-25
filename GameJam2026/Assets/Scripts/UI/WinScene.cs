using UnityEngine;
using UnityEngine.UI;

public class WinScene : MonoBehaviour
{
    public Button mainMenuButton;
    public Button eternalModeButton;

    private void OnEnable()
    {
        if (mainMenuButton) mainMenuButton.onClick.AddListener(HandleMainMenu);
        if (eternalModeButton) eternalModeButton.onClick.AddListener(HandleEternalMode);
    }
    private void OnDisable()
    {
        if (mainMenuButton) mainMenuButton.onClick.RemoveListener(HandleMainMenu);
        if (eternalModeButton) eternalModeButton.onClick.RemoveListener(HandleEternalMode);
    }

    private void HandleMainMenu()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }
    private void HandleEternalMode()
    {
        GameManager.Instance.StartEternalMode();
    }
}
