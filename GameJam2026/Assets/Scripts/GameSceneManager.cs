using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                LoadScene("MainMenu");
                break;

            case GameState.Playing:
                LoadScene("GameScene");
                break;

            case GameState.GameOver:
                LoadScene("GameOverScene");
                break;

            case GameState.GameWin:
                LoadScene("GameWinScene");
                break;
        }
    }

    private void LoadScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == sceneName)
            return;

        SceneManager.LoadScene(sceneName);
    }
}
