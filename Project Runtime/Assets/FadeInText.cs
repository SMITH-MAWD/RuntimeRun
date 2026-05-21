using UnityEngine;
using TMPro;
using System.Collections;

public class FadeInText : MonoBehaviour
{
    [Header("Text Reference")]
    public TextMeshProUGUI textComponent;

    [Header("Fade Durations")]
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 1.0f;

    private Coroutine currentFade;

    void Start()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();

        // Start invisible and fade in
        SetAlpha(0f);
        FadeIn();
    }

    /// <summary>Plays a fade-in from transparent to opaque.</summary>
    public void FadeIn()
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(0f, 1f, fadeInDuration));
    }

    /// <summary>Plays a fade-out from opaque to transparent. Called by TriggeroutPressq.</summary>
    public void FadeOut()
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(1f, 0f, fadeOutDuration));
    }

    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(targetAlpha);
        currentFade = null;
    }

    private void SetAlpha(float alpha)
    {
        if (textComponent != null)
        {
            Color c = textComponent.color;
            c.a = alpha;
            textComponent.color = c;
        }
    }
}