using UnityEngine;

public class Elevator2DPoints : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] private Transform pointA;   // Point A – the first position
    [SerializeField] private Transform pointB;   // Point B – the second position

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool pingPong = true;      // Go back and forth?
    [SerializeField] private float waitTimeAtPoints = 0f; // Seconds to wait at each end
    [SerializeField] private bool startAtPointA = true; // Snap to point A when the game starts?

    private Vector2 posA;
    private Vector2 posB;
    private Vector2 currentTarget;
    private bool movingToB = true;
    private float waitTimer;
    private bool isWaiting;

    private void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("Elevator2DPoints: Both Point A and Point B must be assigned!", this);
            enabled = false;
            return;
        }

        posA = pointA.position;
        posB = pointB.position;

        if (startAtPointA)
            transform.position = posA;

        currentTarget = posB; // Begin by moving toward point B
    }

    private void Update()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                SwitchTarget();
            }
            return;
        }

        // Move toward current target
        transform.position = Vector2.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);

        // Check arrival
        if (Vector2.Distance(transform.position, currentTarget) < 0.01f)
        {
            transform.position = currentTarget;

            if (waitTimeAtPoints > 0f)
            {
                isWaiting = true;
                waitTimer = waitTimeAtPoints;
            }
            else
            {
                SwitchTarget();
            }
        }
    }

    private void SwitchTarget()
    {
        if (!pingPong)
        {
            enabled = false; // Stop moving after reaching the destination
            return;
        }

        movingToB = !movingToB;
        currentTarget = movingToB ? posB : posA;
    }

    // Visual helpers in the Scene view
    private void OnDrawGizmosSelected()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.25f);
            Gizmos.DrawWireSphere(pointB.position, 0.25f);
        }
    }
}