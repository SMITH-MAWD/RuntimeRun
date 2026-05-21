using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Transition : MonoBehaviour
{
    [Header("Scene Selection")]
    [Tooltip("Drag your target scene here from the Project window.")]
    [SerializeField] private SceneAsset targetScene; // Editor-only
    [SerializeField] private string sceneName; // Auto-filled, don't edit manually

    [Header("Load Settings")]
    [SerializeField] private bool useAsyncLoad = false;
    [SerializeField] private bool disableTriggerAfterUse = true;

    [Header("Optional keybind")]
    [SerializeField] private bool requireKeyPress = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool _triggered;
    private bool _playerInRange;

#if UNITY_EDITOR
    // When a scene is assigned in the editor, extract its name
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
            Debug.LogError($"Failed to load scene '{sceneName}'. Make sure it's added to Build Settings.");
            yield break;
        }

        while (!ao.isDone)
            yield return null;
    }
}