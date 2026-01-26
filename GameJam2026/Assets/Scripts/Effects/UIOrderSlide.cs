using UnityEngine;
using System.Collections;

public class UIOrderSlide : MonoBehaviour
{
    public float slideDuration = 0.5f;
    public Vector2 startOffset = new Vector2(5, 0);
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void StartSlide()
    {
        StopAllCoroutines();
        StartCoroutine(SlideInRoutine());
    }

    private IEnumerator SlideInRoutine()
    {
        Vector2 endPosition = Vector2.zero;

        if (canvas == null || rectTransform == null)
            yield break;

        Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
        Vector2 rectSize   = rectTransform.rect.size;

        // Usa startOffset como dirección (ej. (1,0) derecha, (-1,0) izquierda, etc.)
        // y multiplica por (canvas + tamaño propio) para quedar completamente fuera
        Vector2 dir = startOffset.normalized;
        Vector2 startPosition = endPosition + new Vector2(
            dir.x * (canvasSize.x * 0.5f + rectSize.x),
            dir.y * (canvasSize.y * 0.5f + rectSize.y)
        );

        rectTransform.anchoredPosition = startPosition;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            float curveValue = slideCurve.Evaluate(t);
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, curveValue);
            yield return null;
        }

        rectTransform.anchoredPosition = endPosition;
    }
}