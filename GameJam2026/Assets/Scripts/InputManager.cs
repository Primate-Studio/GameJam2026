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

    public float Horizontal => Input.GetAxis("Horizontal");
    public float Vertical => Input.GetAxis("Vertical");

    public bool JumpPressed => Input.GetKeyDown(jumpKey);
    public bool InteractPressed => Input.GetKeyDown(interactKey);
    public bool PausePressed => Input.GetKeyDown(pauseKey);
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
    public bool HasMovementInput()
    {
        return !Mathf.Approximately(Horizontal, 0f) || !Mathf.Approximately(Vertical, 0f);
    }
}
