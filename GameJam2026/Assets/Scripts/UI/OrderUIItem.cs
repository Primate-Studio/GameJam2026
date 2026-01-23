using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderUIItem : MonoBehaviour
{
    [Header("Elements UI")]
    public Image portraitImg; // La foto del client
    public TextMeshProUGUI orderInfoText;

    public void Setup(Order order, Sprite photo)
    {
        portraitImg.sprite = photo;
        
        // Text de la comanda (ex: Drac + Fred)
        string requirements = $"{order.monster.requirementName} + {order.condition.requirementName}";
        orderInfoText.text = $"#{order.orderID}\n{requirements}";
    }
}