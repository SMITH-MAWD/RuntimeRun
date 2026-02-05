using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class dropscript : MonoBehaviour
{
    [Tooltip("Drag the player GameObject here (the object that will trigger the platform)")]
    public GameObject player;
    [Tooltip("Drag the ground GameObject here (object that will reset the platform on collision)")]
    public GameObject ground;

    private Rigidbody rb;
    private bool hasDropped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("dropscript requires a Rigidbody component.");
            enabled = false;
            return;
        }

        if (player == null)
            Debug.LogWarning("dropscript: 'player' not assigned in Inspector. Trigger checks will not detect a player until assigned.");
        if (ground == null)
            Debug.LogWarning("dropscript: 'ground' not assigned in Inspector. Ground collision checks will not work until assigned.");

        // Ensure the platform starts kinematic so it doesn't fall immediately
        rb.isKinematic = true;
    }

    bool IsPlayerCollider(Collider other)
    {
        if (player == null) return false;
        if (other.gameObject == player) return true;
        if (other.transform.IsChildOf(player.transform)) return true;
        var attached = other.attachedRigidbody;
        if (attached != null && attached.gameObject == player) return true;
        return false;
    }

    bool IsGroundCollision(Collision collision)
    {
        if (ground == null) return false;
        if (collision.gameObject == ground) return true;
        if (collision.transform.IsChildOf(ground.transform)) return true;
        return false;
    }

    // Called when something enters this object's trigger collider (e.g., capsule trigger)
    void OnTriggerEnter(Collider other)
    {
        if (!hasDropped && IsPlayerCollider(other))
        {
            hasDropped = true;

            // Make the rigidbody dynamic so gravity affects it
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp();
        }
    }

    // Called when the platform collides with something (use to detect ground)
    void OnCollisionEnter(Collision collision)
    {
        if (hasDropped && IsGroundCollision(collision))
        {
            // Stop motion and make kinematic again to 'stick' the platform to the ground
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    // Optional helper to reset platform to starting state from other scripts or events
    public void ResetToKinematic()
    {
        hasDropped = false;
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }
}
