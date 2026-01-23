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
    private bool isPaused = false;
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
            SettingsButtons.Instance.OnPause(isPaused);
            isPaused = !isPaused;
            GameManager.Instance.ChangeState(isPaused ? GameState.Paused : GameState.Playing);
        }
    }
    public bool HasMovementInput()
    {
        return !Mathf.Approximately(Horizontal, 0f) || !Mathf.Approximately(Vertical, 0f);
    }
    
}
