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

    [Header("Optional keybind")]
    [Tooltip("If enabled, the player must press the key while inside the trigger to activate the transition.")]
    [SerializeField] private bool requireKeyPress = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool _triggered;
    private bool _playerInRange;

    private void Update()
    {
        if (!_triggered && requireKeyPress && _playerInRange && Input.GetKeyDown(interactKey))
        {
            TriggerLoad();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (requireKeyPress)
        {
            _playerInRange = true;
            // Optional: provide feedback to the player (e.g. UI) that they can press the key.
            Debug.Log($"Transition: Player in range. Press '{interactKey}' to enter '{sceneName}'.");
            return;
        }

        // Immediate transition when key press is not required
        TriggerLoad();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
    }

    private void TriggerLoad()
    {
        if (_triggered) return;
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