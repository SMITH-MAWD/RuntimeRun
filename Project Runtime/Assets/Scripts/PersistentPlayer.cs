using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentPlayer : MonoBehaviour
{
    private static PersistentPlayer s_instance;

        [Header("Fallback spawn name inside destination scene")]
    [SerializeField] private string defaultSpawnName = "SpawnArea";

    [Header("Optional: explicitly assign your Cinemachine Virtual Camera component here")]
    [SerializeField] private Component virtualCameraComponent;

    void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Destroy(gameObject); // avoid duplicates if player prefab exists in multiple scenes
            return;
        }

        s_instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Priority: explicit position, then named spawn, then default spawn name.
        if (SpawnStore.NextPosition.HasValue)
        {
            transform.position = SpawnStore.NextPosition.Value;
        }
        else if (!string.IsNullOrEmpty(SpawnStore.NextSpawnName))
        {
            GameObject spawn = GameObject.Find(SpawnStore.NextSpawnName);
            if (spawn != null)
                transform.position = spawn.transform.position;
            else
            {
                GameObject fallback = GameObject.Find(defaultSpawnName);
                if (fallback != null)
                    transform.position = fallback.transform.position;
            }
        }
        else
        {
            GameObject fallback = GameObject.Find(defaultSpawnName);
            if (fallback != null)
                transform.position = fallback.transform.position;
        }

        // Clear store so it doesn't affect later loads
        SpawnStore.Clear();

        // Try to wire Cinemachine Virtual Camera Follow (reflection, no package compile dependency)
        Component vcam = virtualCameraComponent ?? FindVirtualCameraComponent();
        if (vcam != null)
        {
            var type = vcam.GetType();
            var followProp = type.GetProperty("Follow", BindingFlags.Public | BindingFlags.Instance);
            if (followProp != null && followProp.PropertyType == typeof(Transform))
            {
                followProp.SetValue(vcam, transform);
                Debug.Log($"PersistentPlayer: assigned player to VirtualCamera.Follow ({vcam.name}).");
                return;
            }
        }

        // Fallback: move Main Camera to player position (2D)
        Camera main = Camera.main;
        if (main != null)
        {
            var camPos = main.transform.position;
            main.transform.position = new Vector3(transform.position.x, transform.position.y, camPos.z);
            Debug.Log("PersistentPlayer: positioned Main Camera on player (no Cinemachine found).");
        }
        else
        {
            Debug.LogWarning("PersistentPlayer: no VirtualCamera found and no Main Camera exists in the scene.");
        }
    }

    // Best-effort search for a component that looks like a Cinemachine vcam (has Follow property)
    private Component FindVirtualCameraComponent()
    {
        // Some Unity versions require the includeInactive parameter; call the overload with it to be compatible.
        foreach (var go in Object.FindObjectsOfType<GameObject>(includeInactive: true))
        {
            var comps = go.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                var t = c.GetType();
                var prop = t.GetProperty("Follow", BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.PropertyType == typeof(Transform))
                    return c;
            }
        }
        return null;
    }
}