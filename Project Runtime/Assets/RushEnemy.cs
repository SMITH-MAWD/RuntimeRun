using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RushEnemy : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 1f;
    public float cooldownAfterDash = 2f;

    [Header("Attacks (Hitboxes)")]
    public List<AttackHitbox> attacks = new List<AttackHitbox>();

    [Header("Effects")]
    public GameObject dashTrailPrefab;
    public GameObject impactEffectPrefab;
    public AudioClip dashSound;
    public AudioClip hitSound;

    private enum State { Idle, Dashing, Cooldown }
    private State currentState = State.Idle;
    private Vector2 dashDirection;
    private float dashTimer;
    private float cooldownTimer;
    private bool hasHitThisDash = false;
    private GameObject currentTrail;
    private AudioSource audioSource;
    private Transform player;
    private AttackHitbox chosenAttack;

    [System.Serializable]
    public class AttackHitbox
    {
        public string attackName = "Punch";
        public Collider2D hitboxCollider;
        public float hitboxActiveTime = 0.2f;
        public int damage = 1;
        public float delayAfterDashStart = 0f;
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Disable all hitboxes and their visuals initially
        foreach (var attack in attacks)
        {
            if (attack.hitboxCollider != null)
            {
                attack.hitboxCollider.enabled = false;
                SpriteRenderer sr = attack.hitboxCollider.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false;
            }
        }

        if (dashSound != null || hitSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
                    StartDash();
                break;

            case State.Dashing:
                dashTimer -= Time.deltaTime;
                transform.Translate(dashDirection * dashSpeed * Time.deltaTime, Space.World);
                if (dashTimer <= 0f && !hasHitThisDash)
                    EndDash(false);
                break;

            case State.Cooldown:
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    currentState = State.Idle;
                    hasHitThisDash = false;
                }
                break;
        }
    }

    void StartDash()
    {
        if (currentState != State.Idle) return;

        if (attacks.Count == 0)
        {
            Debug.LogWarning("No attacks assigned to RushEnemy!");
            return;
        }

        chosenAttack = attacks[Random.Range(0, attacks.Count)];

        currentState = State.Dashing;
        hasHitThisDash = false;
        dashTimer = dashDuration;

        if (player != null)
            dashDirection = (player.position - transform.position).normalized;
        else
            dashDirection = Vector2.right;

        FlipSprite(dashDirection.x);

        StartCoroutine(ActivateHitboxWithDelay(chosenAttack));

        if (dashSound != null && audioSource != null)
            audioSource.PlayOneShot(dashSound);
        if (dashTrailPrefab != null)
            currentTrail = Instantiate(dashTrailPrefab, transform.position, Quaternion.identity, transform);

        Debug.Log($"Enemy dashes, will use {chosenAttack.attackName}");
    }

    IEnumerator ActivateHitboxWithDelay(AttackHitbox attack)
    {
        yield return new WaitForSeconds(attack.delayAfterDashStart);

        if (currentState != State.Dashing) yield break;
        if (attack.hitboxCollider == null) yield break;

        // Enable collider
        attack.hitboxCollider.enabled = true;

        // Enable visual (SpriteRenderer) – if present
        SpriteRenderer sr = attack.hitboxCollider.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;

        yield return new WaitForSeconds(attack.hitboxActiveTime);

        if (!hasHitThisDash && attack.hitboxCollider != null)
        {
            attack.hitboxCollider.enabled = false;
            if (sr != null) sr.enabled = false;
        }
    }

    void EndDash(bool hitPlayer)
    {
        if (currentTrail != null) Destroy(currentTrail);

        // Disable all hitboxes and their visuals
        foreach (var attack in attacks)
        {
            if (attack.hitboxCollider != null)
            {
                attack.hitboxCollider.enabled = false;
                SpriteRenderer sr = attack.hitboxCollider.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false;
            }
        }

        currentState = State.Cooldown;
        cooldownTimer = cooldownAfterDash;
        if (hitPlayer) Debug.Log($"Enemy hit with {chosenAttack?.attackName}");
        else Debug.Log("Enemy missed");
    }

    // Called by the hitbox's own trigger script (EnemyHitbox)
    public void OnHitboxTrigger(Collider2D other, AttackHitbox attack)
    {
        if (currentState != State.Dashing) return;
        if (hasHitThisDash) return;
        if (other.CompareTag("Player"))
        {
            HitPlayer(other, attack);
        }
    }

    void HitPlayer(Collider2D playerCollider, AttackHitbox attack)
    {
        hasHitThisDash = true;

        PlayerMovement playerScript = playerCollider.GetComponentInParent<PlayerMovement>();
        if (playerScript != null)
            playerScript.Die();   // or playerScript.TakeDamage(attack.damage)

        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);

        // Disable all hitboxes & visuals immediately
        foreach (var att in attacks)
        {
            if (att.hitboxCollider != null)
            {
                att.hitboxCollider.enabled = false;
                SpriteRenderer sr = att.hitboxCollider.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false;
            }
        }

        EndDash(true);
    }

    void FlipSprite(float directionX)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = directionX < 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}