using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [SerializeField] private Image fadeImage;   // Optional – if not set, one is created automatically
    [SerializeField] private Canvas fadeCanvas; // Optional – if not set, a Canvas is created automatically

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 1. Create a persistent canvas if none was assigned or if the assigned one isn't marked DontDestroyOnLoad
        if (fadeCanvas == null)
        {
            CreatePersistentCanvasAndImage();
        }
        else
        {
            // If user assigned a canvas, make it permanent so it doesn't get destroyed on scene load
            if (fadeCanvas.gameObject.scene.IsValid())  // It's part of a scene, not a prefab
            {
                DontDestroyOnLoad(fadeCanvas.gameObject);
            }

            // If no image was assigned, find one on the canvas or create one
            if (fadeImage == null)
            {
                fadeImage = fadeCanvas.GetComponentInChildren<Image>();
                if (fadeImage == null)
                {
                    GameObject imgObj = new GameObject("FadeImage", typeof(Image));
                    imgObj.transform.SetParent(fadeCanvas.transform, false);
                    fadeImage = imgObj.GetComponent<Image>();
                }
            }

            // Make sure the image is opaque black and covers the whole screen
            SetupImage();
        }
    }

    private void CreatePersistentCanvasAndImage()
    {
        // Canvas
        GameObject canvasObj = new GameObject("TransitionCanvas");
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);

        // Image
        GameObject imgObj = new GameObject("FadeImage", typeof(Image));
        imgObj.transform.SetParent(fadeCanvas.transform, false);
        fadeImage = imgObj.GetComponent<Image>();
        SetupImage();
    }

    private void SetupImage()
    {
        if (fadeImage == null) return;

        fadeImage.color = new Color(0, 0, 0, 0);  // Start transparent
        fadeImage.raycastTarget = false;

        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    public void FadeAndLoad(string sceneName, float duration, bool async)
    {
        StopAllCoroutines();
        StartCoroutine(FadeAndLoadRoutine(sceneName, duration, async));
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName, float duration, bool async)
    {
        // Fade out (to black)
        yield return Fade(0, 1, duration);

        // Load the new scene
        if (async)
        {
            AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
            if (ao != null)
            {
                while (!ao.isDone)
                    yield return null;
            }
            else
            {
                Debug.LogError($"Scene '{sceneName}' not found in Build Settings.");
                // Fade back in on error
                yield return Fade(1, 0, duration);
                yield break;
            }
        }
        else
        {
            SceneManager.LoadScene(sceneName);
            // When loading synchronously, the next line only runs after the new scene is fully loaded,
            // but the fadeImage may have been destroyed. The coroutine continues, and the Fade(1,0,duration)
            // will safely check for null.
        }

        // Fade in (back to transparent)
        yield return Fade(1, 0, duration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        // Safety check – if the image was destroyed, stop immediately
        if (fadeImage == null) yield break;

        float time = 0;
        while (time < duration)
        {
            // Re-check inside the loop because a scene load could destroy the image mid‑fade
            if (fadeImage == null) yield break;

            time += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, time / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, to);
    }
}