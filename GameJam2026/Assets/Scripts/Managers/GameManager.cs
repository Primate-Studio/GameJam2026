using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    GameWin,
    Tutorial,
    Result
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GameState> OnGameStateChanged;

    [SerializeField] private GameState currentState;
    public GameState CurrentState => currentState;

    private void Awake()
    {
        // Singleton seguro
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ChangeState(GameState.MainMenu);
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
    }
    public void StartNewGame()
    {
        PlayerPrefs.DeleteAll();
        MoneyManager.Instance.ResetMoney();
        ChangeState(GameState.Playing);
    }
    public void ContinueGame()
    {
        ChangeState(GameState.Playing);
    }

    public void PauseGame()
    {
        ChangeState(GameState.Paused);
    }

    public void ResumeGame()
    {
        ChangeState(GameState.Playing);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("GameScene");
        currentState = GameState.Playing;
        OnGameStateChanged?.Invoke(currentState);
    }
    public void StartEternalMode()
    {
        currentState = GameState.Playing;
        OnGameStateChanged?.Invoke(currentState);
    }

    public void TutorialMode()
    {
        if(currentState == GameState.Tutorial)
        {
            
        }
    }
}
