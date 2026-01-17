using UnityEngine;
using System.Collections.Generic;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Settings")]
    [SerializeField] private SoundData[] sounds;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayMusic(string Name)
    {
        SoundData sound = Array.Find(sounds, s => s.soundName == Name);
        if (sound == null) { Debug.LogWarning($"{Name}: sound not found!"); return; }
 
        musicSource.clip = sound.clip;
        musicSource.volume = sound.volume;
        musicSource.pitch = sound.pitch;
        musicSource.loop = sound.loop;
        musicSource.Play();

    }
    public void PlaySFX(string Name)
    {
        SoundData sound = Array.Find(sounds, s => s.soundName == Name);
        if (sound == null) { Debug.LogWarning($"{Name}: sound not found!"); return; }
 
        sfxSource.PlayOneShot(sound.clip, sound.volume);
    }
    public void StopMusic() {musicSource.Stop(); }
}
