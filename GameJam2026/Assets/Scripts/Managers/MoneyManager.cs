using UnityEngine;

public enum DebtLevel { None, LowLow, Low, Medium, High }
public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    private DebtLevel debtLevel = DebtLevel.High;
    [Header("Configuration")]
    public float Debt = 250;
    [HideInInspector]public int currentMoney = 0;
    [HideInInspector] public int totalMoney = 0;
    public int InventoryCost = 2;

    public int deathPenalty = 10;
    [HideInInspector] public int deathCount = 0;
    [HideInInspector] public int successCount = 0;
    [HideInInspector] public int totalItemsSold = 0;

    public int weaponPenalty = 5;
    [HideInInspector]public int weaponPaycheck = 0;

    [HideInInspector]float debtPaymentAmount = 0;

    public int successReward = 8;
    public float debtPaymentRate = 0.25f; 
    

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
    }

    public void SubtractMoney(int amount)
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

    public int debtPayment()
    {
        debtPaymentAmount = currentMoney * debtPaymentRate;
        Debt -= debtPaymentAmount;
        return (int)debtPaymentAmount;
    }
    
    public void CalculateTotalMoney()
    {
        totalMoney += currentMoney - InventoryCost*totalItemsSold + weaponPaycheck;
        totalMoney -= debtPayment();
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
        else if( Debt <= 70)
        {
            debtLevel = DebtLevel.None;
        }
    }
    public DebtLevel DebtLevel
    {
        get { return debtLevel; }
    }

    public void ResetDayPaycheck()
    {
        weaponPaycheck = 0;
        InventoryCost = 2;
        currentMoney = 0;
        deathCount = 0;
        successCount = 0;
        totalItemsSold = 0;
    }

    public void ResetMoney()
    {
        currentMoney = 0;
        totalMoney = 0;
        InventoryCost = 2;
        weaponPaycheck = 0;
        deathCount = 0;
        successCount = 0;
        totalItemsSold = 0;
        Debt = 250;
    }
    
}   
