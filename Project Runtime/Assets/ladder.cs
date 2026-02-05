using UnityEngine;

/// <summary>
/// Ladder trigger that lets a player attach by pressing W while inside the ladder area.
/// While attached the player is locked to the ladder X and can move up/down with Vertical axis (W/S).
/// Reaching the top or bottom will place the player slightly off the ladder and detach them.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ladder : MonoBehaviour
{
    [Tooltip("Climb speed in units per second.")]
    [SerializeField] private float climbSpeed = 3f;

    [Tooltip("Vertical offset to place the player above the ladder when reaching the top so they can walk off.")]
    [SerializeField] private float topExitOffset = 0.12f;

    private Collider2D col;

    // Player state while in/around ladder
    private Rigidbody2D attachedRb;
    private PlayerMovement attachedMovement;
    private float attachedOriginalGravity = 1f;
    private bool isAttached = false;

    // Potential player in trigger zone (not attached until pressing W)
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
        // Attach only when player is inside zone and presses W
        if (!isAttached && playerInZone && potentialRb != null)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                attachedRb = potentialRb;
                attachedMovement = potentialMovement;
                attachedOriginalGravity = attachedRb != null ? attachedRb.gravityScale : 1f;
                Attach();
            }
        }
    }

    void FixedUpdate()
    {
        if (!isAttached || attachedRb == null) return;

        float v = Input.GetAxis("Vertical");
        float nextY = attachedRb.position.y + v * climbSpeed * Time.fixedDeltaTime;

        const float eps = 0.001f;

        // If nextY would reach or pass the top, place player slightly above and detach
        if (nextY >= topY - eps)
        {
            Vector2 exitPos = new Vector2(transform.position.x, topY + topExitOffset);
            attachedRb.MovePosition(exitPos);
            attachedRb.linearVelocity = Vector2.zero;
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

        // Clear potential values so attach requires re-enter and W-press
        potentialRb = null;
        potentialMovement = null;
        playerInZone = false;
    }
}
