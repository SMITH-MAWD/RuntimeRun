using UnityEngine;
using System.Collections;
using UnityEngine.Events;   // for UnityEvent

public class eyeSmash : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public float followSpeed = 10f;
    public float hoverY = -296f;

    [Header("Smash Timing")]
    public float minTimeBetweenSmash = 3f;
    public float pauseBeforeSmash = 1f;

    [Header("Drop Acceleration")]
    public float dropAcceleration = 40f;
    public float maxDropSpeed = 80f;

    [Header("Rise & Return")]
    public float riseDuration = 2f;
    public AnimationCurve riseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float returnSpeed = 12f;

    [Header("Ground")]
    public Transform groundPoint;

    [Header("Hit & Effects")]
    public float hitRadius = 1.2f;
    public GameObject killEffect;
    public GameObject impactEffect;
    public float impactDelay = 0.1f;

    [Header("Impact Event (for camera shake)")]
    public UnityEvent onImpactEvent;   // Drag impulse source's GenerateImpulse here

    private enum State { Follow, Wait, Drop, Rise, Return }
    private State state = State.Follow;
    private float sinceLastSmash = 0f;
    private float waitTimer;
    private Vector2 dropTarget;
    private float currentDropSpeed;
    private Vector2 riseStart;
    private Vector2 riseEnd;
    private float riseProgress;
    private bool hasKilled = false;
    private bool isDestroying = false;
    private bool impactTriggered = false;

    void Start()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        Vector3 pos = transform.position;
        pos.y = hoverY;
        transform.position = pos;
    }

    void Update()
    {
        switch (state)
        {
            case State.Follow:
                Vector3 target = new Vector3(player != null ? player.position.x : transform.position.x, hoverY, 0);
                transform.position = Vector3.MoveTowards(transform.position, target, followSpeed * Time.deltaTime);

                sinceLastSmash += Time.deltaTime;
                if (sinceLastSmash >= minTimeBetweenSmash && player != null)
                    StartSmash();
                break;

            case State.Wait:
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                    StartDrop();
                break;

            case State.Drop:
                currentDropSpeed = Mathf.Min(currentDropSpeed + dropAcceleration * Time.deltaTime, maxDropSpeed);
                transform.position = Vector2.MoveTowards(transform.position, dropTarget, currentDropSpeed * Time.deltaTime);
                if (!impactTriggered && Vector2.Distance(transform.position, dropTarget) < 0.1f)
                    OnImpact();
                break;

            case State.Rise:
                riseProgress += Time.deltaTime / riseDuration;
                float t = riseCurve.Evaluate(riseProgress);
                float newY = Mathf.Lerp(riseStart.y, riseEnd.y, t);
                transform.position = new Vector2(transform.position.x, newY);
                if (riseProgress >= 1f)
                    StartReturn();
                break;

            case State.Return:
                if (player != null)
                {
                    Vector3 targetPos = new Vector3(player.position.x, hoverY, 0);
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, returnSpeed * Time.deltaTime);
                    if (Mathf.Abs(transform.position.x - player.position.x) < 0.1f)
                    {
                        state = State.Follow;
                        sinceLastSmash = 0f;
                        hasKilled = false;
                        Vector3 pos = transform.position;
                        pos.y = hoverY;
                        pos.x = player.position.x;
                        transform.position = pos;
                    }
                }
                else
                {
                    state = State.Follow;
                }
                break;
        }
    }

    void StartSmash()
    {
        if (state != State.Follow) return;
        state = State.Wait;
        waitTimer = pauseBeforeSmash;
        hasKilled = false;
        impactTriggered = false;
    }

    void StartDrop()
    {
        state = State.Drop;
        currentDropSpeed = 0f;
        float groundY = (groundPoint != null) ? groundPoint.position.y : GetGroundY();
        dropTarget = new Vector2(transform.position.x, groundY);
    }

    float GetGroundY()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 50f);
        return hit.collider != null ? hit.point.y : transform.position.y - 10f;
    }

    void OnImpact()
    {
        if (impactTriggered) return;
        impactTriggered = true;

        // Fire the UnityEvent – this will call GenerateImpulse if set up
        onImpactEvent.Invoke();

        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        CheckHit();
        StartCoroutine(DelayedRise());
    }

    IEnumerator DelayedRise()
    {
        yield return new WaitForSeconds(impactDelay);
        StartRise();
    }

    void CheckHit()
    {
        if (hasKilled) return;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, hitRadius);
        if (hit != null)
            KillPlayer(hit);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasKilled) return;
        KillPlayer(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasKilled) return;
        KillPlayer(collision.collider);
    }

    void KillPlayer(Collider2D other)
    {
        if (hasKilled || isDestroying) return;
        PlayerMovement playerScript = other.GetComponentInParent<PlayerMovement>();
        if (playerScript != null)
        {
            hasKilled = true;
            isDestroying = true;
            playerScript.Die();
            if (killEffect != null)
                Instantiate(killEffect, transform.position, Quaternion.identity);
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            Destroy(gameObject, 0.01f);
        }
    }

    void StartRise()
    {
        state = State.Rise;
        riseProgress = 0f;
        riseStart = transform.position;
        riseEnd = new Vector2(riseStart.x, hoverY);
    }

    void StartReturn()
    {
        state = State.Return;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}