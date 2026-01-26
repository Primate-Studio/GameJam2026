using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Key Bindings")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode manualKey = KeyCode.Tab;

    // Movement
    public float Horizontal => Input.GetAxis("Horizontal");
    public float Vertical => Input.GetAxis("Vertical");
    
    // Mouse
    public float MouseX => Input.GetAxis("Mouse X");
    public float MouseY => Input.GetAxis("Mouse Y");
    public float MouseScrollDelta => Input.mouseScrollDelta.y;

    public bool JumpPressed => Input.GetKeyDown(jumpKey);
    public bool InteractPressed => Input.GetKeyDown(interactKey);
    public bool PausePressed => Input.GetKeyDown(pauseKey);
    public bool ManualPressed => Input.GetKeyDown(manualKey);
    private bool isPaused = false;
    private GameState lastStateBeforePause = GameState.MainMenu;

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }
    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    private void Update()
    {
        if(PausePressed)
        {
            SetPauseState(!isPaused);
        }
        if(ManualPressed && !isPaused)
        {
            // En tutorial, verificar si puede abrir el manual
            if (GameManager.Instance.CurrentState == GameState.Tutorial && 
                TutorialManager.Instance != null)
            {
                // Solo permitir abrir manual si está esperando que lo abra
                if (!TutorialManager.Instance.isWaitingForManualOpen && 
                    !TutorialManager.Instance.isWaitingForManualClose)
                {
                    return;
                }
            }

            ManualUI manualUI = FindFirstObjectByType<ManualUI>();
            if(manualUI.manualPanel != null && !manualUI.manualPanel.activeSelf)
            {
                manualUI.OpenManual();
            }
            else if(manualUI != null && manualUI.manualPanel.activeSelf)
            {
                manualUI.CloseManual();
            }
            
        }
    }
    private void HandleGameStateChanged(GameState newState)
    {
        // Cada cop que el joc canviï d'estat (Win, Loss, etc.), actualitzem el cursor
        UpdateCursorState();
    }
    public void SetPauseState(bool pause)
    {
        if (pause)
        {
            lastStateBeforePause = GameManager.Instance.CurrentState;
            GameManager.Instance.ChangeState(GameState.Paused);
        }
        else
        {
            GameManager.Instance.ChangeState(lastStateBeforePause);
        }

        isPaused = pause;

        if (SettingsButtons.Instance != null)
            SettingsButtons.Instance.OnPause(isPaused);

        UpdateCursorState();
    }
    public void UpdateCursorState()
    {
        bool unlockCursor = isPaused || 
                        GameManager.Instance.CurrentState == GameState.MainMenu || 
                        GameManager.Instance.CurrentState == GameState.Result ||
                        GameManager.Instance.CurrentState == GameState.GameWin || 
                        GameManager.Instance.CurrentState == GameState.GameOver;

        Cursor.lockState = unlockCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = unlockCursor;
    }    
    public bool HasMovementInput()
    {
        return !Mathf.Approximately(Horizontal, 0f) || !Mathf.Approximately(Vertical, 0f);
    }
    
}
