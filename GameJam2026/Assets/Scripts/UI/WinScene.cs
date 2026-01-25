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

    public void HandleMainMenu()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }
    public void HandleEternalMode()
    {
        GameManager.Instance.StartEternalMode();
    }
}
