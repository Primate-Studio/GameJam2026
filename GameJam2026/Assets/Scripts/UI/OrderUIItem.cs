using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderUIItem : MonoBehaviour
{
    [Header("Elements UI")]
    public Image portraitImg; // La foto del client
    public Image monsterIconImg;
    public Image conditionIconImg;
    public Image environmentIconImg;

    public void Setup(Order order, Sprite photo)
    {
        portraitImg.sprite = photo;
        
        // Iconos de requisitos - usar los sprites directamente del RequirementData
        monsterIconImg.sprite = order.monster.icon;
        conditionIconImg.sprite = order.condition.icon;
        
        // Solo mostrar environment si el pedido lo tiene
        if (order.environment != null)
        {
            environmentIconImg.gameObject.SetActive(true);
            environmentIconImg.sprite = order.environment.icon;
        }
        else
        {
            environmentIconImg.gameObject.SetActive(false);
        }
    }
}