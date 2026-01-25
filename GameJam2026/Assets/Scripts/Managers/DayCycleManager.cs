using UnityEngine;
using System;
using UnityEditor.PackageManager;

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance;

    [Header("Configuració del Temps")]
    public float dayDurationInSeconds = 300f; // 5 minuts
    private float currentTime = 0f;
    private bool isDayActive = false;
    public int currentDay = 1;


    [Header("Referències de Llum")]
    [SerializeField] private Light sunLight;
    [SerializeField] private AnimationCurve sunIntensity;
    [SerializeField] private Gradient sunColor;

    public static event Action OnDayStart;
    public static event Action OnDayEnd;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartDay(); // En una versió final, això es cridaria des d'un botó "Obrir Botiga"
        currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
    }

    private void Update()
    {
        if (!isDayActive) return;

        currentTime += Time.deltaTime;
        float progress = currentTime / dayDurationInSeconds; // Valor de 0 a 1

        UpdateLighting(progress);

        if (currentTime >= dayDurationInSeconds || ClientManager.Instance.clientsCount >= ClientManager.Instance.maxClientsPerDay)
        {
            EndDay();
        }
    }

    public void StartDay()
    {
        currentTime = 0f;
        isDayActive = true;
        OnDayStart?.Invoke();
        ClientManager.Instance.clientsCount = 0;
        Debug.Log("<color=yellow>☀ La botiga ha obert!</color>");
        
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    public void NextDay()
    {
        currentDay++;
        SaveDataManager.Instance.SaveGame();
        currentTime = 0f;
        isDayActive = true;
        OnDayStart?.Invoke();
        ClientManager.Instance.CalculateTimer();
        ClientManager.Instance.clientsCount = 0;
        ClientManager.Instance.maxClientsPerDay += 3;

        GameManager.Instance.ChangeState(GameState.Playing);
    }

    private void EndDay()
    {
        isDayActive = false;
        OnDayEnd?.Invoke();
        Debug.Log("<color=red>🌙 La botiga ha tancat!</color>");
        
        GameManager.Instance.ChangeState(GameState.Result);
    }

    private void UpdateLighting(float progress)
    {
        if (sunLight == null) return;

        // Rotació del sol (de matí a nit)
        // El sol gira uns 180 graus sobre l'eix X
        float xRotation = Mathf.Lerp(0f, 180f, progress);
        sunLight.transform.rotation = Quaternion.Euler(xRotation, -90f, 0f);

        // Intensitat i color segons la corba i el gradient
        sunLight.intensity = sunIntensity.Evaluate(progress);
        sunLight.color = sunColor.Evaluate(progress);
    }
}