using UnityEngine;
using UnityEngine.UI;

public class SettingsButtons : MonoBehaviour
{
    public static SettingsButtons Instance { get; private set; }
    [Header("GeneralSettings References")]
    [SerializeField] private Button BackButton;
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject GeneralSettingsCanvas;

    [Header("Audio Settings References")]      
    [SerializeField] private Slider GeneralVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;
    [SerializeField] private Slider AmbientVolumeSlider;
    [SerializeField] private Slider MusicVolumeSlider;

    [Header("Video Settings References")]
    [SerializeField] private Toggle FullscreenToggle;
    [SerializeField] private Toggle VSyncToggle;
    SettingsManager settingsmanager;
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
    void Start()
    {
        settingsmanager = SettingsManager.Instance;
        settingsmanager.LoadSettings();
        SetSettingsUI();
    }

    private void OnEnable()
    {
        if (BackButton != null)
            BackButton.onClick.AddListener(OnBackButton);
    }

    private void OnDisable()
    {
        if (BackButton != null)
            BackButton.onClick.RemoveListener(OnBackButton);
    }

    //--------BUTTON METHODS-----------
    public void OnPause(bool isPaused)
    {
        GeneralSettingsCanvas.SetActive(isPaused);
    }
    public void OnBackButton()
    {  
        InputManager.Instance.SetPauseState(false);
        GeneralSettingsCanvas.SetActive(false);
    }

    public void OnGeneralVolumeChanged(float value)
    {
        settingsmanager.SetVolume("MasterVolume", value);
    }
    public void OnSFXVolumeChanged(float value)
    {
        settingsmanager.SetVolume("SFXVolume", value);
    }
    public void OnAmbientVolumeChanged(float value)
    {
        settingsmanager.SetVolume("AmbientVolume", value);
    }
    public void OnMusicVolumeChanged(float value)
    {
        settingsmanager.SetVolume("MusicVolume", value);
    }

    public void onFullscreenToggleChanged(bool isFullscreen)
    {
        #if UNITY_STANDALONE_WIN
        if (isFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow; // borderless
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, true);
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.SetResolution(1280, 720, false); // elige tamaño de ventana
        }
        #else
            Screen.fullScreen = isFullscreen;
        #endif
    }
    public void SetVSync(bool isTarget)
    {
        QualitySettings.vSyncCount = isTarget ? 1 : 0;
        Application.targetFrameRate = isTarget ? -1 : 60; 
    }
    
    public void SetSettingsUI()
    {
        GeneralVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        MusicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        SFXVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        AmbientVolumeSlider.value = PlayerPrefs.GetFloat("AmbientVolume", 0.75f);
        FullscreenToggle.isOn = Screen.fullScreen;
        VSyncToggle.isOn = QualitySettings.vSyncCount == 1 ? true : false;
    }
}



