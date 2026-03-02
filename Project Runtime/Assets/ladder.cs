using UnityEngine;

/// <summary>
/// Ladder trigger that lets a player attach by pressing W while inside the ladder area.
/// While attached the player is locked to the ladder X and can move up/down with Vertical axis (W/S).
/// Reaching the top or bottom will place the player slightly off the ladder and detach them.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ladder : MonoBehaviour
{
    [SerializeField] private float climbSpeed = 3f;

    private Collider2D col;

    // Player state while in/around ladder
    private Rigidbody2D attachedRb;
    private PlayerMovement attachedMovement;
    private float attachedOriginalGravity = 1f;
    private bool isAttached = false;

    // Potential player in trigger zone (can attach/detach with Q)
    private Rigidbody2D potentialRb;
    private PlayerMovement potentialMovement;
    private bool playerInZone = false;

    // Ladder vertical bounds (computed from collider)
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
        float nextY = attachedRb.position.y + v * climbSpeed * Time.fixedDeltaTime;

        const float eps = 0.001f;

        if (nextY >= topY - eps)
        {
            Vector2 exitPos = new Vector2(transform.position.x, topY);
            attachedRb.MovePosition(exitPos);
            
            // Preserve upward momentum when exiting the top of the ladder.
            attachedRb.linearVelocity = new Vector2(attachedRb.linearVelocity.x, Mathf.Max(0f, v * climbSpeed));
            Detach();
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
            attachedMovement.inputEnabled = false;

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
            attachedMovement.inputEnabled = true;

        attachedRb = null;
        attachedMovement = null;
        isAttached = false;
    }
}
