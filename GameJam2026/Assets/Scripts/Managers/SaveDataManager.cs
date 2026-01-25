using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        //currentDay, totalMoney, debt 
        PlayerPrefs.SetInt("CurrentDay", DayCycleManager.Instance.currentDay);
        PlayerPrefs.SetFloat("TotalMoney", MoneyManager.Instance.totalMoney);
        PlayerPrefs.SetFloat("Debt", MoneyManager.Instance.Debt); 
        PlayerPrefs.Save();
        Debug.Log("Game Saved! \nDay: " + DayCycleManager.Instance.currentDay + 
                  "\nTotal Money: " + MoneyManager.Instance.totalMoney + 
                  "\nDebt: " + MoneyManager.Instance.Debt);
    }
}