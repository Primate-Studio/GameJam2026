using UnityEngine;
using System.Collections;
using TMPro;

public class TypeWritterEffect : MonoBehaviour
{
    public static IEnumerator TypeText(TextMeshProUGUI targetText, string fullText, float delay)
    {
        //saltar escritura de texto si se presiona una tecla
        targetText.text = "";
        
        foreach (char c in fullText)
        {
            if (InputManager.Instance != null && InputManager.Instance.JumpPressed)
            {
                targetText.text = fullText;
                yield break;
            }
            targetText.text += c;
            yield return new WaitForSeconds(delay);
        }
    }
}