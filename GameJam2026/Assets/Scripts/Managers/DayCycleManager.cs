using UnityEngine;
using System;

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance;    [Header("Configuració del Temps")]
    public float dayDurationInSeconds = 300f; // 5 minuts
    private float currentTime = 0f;
    private bool isDayActive = false;
    public int currentDay = 1;

    [Header("Referències de Llum")]
    [SerializeField] private Light sunLight;
    [SerializeField] private AnimationCurve sunIntensity;
    [SerializeField] private Gradient sunColor;

    [Header("Environment Lighting")]
    [SerializeField] private AnimationCurve environmentIntensity;
    [SerializeField] private AnimationCurve reflectionIntensity;

    [Header("Skybox")]
    [SerializeField] private Gradient skyboxTint;
    [SerializeField] private AnimationCurve skyboxExposure;

    public static event Action OnDayStart;
    public static event Action OnDayEnd;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartDay(); // En una versión final, esto se llamaría desde un botón "Abrir Tienda"
        
        currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
    }

    private void Update()
    {

        if (!isDayActive) return;

        currentTime += Time.deltaTime;
        float progress = currentTime / dayDurationInSeconds; // Valor de 0 a 1

        UpdateLighting(progress);

        bool allClientsServed = ClientManager.Instance.clientsCount >= ClientManager.Instance.maxClientsPerDay;
        bool noActiveClients = AreAllClientsDismissed();

        if (GameManager.Instance.CurrentState == GameState.Tutorial) return;
        if (currentTime >= dayDurationInSeconds || (allClientsServed && noActiveClients))
        {
            EndDay();
        }
    }

    public void StartDay()
    {
        if (GameManager.Instance.CurrentState == GameState.Tutorial)
        {
            currentDay = 0;
            isDayActive = true;
            currentTime = 0f;
            dayDurationInSeconds = 600f; 
            return;
        }
        currentTime = 0f;
        isDayActive = true;
        OnDayStart?.Invoke();
        ClientManager.Instance.clientsCount = 0;
        Debug.Log("<color=yellow>☀ La botiga ha obert!</color>");
        
        // NO cambiar el estado si estamos en tutorial
        if (GameManager.Instance.CurrentState != GameState.Tutorial)
        {
            GameManager.Instance.ChangeState(GameState.Playing);
        }
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
        GameState nextState = MoneyManager.Instance.GetNextStateAfterDay();
        GameManager.Instance.ChangeState(nextState);
    }
    public float GetProgress()
    {
        return Mathf.Clamp01(currentTime / dayDurationInSeconds);
    }    private void UpdateLighting(float progress)
    {
        if (sunLight == null) return;

        // Rotació del sol (de matí a nit)
        // El sol gira uns 180 graus sobre l'eix X
        float xRotation = Mathf.Lerp(0f, 180f, progress);
        sunLight.transform.rotation = Quaternion.Euler(xRotation, -180f, 0f);

        // Intensitat i color segons la corba i el gradient
        sunLight.intensity = sunIntensity.Evaluate(progress);
        sunLight.color = sunColor.Evaluate(progress);

        // Actualitzar Environment Lighting
        if (environmentIntensity != null)
        {
            RenderSettings.ambientIntensity = environmentIntensity.Evaluate(progress);
        }

        // Actualitzar Environment Reflections
        if (reflectionIntensity != null)
        {
            RenderSettings.reflectionIntensity = reflectionIntensity.Evaluate(progress);
            DynamicGI.UpdateEnvironment(); // Actualitza les reflections en temps real
        }

        // Actualitzar Skybox Tint i Exposure
        if (RenderSettings.skybox != null)
        {
            if (skyboxTint != null && RenderSettings.skybox.HasProperty("_Tint"))
            {
                RenderSettings.skybox.SetColor("_Tint", skyboxTint.Evaluate(progress));
            }

            if (skyboxExposure != null && RenderSettings.skybox.HasProperty("_Exposure"))
            {
                RenderSettings.skybox.SetFloat("_Exposure", skyboxExposure.Evaluate(progress));
            }
        }
    }
    private bool AreAllClientsDismissed()
    {
        // Verificar si hay algún cliente activo en la tienda
        for (int i = 0; i < ClientManager.Instance.targetPoints.Length; i++)
        {
            if (ClientManager.Instance.GetClientInSlot(i) != null)
            {
                return false; // Todavía hay clientes activos
            }
        }
        return true; // No hay clientes activos
    }
}