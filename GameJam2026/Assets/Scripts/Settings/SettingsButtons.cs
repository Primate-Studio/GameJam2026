using UnityEngine;
using UnityEngine.UI;

public class SettingsButtons : MonoBehaviour
{
    public static SettingsButtons Instance { get; private set; }
    [Header("GeneralSettings References")]
    [SerializeField] private Button BackButton;
    [SerializeField] private GameObject GeneralSettingsPanel;
    [SerializeField] private GameObject GeneralSettingsCanvas;

    [SerializeField] private GameObject AudioSettingsPanel;
    [SerializeField] private GameObject VideoSettingsPanel;
    [SerializeField] private Button AudioSettingsButton;
    [SerializeField] private Button VideoSettingsButton;

    [Header("Audio Settings References")]      
    [SerializeField] private Slider GeneralVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;
    [SerializeField] private Slider AmbientVolumeSlider;
    [SerializeField] private Slider MusicVolumeSlider;

    [SerializeField] private Toggle GeneralMuteToggle;
    [SerializeField] private Toggle SFXMuteToggle; 
    [SerializeField] private Toggle AmbientMuteToggle;
    [SerializeField] private Toggle MusicMuteToggle;

    [Header("Video Settings References")]
    [SerializeField] private Slider BrightnessSlider;
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

    //--------BUTTON METHODS-----------
    public void OnPause(bool isPaused)
    {
        GeneralSettingsCanvas.SetActive(isPaused);
        GeneralSettingsPanel.SetActive(isPaused); 
    }
    public void OnAudioSettingsButton()
    {
        AudioSettingsPanel.SetActive(true);
        GeneralSettingsPanel.SetActive(false);
        VideoSettingsPanel.SetActive(false);
    }
    public void OnVideoSettingsButton()
    {
        VideoSettingsPanel.SetActive(true);
        GeneralSettingsPanel.SetActive(false);
        AudioSettingsPanel.SetActive(false);
    }
    public void OnBackButton()
    {
        GeneralSettingsPanel.SetActive(true);
        AudioSettingsPanel.SetActive(false);
        VideoSettingsPanel.SetActive(false);
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

    public void OnGeneralMuteToggled(bool isMuted)
    {
        settingsmanager.SetMute("MasterVolume", isMuted);
    }
    public void OnSFXMuteToggled(bool isMuted)
    {
        settingsmanager.SetMute("SFXVolume", isMuted);
        AudioManager.Instance.PlayMusic(MusicType.MainMenu);
        AudioManager.Instance.PlayAmbient(AmbientType.Birds);
    }
    public void OnAmbientMuteToggled(bool isMuted)
    {
        settingsmanager.SetMute("AmbientVolume", isMuted);
    }
    public void OnMusicMuteToggled(bool isMuted)
    {
        settingsmanager.SetMute("MusicVolume", isMuted);
    }
    
    public void SetSettingsUI()
    {
        GeneralVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        MusicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        SFXVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        GeneralMuteToggle.isOn = PlayerPrefs.GetInt("MasterVolume_muted", 0) == 1;
        MusicMuteToggle.isOn = PlayerPrefs.GetInt("MusicVolume_muted", 0) == 1;
        SFXMuteToggle.isOn = PlayerPrefs.GetInt("SFXVolume_muted", 0) == 1;
        BrightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 1f);
    }
}



