using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultSceneManager : MonoBehaviour
{    
    [Header("UI")]
    [SerializeField] private TMP_Text exitMissionText;
    [SerializeField] private TMP_Text soldItemsText;
    [SerializeField] private TMP_Text failedMissionsText;
    [SerializeField] private TMP_Text restockInventoryText;
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
        debtText.text = MoneyManager.Instance.Debt.ToString() + "€";
        MoneyManager.Instance.CalculateTotalMoney();
        exitMissionText.text = MoneyManager.Instance.successCount.ToString() + " x " + MoneyManager.Instance.successReward.ToString() + "€" + " = " + (MoneyManager.Instance.successCount * MoneyManager.Instance.successReward).ToString() + "€";
        soldItemsText.text = MoneyManager.Instance.totalItemsSold.ToString() + " = " + MoneyManager.Instance.itemsBenefits.ToString() + "€";
        failedMissionsText.text = MoneyManager.Instance.deathCount.ToString() + " x " + MoneyManager.Instance.deathPenalty.ToString() + "€" + " = " + (MoneyManager.Instance.deathCount * MoneyManager.Instance.deathPenalty).ToString() + "€";
        restockInventoryText.text = (MoneyManager.Instance.InventoryCost * MoneyManager.Instance.totalItemsSold).ToString() + "€";

        totalText.text = MoneyManager.Instance.currentMoney + "€";
        if(MoneyManager.Instance.debtPaymentAmount > 0)
        debtText.text += " - " + MoneyManager.Instance.debtPaymentAmount.ToString() + "€" + " = " + MoneyManager.Instance.Debt.ToString() + "€";
    }

    private void HandleNextDay()
    {
        DayCycleManager.Instance.NextDay();
        //SaveDataManager.Instance.SaveGame();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        MoneyManager.Instance.ResetDayPaycheck();
    }

    private void HandleMainMenu()
    {
        DayCycleManager.Instance.currentDay++;
        SaveDataManager.Instance.SaveGame();
        MoneyManager.Instance.ResetMoney();
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }   
}