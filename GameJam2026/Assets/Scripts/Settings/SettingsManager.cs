using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("References")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Video Settings")]
    [SerializeField] private Image brightnessOverlay;

    private void Awake()
    {
        if(Instance == null) {Instance = this; DontDestroyOnLoad(gameObject);}
        else {Destroy(gameObject); return;}
    }

    //--------AUDIO SETTINGS-----------

    public void SetVolume (string parameterName, float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        Debug.Log($"Canviant {parameterName}. Valor Slider: {value} -> Decibels: {dB}");
        mainMixer.SetFloat(parameterName, dB);
        PlayerPrefs.SetFloat(parameterName, value);
    }
    public void SetMute (string parameterName, bool isMuted)
    {
       float value = isMuted ? -80f : Mathf.Log10(PlayerPrefs.GetFloat(parameterName, 0.75f)) * 20;
       mainMixer.SetFloat(parameterName, value);
       PlayerPrefs.SetInt(parameterName + "_muted", isMuted ? 1 : 0);
    }

    //--------VIDEO & ACCESSIBILITY SETTINGS-----------

    public void SetBrightness(float value)
    {
        if (brightnessOverlay != null)
        {
            // Ajustem l'alfa d'una imatge negra per simular brillantor
            Color c = brightnessOverlay.color;
            c.a = 1f - value; // Si value és 1 (màxim), l'alfa és 0 (transparent)
            brightnessOverlay.color = c;
        }
        PlayerPrefs.SetFloat("Brightness", value);
    }
    public void SetResolution(int index)
    {
        Resolution res = Screen.resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
    }

    //--------LANGUAGE SETTINGS-----------
    
    public void ChangeLanguage(string languageCode)
    {
        // Implementation depends on localization system used
    }

    //--------PERSISTENCE-----------

    public void LoadSettings()
    {
        // Audio
        SetVolume("MasterVolume", PlayerPrefs.GetFloat("MasterVolume", 0.75f));
        SetVolume("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", 0.75f));
        SetVolume("SFXVolume", PlayerPrefs.GetFloat("SFXVolume", 0.75f));
        SetMute("MasterVolume", PlayerPrefs.GetInt("MasterVolume_muted", 0) == 1);
        SetMute("MusicVolume", PlayerPrefs.GetInt("MusicVolume_muted", 0) == 1);
        SetMute("SFXVolume", PlayerPrefs.GetInt("SFXVolume_muted", 0) == 1);

        // Video
        SetBrightness(PlayerPrefs.GetFloat("Brightness", 1f));
       
    }
}
