using UnityEngine;

public enum DebtLevel { Zero ,None, LowLow, Low, Medium, High }
public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    private DebtLevel debtLevel = DebtLevel.High;
    [Header("Configuration")]
    public float Debt = 250f;
    public float InventoryCost = 2f;
    public float deathPenalty = 10f;
    public float debtPaymentRate = 0.25f; 
    public float successReward = 8f;

    [HideInInspector] public float currentMoney = 0;
    [HideInInspector] public float totalMoney = 0;
    [HideInInspector] public float weaponPaycheck = 0;
    [HideInInspector] public float debtPaymentAmount = 0;
    [HideInInspector] public float itemsBenefits = 0;

    [HideInInspector] public int deathCount = 0;
    [HideInInspector] public int successCount = 0;
    [HideInInspector] public int totalItemsSold = 0;

    

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        Debt = PlayerPrefs.GetFloat("Debt", 250);
        totalMoney = PlayerPrefs.GetFloat("TotalMoney", 0);
    }

    public void AddMoney(float amount)
    {
        currentMoney += amount;
        itemsBenefits += amount;
    }

    public void SubtractMoney(float amount)
    {
        currentMoney -= amount;
        totalItemsSold ++;
    }

    public void successMoney()
    {
        currentMoney += successReward;
        successCount++;
    }

    public void deadthPenaltyMoney()
    {
        currentMoney -= deathPenalty;
        deathCount++;
    }

    public float debtPayment()
    {
        if(currentMoney <= 0) return 0f;
        debtPaymentAmount = currentMoney * debtPaymentRate;
        Debt -= debtPaymentAmount;
        return debtPaymentAmount;
    }
    
    public void CalculateTotalMoney()
    {
        totalMoney += currentMoney - InventoryCost*totalItemsSold;
        totalMoney -= debtPayment();
        CheckWinLoseConditions();
    }
    public void CalculateDebtLevel()
    {
        if (Debt > 70 && Debt <= 130)
        {
            debtLevel = DebtLevel.LowLow;
        }
        else if (Debt > 130 && Debt <= 180)
        {
            debtLevel = DebtLevel.Low;
        }
        else if (Debt > 180 && Debt <= 220)
        {
            debtLevel = DebtLevel.Medium;
        }
        else if( Debt > 220 )
        {
            debtLevel = DebtLevel.High;
        }
        else if( Debt <= 70 && Debt > 0 )
        {
            debtLevel = DebtLevel.None;
        }
        else if( Debt <= 0 )
        {
            Debt = 0;
            debtLevel = DebtLevel.Zero;
        }
    }
    public DebtLevel DebtLevel
    {
        get { return debtLevel; }
    }
    private void CheckWinLoseConditions()
    {
        if (Debt <= 0)
        {
            Debt = 0;
            GameManager.Instance.ChangeState(GameState.GameWin);
            return;
        }
        if (totalMoney < 0)
        {
            GameManager.Instance.ChangeState(GameState.GameOver);
        }
    }

    public void ResetDayPaycheck()
    {
        itemsBenefits = 0;
        weaponPaycheck = 0;
        InventoryCost = 2;
        currentMoney = 0;
        deathCount = 0;
        successCount = 0;
        totalItemsSold = 0;
        debtPaymentAmount = 0;
    }

    public void ResetMoney()
    {
        itemsBenefits = 0;
        debtPaymentAmount = 0;
        currentMoney = 0;
        totalMoney = 0;
        InventoryCost = 2f;
        weaponPaycheck = 0;
        deathCount = 0;
        successCount = 0;
        totalItemsSold = 0;
        Debt = 250;
    }
    
}   
