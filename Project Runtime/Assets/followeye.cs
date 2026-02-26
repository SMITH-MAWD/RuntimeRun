using UnityEngine;

public class followeye : MonoBehaviour
{
    [Tooltip("Assign the player's Transform. If left empty the script will try to find the GameObject with tag 'Player'.")]
    public Transform player;

    [Header("Trigger area (optional)")]
    [Tooltip("If assigned, the eye will only follow when the player is inside this Collider2D's bounds.")]
    public Collider2D triggerArea;

    [Header("Follow bounds (world coordinates)")]
    public float minX = 48f;
    public float maxX = 69f;
    public float minY = 27f;
    public float maxY = 28f;

    [Header("Motion")]
    [Tooltip("How fast the eye moves to the target position. Use a large value for near-instant movement.")]
    public float followSpeed = 20f;

    // internal flag to avoid repeated calls to FindWithTag
    private bool triedFindPlayer = false;

    void Start()
    {
        if (player == null)
        {
            TryFindPlayer();
        }
    }

    void Update()
    {
        if (player == null)
        {
            if (!triedFindPlayer) TryFindPlayer();
            return;
        }

        // Only follow when player is inside the trigger area if one is set.
        bool insideArea = true;
        if (triggerArea != null)
        {
            insideArea = triggerArea.OverlapPoint(player.position);
        }

        if (!insideArea)
            return;

        // Compute the desired (clamped) position based on player's position
        float targetX = Mathf.Clamp(player.position.x, minX, maxX);
        float targetY = Mathf.Clamp(player.position.y, minY, maxY);
        Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);

        // Smoothly move the eye towards the target position. For instant movement set followSpeed to a very large value.
        if (followSpeed <= 0f)
            transform.position = targetPos;
        else
            transform.position = Vector3.Lerp(transform.position, targetPos, Mathf.Clamp01(followSpeed * Time.deltaTime));
    }

    private void TryFindPlayer()
    {
        triedFindPlayer = true;
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
            player = p.transform;
    }
}
