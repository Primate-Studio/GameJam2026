using UnityEngine;
using System.Collections;

public class UIPopEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private Vector3 targetScale = Vector3.one;

    private void OnEnable()
    {
        // Cada cop que la bafarada aparegui o s'instancii, farà el pop
        StopAllCoroutines();
        StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        // 1. Comencem des de zero
        transform.localScale = Vector3.zero;
        float elapsed = 0;

        // 2. Creixem fins a 1.2 (l'overshoot)
        while (elapsed < duration * 0.7f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.7f);
            // Fem servir una corba suau
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale * 1.2f, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return null;
        }

        // 3. Tornem al tamany normal (1.0)
        elapsed = 0;
        Vector3 currentScale = transform.localScale;
        while (elapsed < duration * 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.3f);
            transform.localScale = Vector3.Lerp(currentScale, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
}