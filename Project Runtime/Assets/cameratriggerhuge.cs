using UnityEngine;
// Cinemachine is optional. We avoid a compile-time dependency so the script works
// whether or not the Cinemachine package is installed.

public class cameratriggerhuge : MonoBehaviour
{
    [Tooltip("Name of the Camera GameObject to switch to (exact name in hierarchy)")]
    public string cameraName = "CameraSet";

    [Header("Cinemachine support")]
    [Tooltip("If true, the trigger will control a Cinemachine Virtual Camera instead of a Unity Camera component")]
    public bool useCinemachine = false;

    [Tooltip("Reference to the Cinemachine Virtual Camera (or any Component) to activate. If null, cameraName fallback will be used.")]
    public Component targetVirtualCamera = null;

    [Tooltip("Priority value to assign to the target virtual camera while active (higher wins)")]
    public int vcamPriority = 20;

    [Tooltip("If true, restore the virtual camera's previous priority when exiting the trigger")]
    public bool revertVcamOnExit = true;

    // internal storage for restoring state
    private int prevVcamPriority = 0;
    private bool vcamPriorityChanged = false;
    // reference to the component we modified (so we can restore it later)
    private Component modifiedVcam = null;

    [Tooltip("If true, player must press the Interact Key while inside the trigger to change camera. If false, camera switches on trigger enter.")]
    public bool requireKey = false;

    [Header("Debug")]
    [Tooltip("Enable debug logs for this trigger (OnTrigger events and camera selection)")]
    public bool enableDebugLogs = true;

    [Header("Interaction")]
    [Tooltip("If true, once the player interacts and the camera is switched, keep that camera state even if the player immediately leaves the trigger (useful when teleporting).")]
    public bool keepCameraAfterInteract = true;

    // set to true when SwitchToNamedCamera succeeds via interaction
    private bool interactedThisTrigger = false;
    [Tooltip("Grace period (seconds) after interaction during which OnTriggerExit will not revert the camera. Helps when a teleporter moves the player immediately.")]
    public float keepCameraGrace = 0.5f;
    private float lastInteractTime = -10f;

    [Tooltip("Key used for interaction when Require Key is enabled")]
    public KeyCode interactKey = KeyCode.E;

    [Tooltip("Tag the player GameObject uses (defaults to 'Player')")]
    public string playerTag = "Player";

    // internal state
    private bool playerInRange = false;

    void Start()
    {
        Collider c = GetComponent<Collider>();
        if (c == null)
        {
            Debug.LogWarning(name + ": cameratriggerhuge requires a Collider. Attach one and set 'Is Trigger' to true.");
        }
        else if (!c.isTrigger)
        {
            Debug.LogWarning(name + ": Collider should be set as a Trigger for cameratriggerhuge to work correctly.");
        }
    }

    void Update()
    {
        if (requireKey && playerInRange && Input.GetKeyDown(interactKey))
        {
            SwitchToNamedCamera();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (enableDebugLogs) Debug.Log("cameratriggerhuge: OnTriggerEnter -> " + other.name, this);

        if (requireKey)
        {
            playerInRange = true;
        }
        else
        {
            if (enableDebugLogs) Debug.Log("cameratriggerhuge: auto-switching camera on enter", this);
            SwitchToNamedCamera();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        
        // If the player interacted and the option to keep the camera is enabled, don't revert when exiting (useful for teleporters)
        if (useCinemachine && revertVcamOnExit && vcamPriorityChanged && modifiedVcam != null)
        {
            if (keepCameraAfterInteract && interactedThisTrigger)
            {
                // if we're within the grace period after interaction, keep the camera (useful when teleported immediately)
                if (Time.time - lastInteractTime <= keepCameraGrace)
                {
                    if (enableDebugLogs) Debug.Log("cameratriggerhuge: keeping camera after interact on exit (within grace)", this);
                    interactedThisTrigger = false; // clear interaction marker for next time
                    return;
                }
                // otherwise fall through and restore as usual
            }
            var type = modifiedVcam.GetType();
            var prop = type.GetProperty("Priority");
            if (prop != null && prop.PropertyType == typeof(int) && prop.CanWrite)
            {
                prop.SetValue(modifiedVcam, prevVcamPriority);
                Debug.Log("cameratriggerhuge: restored priority for virtual camera '" + modifiedVcam.name + "'.");
            }
            else
            {
                var field = type.GetField("Priority");
                if (field != null && field.FieldType == typeof(int))
                {
                    field.SetValue(modifiedVcam, prevVcamPriority);
                    Debug.Log("cameratriggerhuge: restored priority for virtual camera '" + modifiedVcam.name + "'.");
                }
            }

            vcamPriorityChanged = false;
            modifiedVcam = null;
        }
        if (enableDebugLogs) Debug.Log("cameratriggerhuge: OnTriggerExit -> " + other.name, this);
    }

    private void SwitchToNamedCamera()
    {
        // reset interaction marker when explicitly switching
        interactedThisTrigger = false;
        // Find the target GameObject once (used for both Cinemachine and non-Cinemachine paths)
        GameObject camObj = GameObject.Find(cameraName);

    if (useCinemachine)
        {
            // try the explicit reference first
            if (targetVirtualCamera != null)
            {
                if (ActivateVirtualCamera(targetVirtualCamera)) return;
            }

            // fall back to finding by name
            if (camObj != null)
            {
                var vcam = FindVirtualCameraComponent(camObj);
                if (vcam != null)
                {
                    if (ActivateVirtualCamera(vcam))
                    {
                        interactedThisTrigger = true;
                        lastInteractTime = Time.time;
                        return;
                    }
                }
            }

            Debug.LogWarning("cameratriggerhuge: Cinemachine requested but no VirtualCamera found. Falling back to regular Camera handling.");
            // fall through to non-cinemachine handling
        }
        if (camObj == null)
        {
            Debug.LogWarning("cameratriggerhuge: No GameObject named '" + cameraName + "' was found in the scene.");
            return;
        }

            Camera targetCam = camObj.GetComponent<Camera>();
        if (targetCam == null)
        {
            Debug.LogWarning("cameratriggerhuge: GameObject '" + cameraName + "' does not have a Camera component.");
            return;
        }

        // Disable all other cameras and enable the target camera.
        Camera[] all = Camera.allCameras;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == targetCam) continue;
            all[i].enabled = false;
        }

        targetCam.enabled = true;
        Debug.Log("cameratriggerhuge: switched to camera '" + cameraName + "'.");
        interactedThisTrigger = true;
        lastInteractTime = Time.time;
    }

    // Activates a virtual camera-like Component by setting its 'Priority' integer field/property via reflection.
    // Returns true if activation succeeded.
    private bool ActivateVirtualCamera(Component vcamComp)
    {
        if (vcamComp == null) return false;

        var type = vcamComp.GetType();

        // Try to find an int property or field named Priority
        var prop = type.GetProperty("Priority");
        if (prop != null && prop.PropertyType == typeof(int) && prop.CanWrite)
        {
            if (revertVcamOnExit)
            {
                prevVcamPriority = (int)prop.GetValue(vcamComp);
                vcamPriorityChanged = true;
                modifiedVcam = vcamComp;
            }

            prop.SetValue(vcamComp, vcamPriority);
            Debug.Log("cameratriggerhuge: activated virtual camera '" + vcamComp.name + "' with priority " + vcamPriority + ".");
            return true;
        }

        var field = type.GetField("Priority");
        if (field != null && field.FieldType == typeof(int))
        {
            if (revertVcamOnExit)
            {
                prevVcamPriority = (int)field.GetValue(vcamComp);
                vcamPriorityChanged = true;
                modifiedVcam = vcamComp;
            }

            field.SetValue(vcamComp, vcamPriority);
            Debug.Log("cameratriggerhuge: activated virtual camera '" + vcamComp.name + "' with priority " + vcamPriority + ".");
            return true;
        }

        // No Priority member found
        return false;
    }

    // Try to find a component that looks like a Cinemachine virtual camera (has int Priority)
    private Component FindVirtualCameraComponent(GameObject go)
    {
        var comps = go.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            var t = c.GetType();
            var prop = t.GetProperty("Priority");
            if (prop != null && prop.PropertyType == typeof(int)) return c;
            var field = t.GetField("Priority");
            if (field != null && field.FieldType == typeof(int)) return c;
        }
        return null;
    }
}
