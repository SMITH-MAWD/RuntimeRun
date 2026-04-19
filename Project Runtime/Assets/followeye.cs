using UnityEngine;

public class followeye : MonoBehaviour
{
    [Tooltip("Transform to follow (preferred). If null the script will try to find the GameObject with tag 'Player'.")]
    public Transform target;

    [Tooltip("Follow speed in world units per second. Set large for near-instant movement.")]
    public float followSpeed = 10f;

    [Tooltip("Fixed Z position to keep (useful for 2D).")]
    public float fixedZ = -10f;

    [Header("Lock Y")]
    [Tooltip("When true the eye's Y will be fixed to fixedY (eye only moves left/right).")]
    public bool lockY = true;
    [Tooltip("Fixed world Y position for the eye when lockY is enabled. If left NaN the current transform.y is used.")]
    public float fixedY = float.NaN;

    [Header("Optional X clamp")]
    [Tooltip("Clamp X between minX and maxX")]
    public bool useXClamp = false;
    public float minX = -Mathf.Infinity;
    public float maxX = Mathf.Infinity;

    void Start()
    {
        if (target == null)
            TryFindPlayer();

        if (lockY && float.IsNaN(fixedY))
            fixedY = transform.position.y;
    }

    void Update()
    {
        if (target == null)
        {
            TryFindPlayer();
            if (target == null) return;
        }

        float desiredX = target.position.x;
        if (useXClamp)
            desiredX = Mathf.Clamp(desiredX, minX, maxX);

        float desiredY = lockY ? fixedY : target.position.y;
        Vector3 current = transform.position;
        Vector3 desired = new Vector3(desiredX, desiredY, fixedZ);

        if (followSpeed <= 0f)
            transform.position = desired;
        else
            transform.position = Vector3.MoveTowards(current, desired, followSpeed * Time.deltaTime);
    }

    private void TryFindPlayer()
    {
        var go = GameObject.FindWithTag("Player");
        if (go != null)
            target = go.transform;
    }
}