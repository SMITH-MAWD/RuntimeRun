using UnityEngine;

public class EyeMovementLimiter : MonoBehaviour
{
    public Transform limiterBox; // The big square object (used for position + scale)
    public float insideOffset = 0f; // Optional: keep object away from the edge

    private Bounds bounds;

    void Start()
    {
        // Calculate the world-space bounds of the limiter box
        // Assumes limiterBox has a Renderer or Collider; if not, compute from position + scale
        Collider col = limiterBox.GetComponent<Collider>();
        if (col != null)
            bounds = col.bounds;
        else
        {
            Vector3 center = limiterBox.position;
            Vector3 size = limiterBox.localScale; // Assumes uniform scale or you can set manually
            bounds = new Bounds(center, size);
        }

        // Shrink bounds slightly if insideOffset > 0
        if (insideOffset > 0)
            bounds.Expand(-insideOffset * 2);
    }

    void Update()
    {
        // Move your object here (e.g., with arrow keys or physics)
        float moveSpeed = 5f;
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0) * moveSpeed * Time.deltaTime;
        Vector3 newPos = transform.position + move;

        // Clamp to bounds
        newPos.x = Mathf.Clamp(newPos.x, bounds.min.x, bounds.max.x);
        newPos.y = Mathf.Clamp(newPos.y, bounds.min.y, bounds.max.y);
        newPos.z = Mathf.Clamp(newPos.z, bounds.min.z, bounds.max.z);

        transform.position = newPos;
    }
}