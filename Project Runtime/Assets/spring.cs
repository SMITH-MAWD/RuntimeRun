using UnityEngine;

public class spring : MonoBehaviour
{
    public float bounceForce = 10f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerBounce(collision.gameObject);
        }
    }

    private void HandlePlayerBounce(GameObject player)
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        if (playerRb)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f); // Reset vertical velocity
            Vector2 vector2 = new(playerRb.linearVelocity.x, bounceForce);
            playerRb.linearVelocity = vector2; // Apply bounce force
            playerRb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        }

    }
}

