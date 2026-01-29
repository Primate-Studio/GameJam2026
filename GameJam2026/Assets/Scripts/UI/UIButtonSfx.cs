using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Button button;
    private Vector3 originalScale;

    void Start()
    {
        button = GetComponent<Button>();
        originalScale = button.transform.localScale;

        if (button != null)
        {
            button.onClick.AddListener(() => {
                AudioManager.Instance.PlayUI(UIType.Click);
            });
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            AudioManager.Instance.PlayUI(UIType.Hover);
            button.transform.localScale = originalScale * 1.1f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null)
            button.transform.localScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null)
            button.transform.localScale = originalScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null)
            button.transform.localScale = originalScale;
    }

    void OnDisable()
    {
        if (button != null)
            button.transform.localScale = originalScale;
    }
}