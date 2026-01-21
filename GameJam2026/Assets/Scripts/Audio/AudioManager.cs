using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Micer & Routing")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Categories")]
    public SoundCategory[] musicList;
    public SoundCategory[] sfxList;
    public SoundCategory[] uiList;
    public SoundCategory[] ambientList;

    private AudioSource musicSource, sfxSource, uiSource, ambientSource;

    private void Awake()
    {
        if(Application.isPlaying)
        {
            if(Instance == null) {Instance = this; DontDestroyOnLoad(gameObject);}
            else {Destroy(gameObject); return;}
        }
        SetupSources();
    } 

    private void SetupSources()
    {
        musicSource = CreateSound("MusicSource","Music");
        sfxSource = CreateSound("SFXSource","SFX");
        uiSource = CreateSound("UISource","UI");
        ambientSource = CreateSound("AmbientSource","Ambient");
    }
    private AudioSource CreateSound(string name, string group)
    {
        GameObject obj = new GameObject(name);
        obj.transform.parent = transform;
        AudioSource source = obj.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = mainMixer.FindMatchingGroups(group)[0];
        return source;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Playback Logic
    
    public void PlaySFX(SFXType type) => PlayRandom(type, sfxList, sfxSource);
    public void PlayUI(UIType type) => PlayRandom(type, uiList, uiSource);

    public void PlayMusic(MusicType type)
    {
        var cat = musicList[(int) type];
        musicSource.clip = cat.clips[0];
        musicSource.loop = true;
        musicSource.Play();
    }
    public void PlayAmbient(AmbientType type)
    {
        var cat = ambientList[(int) type];
        ambientSource.clip = cat.clips[0];
        ambientSource.loop = true;
        ambientSource.Play();
    }
    
    private void PlayRandom(Enum type, SoundCategory[] list, AudioSource source)
    {
        int index = Convert.ToInt32(type);
        if(index >= list.Length || list[index].clips.Length == 0) return;

        var category = list[index];
        AudioClip clip = category.clips[UnityEngine.Random.Range(0, category.clips.Length)];
        source.PlayOneShot(clip, category.volume);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Editor Logic
    private void OnEnable()
    {
        UpdateListNames<MusicType>(ref musicList);
        UpdateListNames<SFXType>(ref sfxList);
        UpdateListNames<UIType>(ref uiList);
        UpdateListNames<AmbientType>(ref ambientList);
    }
    private void UpdateListNames<T>(ref SoundCategory[] list ) where T : Enum
    {
        string[] names = Enum.GetNames(typeof(T));
        Array.Resize(ref list, names.Length);
        for(int i = 0; i < names.Length; i++) list[i].name = names[i];
    }

}