using UnityEngine;

public class doormovment : MonoBehaviour
{
    [Tooltip("Optional: assign player Transform. If left blank the script will try to find GameObject with tag 'Player' at runtime.")]
    public Transform player;

    [Tooltip("Distance at which the object will start moving toward the target.")]
    public float triggerDistance = 3f;

    [Tooltip("Local offset from the starting position to move to when triggered.")]
    public Vector3 moveDirection = new Vector3(0f, -3f, 0f);

    [Tooltip("Movement speed (units per second)")]
    public float moveSpeed = 2f;

    [Tooltip("If true the object will move only once. If false it will toggle back and forth each trigger.")]
    public bool moveOnce = true;

    [Tooltip("If true the script will disable itself after completing the move (keeps object at target).")]
    public bool disableAfterMove = true;

    [Tooltip("If true the door will open when the player is within Trigger Distance and close when the player moves away. Uses hysteresis to avoid jitter.")]
    public bool proximityControlled = true;

    [Tooltip("Small gap added to triggerDistance when deciding to close again (prevents rapid open/close around the boundary)")]
    public float hysteresis = 0.25f;

    Vector3 startPos;
    Vector3 targetPos;
    bool moving = false;
    bool hasMoved = false; // used by non-proximity one-time behavior
    bool isOpen = false;   // current open/closed state used by proximityControlled

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + moveDirection;

        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        if (proximityControlled)
        {
            // decide whether to open or close using hysteresis to avoid flicker
            if (!isOpen && dist <= triggerDistance)
            {
                isOpen = true;
                moving = true;
            }
            else if (isOpen && dist >= triggerDistance + hysteresis)
            {
                isOpen = false;
                moving = true;
            }

            Vector3 desired = isOpen ? targetPos : startPos;
            if (moving)
            {
                transform.position = Vector3.MoveTowards(transform.position, desired, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, desired) < 0.001f)
                {
                    transform.position = desired;
                    moving = false;
                }
            }
        }
        else
        {
            // legacy behaviour: move once or toggle back and forth when triggered
            if (!hasMoved && !moving)
            {
                if (dist <= triggerDistance)
                {
                    moving = true;
                }
            }

            if (moving)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPos) < 0.001f)
                {
                    transform.position = targetPos;
                    moving = false;
                    hasMoved = true;

                    if (!moveOnce)
                    {
                        // swap target and start so it can move back on the next trigger
                        Vector3 tmp = startPos;
                        startPos = targetPos;
                        targetPos = tmp;
                        hasMoved = false; // allow next movement
                    }
                    else
                    {
                        // If configured to disable after moving once, disable this component so it won't react again.
                        if (disableAfterMove)
                        {
                            enabled = false;
                        }
                    }
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + moveDirection);
    }
}
