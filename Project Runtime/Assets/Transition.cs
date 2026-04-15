using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    [SerializeField] private string sceneName = "Path to Finale";
    [Tooltip("Load async instead of immediately (useful for adding a fade later).")]
    [SerializeField] private bool useAsyncLoad = false;

    [Tooltip("If true, disable this trigger after it fires to prevent double-triggering.")]
    [SerializeField] private bool disableTriggerAfterUse = true;

    private bool _triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        if (disableTriggerAfterUse)
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        if (useAsyncLoad)
            StartCoroutine(LoadSceneAsyncRoutine());
        else
            SceneManager.LoadScene(sceneName);
    }

    private IEnumerator LoadSceneAsyncRoutine()
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
        if (ao == null)
        {
            Debug.LogError($"Transition: failed to load scene '{sceneName}'");
            yield break;
        }

        // Optionally you could wait for ao.progress >= 0.9f then show a fade before allowSceneActivation = true.
        while (!ao.isDone)
            yield return null;
    }
}