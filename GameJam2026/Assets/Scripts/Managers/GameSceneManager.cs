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
                AudioManager.Instance.PlayMusic(MusicType.MainMenu);
                break;

            case GameState.Playing:
                LoadScene("GameScene");
                AudioManager.Instance.PlayMusic(MusicType.Gameplay);
                break;

            case GameState.GameOver:
                LoadScene("ResultSceneLOSE");
                AudioManager.Instance.PlayMusic(MusicType.Lose);
                break;

            case GameState.GameWin:
                LoadScene("ResultSceneWIN");
                AudioManager.Instance.PlayMusic(MusicType.Win);
                break;
            case GameState.Result:
                LoadScene("ResultScene");
                AudioManager.Instance.PlayMusic(MusicType.Gameplay);
                break;
            case GameState.Tutorial:
                // Si TutorialScene no está en Build Settings, usar GameScene
                string tutorialSceneName = "nEWTutorialScene";
                
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
