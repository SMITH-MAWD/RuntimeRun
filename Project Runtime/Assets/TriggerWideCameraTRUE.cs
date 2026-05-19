using Unity.Cinemachine;
using UnityEngine;

public class TriggerWideCamTRUE : MonoBehaviour
{
    [Header("Drag the camera that will take over permanently")]
    [SerializeField] private CinemachineCamera targetCamera;  // the wide camera

    private bool hasSwitched = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasSwitched) return;

        // Find the currently active camera
        CinemachineBrain brain = Camera.main?.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            Debug.LogError("CinemachineBrain not found!");
            return;
        }

        CinemachineCamera currentCamera = brain.ActiveVirtualCamera as CinemachineCamera;
        if (currentCamera == null)
        {
            Debug.LogError("No active CinemachineCamera found!");
            return;
        }

        // Disable current camera (set priority 0)
        currentCamera.Priority = 0;

        // Activate target camera (use its existing priority, or set a specific one)
        // Since normal was priority 2 and wide is 1, we need to make wide win.
        // Either set wide priority to something > 0, e.g., keep 1 (since normal now 0, wide 1 wins)
        // Or force it to 2. Let's keep its original priority.
        // But to ensure it takes over immediately, we can set it to at least 1.
        if (targetCamera.Priority <= 0)
            targetCamera.Priority = 1;

        // Optionally, you could set targetCamera.Priority = 2; // uncomment to force

        hasSwitched = true;

        Debug.Log($"Switched permanently to {targetCamera.name}. Current camera priority: {targetCamera.Priority}");
    }
}