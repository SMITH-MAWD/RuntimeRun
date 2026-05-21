using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(TouchingDirections))]
public class Knight : MonoBehaviour
{
    public float moveSpeed = 5f;

    public enum WalkDirection {
        Left,
        Right
    }

    private Rigidbody2D rb;
    private TouchingDirections touchingDirection;
    private Vector2 walkDirectionVector = Vector2.right;
    private WalkDirection _walkDirection = WalkDirection.Right;

    public WalkDirection Direction {
        get
        {
            return _walkDirection;
        }
        set {
            if (_walkDirection != value) {
                gameObject.transform.localScale = new Vector2(gameObject.transform.localScale.x * -1, gameObject.transform.localScale.y);

                if (value == WalkDirection.Right)
                {
                    walkDirectionVector = Vector2.right;
                } else if (value == WalkDirection.Left)
                {
                    walkDirectionVector = Vector2.left;
                }

            }
            _walkDirection = value;
        }
    }

   public void Awake() {
        rb = GetComponent<Rigidbody2D>();
        touchingDirection = GetComponent<TouchingDirections>();
    }

    public void FixedUpdate() {

        if (touchingDirection.IsGrounded && touchingDirection.IsOnWall)
        {
            FlipDirection();
        }
        rb.linearVelocity = new Vector2(walkDirectionVector.x * moveSpeed, rb.linearVelocity.y);
    }

    private void FlipDirection() {
        Direction = Direction == WalkDirection.Right
            ? WalkDirection.Left
            : WalkDirection.Right;
    }
}
