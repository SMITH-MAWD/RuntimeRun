using UnityEngine;

// Ladder trigger that lets a player attach 
[RequireComponent(typeof(Collider2D))]
public class ladder : MonoBehaviour
{
    [SerializeField] private float climbSpeed = 3f;

    private Collider2D col;

    // Player state while in/around ladder
    private Rigidbody2D attachedRb;
    private PlayerMovement attachedMovement;
    private bool attachedMovementOriginalEnabled = true;
    private bool attachedMovementOriginalInputEnabled = true;
    private float attachedOriginalGravity = 1f;
    private bool isAttached = false;

    // Potential player in trigger zone (can attach/detach with Q)
    private Rigidbody2D potentialRb;
    private PlayerMovement potentialMovement;
    private bool playerInZone = false;

    private float topY;
    private float bottomY;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            Debug.LogWarning("ladder: Collider2D should be set to 'Is Trigger' for climbable behaviour.", this);

        if (col != null)
        {
            topY = col.bounds.max.y;
            bottomY = col.bounds.min.y;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        if (!other.CompareTag("Player")) return;

        potentialRb = other.GetComponentInParent<Rigidbody2D>();
        potentialMovement = other.GetComponentInParent<PlayerMovement>();

        if (potentialRb == null)
        {
            Debug.LogWarning("ladder: Player entering ladder trigger has no Rigidbody2D.", other.gameObject);
            return;
        }

        playerInZone = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null) return;
        if (!other.CompareTag("Player")) return;

        // If the leaving collider is the potential player, clear potential refs
        Rigidbody2D rb = other.GetComponentInParent<Rigidbody2D>();
        if (rb == null) return;

        playerInZone = false;
        potentialRb = null;
        potentialMovement = null;

        // If attached player left the collider (edge cases), detach
        if (isAttached && attachedRb == rb)
            Detach();
    }

    void Update()
    {
        if (isAttached)
        {
            if (Input.GetKeyDown(KeyCode.Q))
                Detach();

            return;
        }

        if (playerInZone && potentialRb != null && Input.GetKeyDown(KeyCode.W))
        {
            attachedRb = potentialRb;
            attachedMovement = potentialMovement;
            attachedOriginalGravity = attachedRb.gravityScale;
            Attach();
        }
    }

    void FixedUpdate()
    {
        if (!isAttached || attachedRb == null) return;

        float v = Input.GetAxis("Vertical");

        // Remove downward momentum unless the player is actively pressing down (S).
        if (v >= -0.01f && attachedRb.linearVelocity.y < 0f)
            attachedRb.linearVelocity = new Vector2(attachedRb.linearVelocity.x, 0f);

        float nextY = attachedRb.position.y + v * climbSpeed * Time.fixedDeltaTime;

        const float eps = 0.001f;

        if (nextY >= topY - eps)
        {
            PlayerMovement pm = attachedMovement;
            Vector2 exitPos = new Vector2(transform.position.x, topY);
            attachedRb.MovePosition(exitPos);

            // Preserve a bit of upward momentum when stepping off the ladder.
            if (v > 0.01f)
                attachedRb.linearVelocity = new Vector2(attachedRb.linearVelocity.x, v * climbSpeed);

            Detach();

            // Manual top "free jump"
            if (pm != null && v > 0.01f && Input.GetKey(KeyCode.W))
                pm.ForceJump();
            return;
        }

        // If nextY would reach or pass bottom, snap to bottom and detach
        if (nextY <= bottomY + eps)
        {
            Vector2 bottomPos = new Vector2(transform.position.x, bottomY);
            attachedRb.MovePosition(bottomPos);
            Detach();
            return;
        }

        // Normal climb movement; lock X to ladder
        Vector2 newPos = new Vector2(transform.position.x, nextY);
        attachedRb.MovePosition(newPos);
    }

    private void Attach()
    {
        if (attachedRb == null) return;
        isAttached = true;

        if (attachedMovement != null)
        {
            attachedMovementOriginalEnabled = attachedMovement.enabled;
            attachedMovementOriginalInputEnabled = attachedMovement.inputEnabled;
            attachedMovement.inputEnabled = false;
            
            // Disable PlayerMovement while on ladder so its gravity/movement logic can't pull the player off.
            attachedMovement.enabled = false;
        }

        attachedRb.gravityScale = 0f;
        attachedRb.linearVelocity = Vector2.zero;

        // Snap X to ladder
        Vector3 p = attachedRb.transform.position;
        p.x = transform.position.x;
        attachedRb.transform.position = p;
    }

    private void Detach()
    {
        if (attachedRb != null)
            attachedRb.gravityScale = attachedOriginalGravity;

        if (attachedMovement != null)
        {
            attachedMovement.enabled = attachedMovementOriginalEnabled;
            attachedMovement.inputEnabled = attachedMovementOriginalInputEnabled;
        }

        attachedRb = null;
        attachedMovement = null;
        isAttached = false;
    }
}
