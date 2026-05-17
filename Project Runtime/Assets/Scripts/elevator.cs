using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class elevator : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    public float speed = 20f;
    private Vector3 nextPosition;
    private Rigidbody2D _rb;

    private bool _isMoving = false;
    private bool _playerOnboard;
    private const float ArrivalThreshold = 0.1f;


    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Start()
    {
        nextPosition = pointB.position;
    }

    void FixedUpdate()
    {
        if (!_isMoving)
            return;

        Vector3 target = Vector3.MoveTowards(_rb.position, nextPosition, speed * Time.fixedDeltaTime);
        _rb.MovePosition(target);

        if (Vector2.Distance(_rb.position, nextPosition) <= ArrivalThreshold)
        {
            nextPosition = (nextPosition == pointA.position) ? pointB.position : pointA.position;
            _isMoving = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent = transform;
            _playerOnboard = true;
            _isMoving = true;
        }
    }

}
