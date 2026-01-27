using UnityEngine;
using System.Collections;
using TMPro;
using NUnit.Framework;

public class TypeWritterEffect : MonoBehaviour
{
    private static bool skipText = false;
    public static bool isWriting = false;
    void Update()
    {
        if (InputManager.Instance != null && InputManager.Instance.JumpPressed && isWriting)
        {
            skipText = true;
        }
    }
    public static IEnumerator TypeText(TextMeshProUGUI targetText, string fullText, float delay)
    {
        //saltar escritura de texto si se presiona una tecla
        isWriting = true;
        targetText.text = "";
        
        foreach (char c in fullText)
        {
            if (skipText)
            {
                targetText.text = fullText;
                skipText = false;
                isWriting = false;
                yield break;
            }
            targetText.text += c;
            yield return new WaitForSeconds(delay);
        }
        skipText = false;
        isWriting = false;
    }
}