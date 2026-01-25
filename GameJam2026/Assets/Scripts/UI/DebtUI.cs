using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebtUI : MonoBehaviour
{
    public TextMeshProUGUI debtText;

    void Start()
    {
        debtText.text = MoneyManager.Instance.GetDebt().ToString() + "€";
    }
}
