Assets/SceneInitializer.cs
using System.Reflection;
using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    [Header("Spawn")]
    [Tooltip("Optional spawn Transform. If null the player's current position is used.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Camera (optional)")]
    [Tooltip("Assign your Cinemachine Virtual Camera here (drag the Virtual Camera component). If left empty the script will try to find one automatically.")]
    [SerializeField] private Component virtualCameraComponent;
    [Tooltip("Fallback: assign a regular Camera (Main Camera will be used if left empty).")]
    [SerializeField] private Camera mainCamera;

    void Start()
    {
        // find player by tag
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogWarning($"SceneInitializer: No GameObject tagged '{playerTag}' found.");
            return;
        }

        // place player at spawn
        if (spawnPoint != null)
        {
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.rotation;
        }

        // Ensure mainCamera reference
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Try to wire up Cinemachine virtual camera (no compile-time dependency)
        Component vcam = virtualCameraComponent ?? FindVirtualCameraComponent();
        if (vcam != null)
        {
            // Set the Follow property if present
            var type = vcam.GetType();
            var followProp = type.GetProperty("Follow", BindingFlags.Public | BindingFlags.Instance);
            if (followProp != null && followProp.PropertyType == typeof(Transform))
            {
                followProp.SetValue(vcam, player.transform);
                Debug.Log($"SceneInitializer: Assigned player to VirtualCamera.Follow ({vcam.name}).");
            }

            // Optionally raise priority if property exists
            var priorityProp = type.GetProperty("Priority", BindingFlags.Public | BindingFlags.Instance);
            if (priorityProp != null && priorityProp.PropertyType == typeof(int))
            {
                try
                {
                    int prev = (int)priorityProp.GetValue(vcam);
                    priorityProp.SetValue(vcam, Mathf.Max(prev, 10));
                }
                catch { /* ignore reflection exceptions */ }
            }

            return; // Cinemachine handled
        }

        // No virtual camera found: position main camera over player (2D)
        if (mainCamera != null)
        {
            var camPos = mainCamera.transform.position;
            mainCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, camPos.z);
            Debug.Log("SceneInitializer: Moved main camera to player position (no Cinemachine found).");
        }
    }

    // Attempts to find a component instance that looks like a Cinemachine Virtual Camera (has Transform Follow property)
    private Component FindVirtualCameraComponent()
    {
        foreach (var go in Object.FindObjectsOfType<GameObject>())
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