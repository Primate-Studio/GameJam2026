using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    GameWin
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GameState> OnGameStateChanged;

    [SerializeField] private GameState currentState;
    public GameState CurrentState => currentState;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

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

    // Update is called once per frame
    void Update()
    {
        
    }

      public void StartNewGame()
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


}
