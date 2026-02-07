using System.Threading.Tasks;
using UnityEngine;

public class Screenfade : MonoBehaviour
{
    public static Screenfade instance; // Singleton instance
    [SerializeField] CanvasGroup canvasGroup; // Reference to the CanvasGroup component
    [SerializeField] float fadeDuration = 0.5f; // Duration of the fade effect

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(0f));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(1f));
    }

    public void FadeTo(float targetAlpha)
    {
        StartCoroutine(Fade(targetAlpha));
    }

    private System.Collections.IEnumerator Fade(float targetAlpha)
    {
        if (canvasGroup == null) yield break;
        float startAlpha = canvasGroup.alpha;
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }
}