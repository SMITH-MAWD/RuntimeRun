using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Collider2D))]
public class jumpboost : MonoBehaviour
{
    [Header("Double-tap settings")]
    [SerializeField] private float doubleTapMaxTime = 0.35f;

    [Header("Timeline (PlayableDirector)")]
    [SerializeField] private PlayableDirector timelineToPlay;

    [Header("OR simple Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animationTriggerName = "Boost";

    [Header("Bounce power")]
    [SerializeField] private float bouncePower = 25f;

    private bool playerInRange;
    private float lastWTime = -10f;
    private bool triggeredThisSession = false;
    private Rigidbody2D playerRb;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        lastWTime = -10f;
        triggeredThisSession = false;
        playerRb = other.GetComponent<Rigidbody2D>();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        lastWTime = -10f;
        playerRb = null;
        triggeredThisSession = false;
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            float now = Time.time;
            if (now - lastWTime <= doubleTapMaxTime)
            {
                OnDoubleTap();
                lastWTime = -10f;
            }
            else
            {
                lastWTime = now;
            }
        }

        if (lastWTime > 0f && Time.time - lastWTime > doubleTapMaxTime)
            lastWTime = -10f;
    }

    private void OnDoubleTap()
    {
        if (triggeredThisSession) return;

        // Play Timeline (force restart)
        if (timelineToPlay != null)
        {
            timelineToPlay.Stop();   // Stop if already playing
            timelineToPlay.time = 0; // Rewind to beginning
            timelineToPlay.Play();   // Start fresh
        }
        // Fallback to Animator
        else if (animator != null)
        {
            animator.SetTrigger(animationTriggerName);
        }
        else
        {
            Debug.LogWarning("No Timeline or Animator – only bounce.");
        }

        // Apply bounce
        if (playerRb != null)
        {
            Vector2 vel = playerRb.linearVelocity;
            vel.y = bouncePower;
            playerRb.linearVelocity = vel;
        }

        triggeredThisSession = true;
    }

    public void ResetTriggerLock()
    {
        triggeredThisSession = false;
    }
}