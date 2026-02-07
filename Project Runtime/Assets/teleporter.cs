using UnityEngine;

public class teleporter : MonoBehaviour
{
    [Header("Teleport Destination")]
    [Tooltip("Assign a GameObject to use its position as the teleport destination.")]
    [SerializeField] private GameObject destination;

    [Header("Player Settings")]
    [Tooltip("Optional: assign the player root GameObject. If left empty, the script will find a GameObject with the Player tag.")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private string playerTag = "Player";

    [Header("Input")]
    [SerializeField] private KeyCode teleportKey = KeyCode.E;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private bool playerInTrigger = false;
    // store last player GameObject that entered (works for 2D/3D)
    private GameObject playerInTriggerObj = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        bool isPlayer = false;
        if (playerObject != null)
        {
            if (other.gameObject == playerObject) isPlayer = true;
            else if (other.transform.root != null && other.transform.root.gameObject == playerObject) isPlayer = true;
        }
        if (!isPlayer && other.CompareTag(playerTag)) isPlayer = true;

        if (!isPlayer) return;
        playerInTrigger = true;
        playerInTriggerObj = other.transform.root != null ? other.transform.root.gameObject : other.gameObject;
        if (enableDebugLogs) Debug.Log("teleporter: player entered 3D trigger -> " + playerInTriggerObj.name, this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        bool isPlayer = false;
        if (playerObject != null)
        {
            if (other.gameObject == playerObject) isPlayer = true;
            else if (other.transform.root != null && other.transform.root.gameObject == playerObject) isPlayer = true;
        }
        if (!isPlayer && other.CompareTag(playerTag)) isPlayer = true;

        if (!isPlayer) return;
        playerInTrigger = false;
        playerInTriggerObj = null;
        if (enableDebugLogs) Debug.Log("teleporter: player exited 3D trigger", this);
    }

    // 2D trigger handlers
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        bool isPlayer = false;
        if (playerObject != null)
        {
            if (other.gameObject == playerObject) isPlayer = true;
            else if (other.transform.root != null && other.transform.root.gameObject == playerObject) isPlayer = true;
        }
        if (!isPlayer && other.CompareTag(playerTag)) isPlayer = true;

        if (!isPlayer) return;
        playerInTrigger = true;
        playerInTriggerObj = other.transform.root != null ? other.transform.root.gameObject : other.gameObject;
        if (enableDebugLogs) Debug.Log("teleporter: player entered 2D trigger -> " + playerInTriggerObj.name, this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null) return;

        bool isPlayer = false;
        if (playerObject != null)
        {
            if (other.gameObject == playerObject) isPlayer = true;
            else if (other.transform.root != null && other.transform.root.gameObject == playerObject) isPlayer = true;
        }
        if (!isPlayer && other.CompareTag(playerTag)) isPlayer = true;

        if (!isPlayer) return;
        playerInTrigger = false;
        playerInTriggerObj = null;
        if (enableDebugLogs) Debug.Log("teleporter: player exited 2D trigger", this);
    }

    private void Update()
    {
        if (!playerInTrigger) return;

        if (Input.GetKeyDown(teleportKey))
        {
            GameObject p = playerObject;
            if (p == null)
            {
                p = GameObject.FindWithTag(playerTag);
            }

            if (p == null)
            {
                if (enableDebugLogs) Debug.LogWarning("teleporter: No player found to teleport (assign playerObject or set correct tag).", this);
                return;
            }

            if (destination == null)
            {
                if (enableDebugLogs) Debug.LogWarning("teleporter: No destination assigned. Set a destination GameObject in the Inspector.", this);
                return;
            }

            p.transform.position = destination.transform.position;
            if (enableDebugLogs) Debug.Log($"teleporter: Teleported {p.name} to {destination.name}", this);
        }
    }
}
