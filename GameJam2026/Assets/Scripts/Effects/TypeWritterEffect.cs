using UnityEngine;
using System.Collections;
using TMPro;

public class TypeWritterEffect : MonoBehaviour
{
    public static IEnumerator TypeText(TextMeshProUGUI targetText, string fullText, float delay)
    {
        targetText.text = "";
        
        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSeconds(delay);
        }
    }
}