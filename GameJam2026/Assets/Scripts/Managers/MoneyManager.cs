using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static MoneyManager Instance { get; private set; }

    [Header("Configuration")]
    public float Debt = 250;
    public int currentMoney = 0;
    public int totalMoney = 0;
    public int InventoryCost = 0;

    private int deathPenalty = 10;
    public int deathCount = 0;
    public int successCount = 0;
    public int totalItemsSold = 0;

    public int weaponPenalty = 5;
    public int weaponPaycheck = 0;

    float debtPaymentAmount = 0;
    

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
        
    }

    // Update is called once per frame
    void Update()
    {

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
        currentMoney += 8;
        successCount++;
    }

    public void deadthPenaltyMoney()
    {
        currentMoney -= deathPenalty;
        deathCount++;
    }

    public int debtPayment()
    {
        debtPaymentAmount = currentMoney * 0.25f;
        Debt -= debtPaymentAmount;
        return (int)debtPaymentAmount;
    }
    
    public void CalculateTotalMoney()
    {
        totalMoney += currentMoney - InventoryCost + weaponPaycheck;
        totalMoney -= debtPayment();
    }

    public void ResetDayPaycheck()
    {
        weaponPaycheck = 0;
        InventoryCost = 0;
        currentMoney = 0;
        deathCount = 0;
        successCount = 0;
        totalItemsSold = 0;
    }

    public void ResetMoney()
    {
        currentMoney = 0;
        totalMoney = 0;
        InventoryCost = 0;
        weaponPaycheck = 0;
        deathCount = 0;
        successCount = 0;
        totalItemsSold = 0;
        Debt = 250;
    }
    
}   
