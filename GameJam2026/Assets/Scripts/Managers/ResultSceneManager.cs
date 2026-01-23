using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultSceneManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text benefitsText;
    [SerializeField] private TMP_Text lossesText;
    [SerializeField] private TMP_Text totalText;
    [SerializeField] private TMP_Text debtText;

    [Header("Botones")]
    [SerializeField] private Button nextDayButton;
    [SerializeField] private Button mainMenuButton;

    private void Start()
    {

    }

    private void OnEnable()
    {
        if (nextDayButton)  nextDayButton.onClick.AddListener(HandleNextDay);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(HandleMainMenu);
    }

    private void OnDisable()
    {
        if (nextDayButton)  nextDayButton.onClick.RemoveListener(HandleNextDay);
        if (mainMenuButton) mainMenuButton.onClick.RemoveListener(HandleMainMenu);
    }

    // Permite setear los datos desde otro sistema (GameManager, etc.)
    public void SetResults(int customers, int revenue, float rating)
    {
        if (lossesText)
            lossesText.text = $"Clientes: {customers}\nIngresos: {revenue}\nValoración: {rating:0.0}/5";
    }

    private void HandleNextDay()
    {
        // Empieza un nuevo día
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    private void HandleMainMenu()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }   
}