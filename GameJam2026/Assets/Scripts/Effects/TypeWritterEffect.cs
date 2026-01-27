using UnityEngine;
using System.Collections;
using TMPro;

public class TypeWritterEffect : MonoBehaviour
{
    private static bool skipText = false;
    void Update()
    {
        if (InputManager.Instance != null && InputManager.Instance.JumpPressed)
        {
            skipText = true;
        }
    }
    public static IEnumerator TypeText(TextMeshProUGUI targetText, string fullText, float delay)
    {
        //saltar escritura de texto si se presiona una tecla
        targetText.text = "";
        
        foreach (char c in fullText)
        {
            if (skipText)
            {
                targetText.text = fullText;
                skipText = false;
                yield break;
            }
            targetText.text += c;
            yield return new WaitForSeconds(delay);
        }
    }
}