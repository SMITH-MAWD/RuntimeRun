using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Transition : MonoBehaviour
{
    [Header("Scene Selection")]
    [Tooltip("Drag your target scene here from the Project window.")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset targetScene;
#endif
    [SerializeField] private string sceneName;   // Auto‑filled from targetScene in OnValidate()

    [Header("Load Settings")]
    [SerializeField] private bool useAsyncLoad = false;
    [SerializeField] private bool disableTriggerAfterUse = true;

    [Header("Fade Transition")]
    [SerializeField] private bool useFadeTransition = true;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Optional Keybind")]
    [SerializeField] private bool requireKeyPress = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool _triggered;
    private bool _playerInRange;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (targetScene != null)
        {
            string path = AssetDatabase.GetAssetPath(targetScene);
            sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }
#endif

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
            Debug.Log($"Press '{interactKey}' to go to '{sceneName}'");
            return;
        }
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
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("No scene selected. Please assign a Scene in the Inspector.");
            return;
        }

        if (useFadeTransition)
        {
            // Let the manager handle the fade and load
            if (SceneTransitionManager.Instance == null)
            {
                Debug.LogError("No SceneTransitionManager found. Please add a GameObject with the SceneTransitionManager script to your initial scene.");
                // Fallback to direct load
                StartCoroutine(LoadDirectly());
                return;
            }
            SceneTransitionManager.Instance.FadeAndLoad(sceneName, fadeDuration, useAsyncLoad);
        }
        else
        {
            StartCoroutine(LoadDirectly());
        }
    }

    private System.Collections.IEnumerator LoadDirectly()
    {
        if (useAsyncLoad)
        {
            AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
            if (ao != null)
                while (!ao.isDone)
                    yield return null;
            else
                Debug.LogError($"Failed to load scene '{sceneName}'. Make sure it's added to Build Settings.");
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
        yield break;
    }
}