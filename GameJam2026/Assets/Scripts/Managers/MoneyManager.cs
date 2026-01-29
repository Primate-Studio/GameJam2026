using UnityEngine;

public enum DebtLevel { Zero, LowLow, Low, Medium, High }
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
    public float totalMoneyBeforeDebt;
    public bool isEternalMode = false;

    [HideInInspector] public float currentMoney = 0;
    [HideInInspector] public float totalMoney = 0;
    [HideInInspector] public float weaponPaycheck = 0;
    [HideInInspector] public float debtPaymentAmount = 0;
    [HideInInspector] public float itemsBenefits = 0;
    [HideInInspector] public float initialDayDebt = 0;

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
    public GameState GetNextStateAfterDay()
    {
        CalculateTotalMoney();
        
        if (isEternalMode) return GameState.Result;

        if (Debt <= 0)
        {
            Debt = 0;
            return GameState.GameWin;
            
        }
        else if (totalMoney < 0)
        {
            return GameState.GameOver;
        }
        else return GameState.Result;
    }
    public void AddMoney(float amount)
    {
        currentMoney += amount;
        itemsBenefits += amount;
        totalItemsSold++;
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
        initialDayDebt = Debt;
        if(currentMoney <= 0) return 0f;
        debtPaymentAmount = totalMoney * debtPaymentRate;
        Debt -= debtPaymentAmount;
        PlayerPrefs.SetFloat("Debt", Debt);
        return debtPaymentAmount;
    }
    
    public void CalculateTotalMoney()
    {
        float inventoryRestockCost = InventoryCost * totalItemsSold;
        float dailyBalance = currentMoney - inventoryRestockCost;
        if(isEternalMode)
        {
            // MODE ETERN: El benefici resta deute, la pèrdua suma deute
            // Si dailyBalance és -50, Debt - (-50) farà que el deute pugi a +50
            Debt -= dailyBalance;
            
            // En aquest mode, potser vols que el totalMoney (els teus estalvis) 
            // sigui simplement el que t'ha sobrat després de pagar deute, 
            // o pots deixar-lo a 0 i que tot vagi al deute.
            totalMoney = Mathf.Max(0, totalMoney + dailyBalance);
            
            // No hi ha un "debtPaymentAmount" fix perquè tot el benefici s'ha aplicat al deute
            debtPaymentAmount = dailyBalance;
        }
        else{
        totalMoney += dailyBalance;
        totalMoneyBeforeDebt = totalMoney;
        totalMoney -= debtPayment();
        }
    }
    public void CalculateDebtLevel()
    {

        if (Debt > 0 && Debt <= 60)
        {
            debtLevel = DebtLevel.LowLow;
        }
        else if (Debt > 60 && Debt <= 120)
        {
            debtLevel = DebtLevel.Low;
        }
        else if (Debt > 120 && Debt <= 180)
        {
            debtLevel = DebtLevel.Medium;
        }
        else if( Debt > 180 )
        {
            debtLevel = DebtLevel.High;
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
    public float GetDebt()
    {
        return Debt;
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
