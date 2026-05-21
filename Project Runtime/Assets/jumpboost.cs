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

    [Header("Audio")]
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private AudioSource audioSource;  // Optional – auto-created if empty

    private bool playerInRange;
    private float lastWTime = -10f;
    private bool triggeredThisSession = false;
    private Rigidbody2D playerRb;

    void Awake()
    {
        // If no AudioSource is assigned, create one
        if (audioSource == null && bounceSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Make absolutely sure the sound never plays on its own
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D
        }
    }

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

        PlayBounceSound();

        if (timelineToPlay != null)
        {
            timelineToPlay.Stop();
            timelineToPlay.time = 0;
            timelineToPlay.Play();
        }
        else if (animator != null)
        {
            animator.SetTrigger(animationTriggerName);
        }

        if (playerRb != null)
        {
            Vector2 vel = playerRb.linearVelocity;
            vel.y = bouncePower;
            playerRb.linearVelocity = vel;
        }

        triggeredThisSession = true;
    }

    private void PlayBounceSound()
    {
        if (bounceSound == null || audioSource == null) return;
        audioSource.PlayOneShot(bounceSound);
    }

    public void ResetTriggerLock()
    {
        triggeredThisSession = false;
    }
}