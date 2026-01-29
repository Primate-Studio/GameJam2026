using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonSfx : MonoBehaviour, IPointerEnterHandler
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        // Afegim el so del clic automàticament
        if (button != null)
        {
            button.onClick.AddListener(() => {
                AudioManager.Instance.PlayUI(UIType.Click);
            });
        }
    }

    // Opcional: So quan el ratolí passa per sobre (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            AudioManager.Instance.PlayUI(UIType.Hover);
        }
    }
}