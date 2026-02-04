using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeathSpike : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Robustly find PlayerMovement on collider or parent
        PlayerMovement player = collision.collider.GetComponentInParent<PlayerMovement>();
        if (player != null)
            player.Die();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player != null)
            player.Die();
    }
}
