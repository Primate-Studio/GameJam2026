using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;

    private void Awake()
    {
        Instance = this;
    }
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
            case GameState.Result:
                LoadScene("ResultScene");
                break;
            case GameState.Tutorial:
                // Si TutorialScene no está en Build Settings, usar GameScene
                string tutorialSceneName = "TutorialScene";
                
                // Verificar si la escena existe en Build Settings
                bool sceneExists = false;
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    if (sceneName == tutorialSceneName)
                    {
                        sceneExists = true;
                        break;
                    }
                }
                
                if (sceneExists)
                {
                    LoadScene(tutorialSceneName);
                }
                else
                {
                    Debug.LogWarning($"⚠ TutorialScene no está en Build Settings, usando GameScene");
                    LoadScene("GameScene");
                }
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
