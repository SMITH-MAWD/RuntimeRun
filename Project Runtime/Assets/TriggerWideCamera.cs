using Unity.Cinemachine;
using UnityEngine;

public class TriggerCameraSwitch : MonoBehaviour
{
    [Header("Drag the camera you want to switch to")]
    [SerializeField] private CinemachineCamera targetCamera;  // the wide/alternative camera

    private CinemachineCamera originalCamera;
    private int originalPriority;
    private int targetPriority;

    void Start()
    {
        if (targetCamera == null)
        {
            Debug.LogError("No target camera assigned to TriggerCameraSwitch!");
            return;
        }

        // Store target's original priority
        targetPriority = targetCamera.Priority;
        // Ensure target starts disabled
        targetCamera.Priority = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Find the currently active Cinemachine camera
        CinemachineBrain brain = Camera.main?.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            Debug.LogError("CinemachineBrain not found on main camera!");
            return;
        }

        originalCamera = brain.ActiveVirtualCamera as CinemachineCamera;
        if (originalCamera == null)
        {
            Debug.LogError("Could not get active CinemachineCamera!");
            return;
        }

        // Store its priority and disable it
        originalPriority = originalCamera.Priority;
        originalCamera.Priority = 0;

        // Enable target camera
        targetCamera.Priority = targetPriority;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (originalCamera == null) return;

        // Restore original camera
        originalCamera.Priority = originalPriority;
        // Disable target camera
        targetCamera.Priority = 0;
    }
}