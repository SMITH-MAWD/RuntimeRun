using System.Collections;
using UnityEngine;

// Attach to any entity that should "die" and respawn when touching a DeathSpike
public class dieandspawn : MonoBehaviour
{
    [SerializeField, Tooltip("Seconds to wait before respawning")]
    private float respawnDelay = 1f;

    [SerializeField, Tooltip("Optional spawn transform. If unset, the object's initial position is used.")]
    private Transform spawnPoint;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Animator animator;

    private bool isDead = false;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponentInParent<DeathSpike>() != null)
            StartCoroutine(DieAndRespawn());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<DeathSpike>() != null)
            StartCoroutine(DieAndRespawn());
    }

    private IEnumerator DieAndRespawn()
    {
        if (isDead) yield break;
        isDead = true;

        // Play death animation if available
        if (animator != null)
            animator.SetTrigger("Die");

        // Disable physics and visuals
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        if (col != null)
            col.enabled = false;

        if (sr != null)
            sr.enabled = false;

        // Wait before respawning
        yield return new WaitForSeconds(respawnDelay);

        // Move back to spawn position
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : initialPosition;
        transform.position = spawnPos;
        transform.rotation = initialRotation;

        // Restore visuals and physics
        if (sr != null)
            sr.enabled = true;

        if (col != null)
            col.enabled = true;

        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Play respawn animation if available
        if (animator != null)
            animator.SetTrigger("Respawn");

        isDead = false;
    }
}
