using UnityEngine;
using System;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public float fadeInDuration;

    public float fadeOutDuration;


    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeIn(Action onFadeInComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(0, 1, fadeInDuration, onFadeInComplete));
    }

    public void FadeOut(Action onFadeOutComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(1, 0, fadeOutDuration, onFadeOutComplete));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration, Action onComplete)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        onComplete?.Invoke();
    }
}
