using UnityEngine;
using UnityEngine.Audio;
using System;

[ExecuteInEditMode]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer & Routing")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Pitch Variation (Game Feel)")]
    [Range(0.5f, 1.5f)] [SerializeField] private float minPitch = 0.95f;
    [Range(0.5f, 1.5f)] [SerializeField] private float maxPitch = 1.05f;

    [Header("Categories")]
    public SoundCategory[] musicList;
    public SoundCategory[] sfxList;
    public SoundCategory[] uiList;
    public SoundCategory[] ambientList;

    private AudioSource musicSource, sfxSource, uiSource, ambientSource;

    // Aquest mètode elimina el warning i assegura que la instància estigui neta
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        SetupSources();
    }

    private void SetupSources()
    {
        // Netegem fills antics si n'hi hagués (útil en l'editor)
        foreach (Transform child in transform) { if (Application.isEditor) DestroyImmediate(child.gameObject); }

        musicSource = GetOrCreateSource("MusicSource", "Music");
        sfxSource = GetOrCreateSource("SFXSource", "SFX");
        uiSource = GetOrCreateSource("UISource", "UI");
        ambientSource = GetOrCreateSource("AmbientSource", "Ambient");
        PlayMusic(MusicType.MainMenu);
    }
    private AudioSource GetOrCreateSource(string name, string group)
    {
        // 1. Busquem si ja existeix un fill amb aquest nom
        Transform existingChild = transform.Find(name);
        
        if (existingChild != null)
        {
            // Si existeix, retornem el seu AudioSource
            return existingChild.GetComponent<AudioSource>();
        }

        // 2. Si no existeix, el creem com feies abans
        GameObject obj = new GameObject(name);
        obj.transform.parent = transform;
        AudioSource source = obj.AddComponent<AudioSource>();
        
        // Assignem el grup del Mixer
        var groups = mainMixer.FindMatchingGroups(group);
        if (groups.Length > 0)
        {
            source.outputAudioMixerGroup = groups[0];
        }
        
        return source;
    }

    private AudioSource CreateSound(string name, string group)
    {
        GameObject obj = new GameObject(name);
        obj.transform.parent = transform;
        AudioSource source = obj.AddComponent<AudioSource>();
        
        var groups = mainMixer.FindMatchingGroups(group);
        if (groups.Length > 0) source.outputAudioMixerGroup = groups[0];
        
        return source;
    }

    //--------------------------------------------------------------------------------
    // Mètodes de Reproducció

    public void PlaySFX(SFXType type, bool randomPitch = true) => PlayRandom(type, sfxList, sfxSource, randomPitch);
    public void PlayUI(UIType type, bool randomPitch = false) => PlayRandom(type, uiList, uiSource, randomPitch);
    public void PlayAmbient(AmbientType type)
    {
        int index = (int)type;
        if (index >= ambientList.Length || ambientList[index].clips.Length == 0) return;
        
        ambientSource.clip = ambientList[index].clips[0];
        ambientSource.volume = ambientList[index].volume;
        ambientSource.loop = true;
        ambientSource.Play();
    }

    public void StopAmbient()
{
    if (ambientSource != null && ambientSource.isPlaying)
    {
        ambientSource.Stop();
    }
}

    public void PlayMusic(MusicType type)
    {
        int index = (int)type;
        
        // 1. Validació de seguretat
        if (index >= musicList.Length || musicList[index].clips.Length == 0) return;
        
        AudioClip clipAPosar = musicList[index].clips[0];

        // 2. Si ja està sonant aquest mateix clip, no fem res
        if (musicSource.clip == clipAPosar && musicSource.isPlaying)
        {
            return; 
        }

        // 3. Si el clip és el que toca però està pausat o aturat, el reactivem
        if (musicSource.clip == clipAPosar && !musicSource.isPlaying)
        {
            musicSource.Play();
            return;
        }

        // 4. Si és un clip nou (o el primer de tots), el configurem i l'engeguem
        musicSource.clip = clipAPosar;
        musicSource.volume = musicList[index].volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    private void PlayRandom(Enum type, SoundCategory[] list, AudioSource source, bool useRandomPitch)
    {
        int index = Convert.ToInt32(type);
        if (index >= list.Length || list[index].clips.Length == 0) return;

        var category = list[index];
        AudioClip clip = category.clips[UnityEngine.Random.Range(0, category.clips.Length)];

        // Apliquem pitch aleatori per donar varietat (com en el pas de pàgina)
        //source.pitch = useRandomPitch ? UnityEngine.Random.Range(minPitch, maxPitch) : 1f;
        source.PlayOneShot(clip, category.volume);
    }

    public AudioClip GetSFXClip(SFXType type)
    {
        if (sfxList == null) return null;

        int index = (int)type;
        if (index < 0 || index >= sfxList.Length) return null;

        var clips = sfxList[index].clips;
        if (clips == null || clips.Length == 0) return null;

        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }
    //--------------------------------------------------------------------------------
    // Lògica de l'Editor (Auto-nomena les llistes segons l'Enum)
    private void OnEnable()
    {
        UpdateListNames<MusicType>(ref musicList);
        UpdateListNames<SFXType>(ref sfxList);
        UpdateListNames<UIType>(ref uiList);
        UpdateListNames<AmbientType>(ref ambientList);
    }

    private void UpdateListNames<T>(ref SoundCategory[] list) where T : Enum
    {
        string[] names = Enum.GetNames(typeof(T));
        if (list == null || list.Length != names.Length) Array.Resize(ref list, names.Length);
        for (int i = 0; i < names.Length; i++) list[i].name = names[i];
    }
}