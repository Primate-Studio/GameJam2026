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

private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void Start()
    {
        SetResults();
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

    public void SetResults()
    {
        benefitsText.text = "Missions exitoses: \n" + MoneyManager.Instance.successCount.ToString() + "*" + 
            MoneyManager.Instance.successReward.ToString() + " = " + 
            (MoneyManager.Instance.successCount * MoneyManager.Instance.successReward).ToString() + " € \n"
            + "Items venuts: \n" + MoneyManager.Instance.totalItemsSold.ToString() + " = " + 
            MoneyManager.Instance.currentMoney.ToString() + " € \n";
        lossesText.text = "Penalitzacions per morts: \n" + MoneyManager.Instance.deathCount.ToString() + "*" + 
            MoneyManager.Instance.deathPenalty.ToString() + " = -" + (MoneyManager.Instance.deathCount * MoneyManager.Instance.deathPenalty).ToString() + " € \n"
            + "Reposar inventari: \n" + MoneyManager.Instance.InventoryCost.ToString() + " € \n";
        
        totalText.text = "Total guanyat: " + MoneyManager.Instance.currentMoney.ToString() + " € \n";
        
        debtText.text = "Deute restant: " + MoneyManager.Instance.Debt.ToString() + " €" + "-" + MoneyManager.Instance.debtPayment().ToString() + " € = " + MoneyManager.Instance.Debt.ToString() + " €";
    }

    private void HandleNextDay()
    {
        DayCycleManager.Instance.NextDay();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        MoneyManager.Instance.ResetDayPaycheck();
    }

    private void HandleMainMenu()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }   
}