using UnityEngine;

public class ManualUI : MonoBehaviour
{
    public GameObject manualPanel;
    [SerializeField] private GameObject[] pages;

    public void Start()
    {
        manualPanel.SetActive(false);
    }
    public void OpenManual()
    {
        manualPanel.SetActive(true);
    }
    public void CloseManual()
    {
        manualPanel.SetActive(false);
    }
    public void TurnPage(int pageIndex)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == pageIndex);
        }
    }
}   