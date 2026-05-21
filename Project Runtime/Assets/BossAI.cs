using UnityEngine;

[DisallowMultipleComponent]
public class BossAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private float maxDashDuration = 2f;
    [SerializeField] private float attackRange = 2.5f;

    [Header("Attack")]
    [SerializeField, Range(0f, 1f)] private float comboChance = 0.75f;
    [SerializeField] private float comboCooldown = 1.25f;
    [SerializeField] private int comboDamage = 15;
    [SerializeField] private int comboVarDamage = 25;

    [Header("Hurt")]
    [SerializeField] private float hurtLockDuration = 0.45f;

    [Header("Trigger Detection (Empty Child with BoxCollider2D)")]
    [SerializeField] private Collider2D attackTriggerZone;          // Drag the empty child trigger here
    [SerializeField] private bool useTriggerZoneInsteadOfDistance = true;

    [Header("Knockback (Push)")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Animator Parameters")]
    [SerializeField] private string comboParam = "Combo";        // Now a bool
    [SerializeField] private string comboVarParam = "ComboVar";  // Now a bool
    [SerializeField] private string dashParam = "Dash";
    [SerializeField] private string deadParam = "Dead";
    [SerializeField] private string hurtHorseParam = "HurtHorse";
    [SerializeField] private string hurtNoHorseParam = "HurtNohorse";

    [Header("Hitboxes")]
    [SerializeField] private BossAttackHitbox attackHitbox;   // For damage (can also be a plain Collider2D)

    private Animator anim;
    private Health health;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private SlashEffectSpawner slashSpawner;
    private Rigidbody2D playerRb;   // For knockback

    private bool isDead;
    private bool isDashing;
    private bool isAttacking;
    private bool isInCombo;
    private bool isInComboVar;
    private bool isHurting;
    private float dashTimer;
    private float dashCooldownTimer;
    private float attackCooldownTimer;
    private float hurtTimer;
    private float lockedY;
    private int dashDirectionX;
    private bool attackFacingLocked;
    private bool lockedFlipX;
    private Vector3 attackHitboxDefaultLocalPos;
    private bool attackHitboxPosCached;

    public bool IsInCombo => isInCombo;
    public bool IsInComboVar => isInComboVar;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        health = GetComponent<Health>();
        if (health == null)
            health = gameObject.AddComponent<Health>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        slashSpawner = GetComponent<SlashEffectSpawner>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody2D>();
        }

        EnsureAttackHitbox();
        CacheAttackHitboxDefaultPosition();
    }

    private void CacheAttackHitboxDefaultPosition()
    {
        if (attackHitbox == null || attackHitboxPosCached)
            return;

        attackHitboxDefaultLocalPos = attackHitbox.transform.localPosition;
        attackHitboxPosCached = true;
    }

    private void Start()
    {
        lockedY = transform.position.y;
        health.onDamage.AddListener(OnBossDamaged);
        health.onDeath.AddListener(OnBossDeath);
        dashCooldownTimer = 0f;

        if (useTriggerZoneInsteadOfDistance && attackTriggerZone != null)
        {
            var col = attackTriggerZone as BoxCollider2D;
            if (col != null && !col.isTrigger)
                Debug.LogWarning("Attack trigger zone should have 'Is Trigger' enabled.", this);
        }
    }

    private void Update()
    {
        if (isDead || player == null)
            return;

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (hurtTimer > 0f)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0f)
                isHurting = false;
        }

        if (attackCooldownTimer > 0f && !isAttacking)
            attackCooldownTimer -= Time.deltaTime;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            MoveHorizontal(dashDirectionX * dashSpeed * Time.deltaTime);

            float distanceWhileDashing = HorizontalDistanceToPlayer();
            if (distanceWhileDashing <= attackRange)
                EndDash();
            else if (dashTimer <= 0f)
                EndDash();

            return;
        }

        if (isHurting)
            return;

        if (isAttacking)
        {
            ApplyLockedFacing();
            return;
        }

        FacePlayer();

        if (useTriggerZoneInsteadOfDistance && attackTriggerZone != null)
        {
            // Attack triggered by OnTriggerEnter2D
            ChasePlayer();
            if (dashCooldownTimer <= 0f)
                StartDash();
        }
        else
        {
            float distance = HorizontalDistanceToPlayer();
            if (distance <= attackRange && attackCooldownTimer <= 0f)
            {
                PerformComboAttack();
            }
            else
            {
                ChasePlayer();
                if (dashCooldownTimer <= 0f)
                    StartDash();
            }
        }
    }

    private void LateUpdate()
    {
        bool flipX = attackFacingLocked ? lockedFlipX : spriteRenderer != null && spriteRenderer.flipX;
        slashSpawner?.SyncFacing(flipX, force: attackFacingLocked);
        SyncAttackHitboxFacing(flipX);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || player == null) return;
        if (!useTriggerZoneInsteadOfDistance) return;
        if (attackTriggerZone == null) return;

        if (other == attackTriggerZone && other.CompareTag("Player"))
        {
            if (!isAttacking && !isDashing && !isHurting && attackCooldownTimer <= 0f)
            {
                PerformComboAttack();
            }
        }
    }

    private void EnsureAttackHitbox()
    {
        if (attackHitbox != null) return;

        attackHitbox = GetComponentInChildren<BossAttackHitbox>(true);
        if (attackHitbox != null) return;

        GameObject hitboxObject = new GameObject("BossAttackHitbox");
        hitboxObject.transform.SetParent(transform, false);
        hitboxObject.transform.localPosition = new Vector3(0.08f, 0.02f, 0f);

        BoxCollider2D box = hitboxObject.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(0.35f, 0.28f);

        attackHitbox = hitboxObject.AddComponent<BossAttackHitbox>();
        attackHitbox.SetActive(false);

        attackHitboxDefaultLocalPos = hitboxObject.transform.localPosition;
        attackHitboxPosCached = true;
    }

    private void ChasePlayer()
    {
        float deltaX = Mathf.Sign(player.position.x - transform.position.x) * moveSpeed * Time.deltaTime;
        if (Mathf.Abs(player.position.x - transform.position.x) > 0.05f)
            MoveHorizontal(deltaX);
    }

    private void MoveHorizontal(float deltaX)
    {
        Vector3 pos = transform.position;
        pos.x += deltaX;
        pos.y = lockedY;
        transform.position = pos;
    }

    private float HorizontalDistanceToPlayer()
    {
        return Mathf.Abs(player.position.x - transform.position.x);
    }

    public void FacePlayer()
    {
        if (player == null || spriteRenderer == null || attackFacingLocked)
            return;

        spriteRenderer.flipX = player.position.x < transform.position.x;
    }

    private void ApplyLockedFacing()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.flipX = lockedFlipX;
    }

    private void SyncAttackHitboxFacing(bool flipX)
    {
        if (attackHitbox == null || !attackHitboxPosCached) return;

        float sign = flipX ? -1f : 1f;
        Transform hitboxTransform = attackHitbox.transform;
        hitboxTransform.localPosition = new Vector3(
            Mathf.Abs(attackHitboxDefaultLocalPos.x) * sign,
            attackHitboxDefaultLocalPos.y,
            attackHitboxDefaultLocalPos.z);
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = maxDashDuration;
        dashCooldownTimer = dashCooldown;
        dashDirectionX = player.position.x >= transform.position.x ? 1 : -1;

        if (anim != null)
            anim.SetBool(dashParam, true);
    }

    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;

        if (anim != null)
            anim.SetBool(dashParam, false);
    }

    private void PerformComboAttack()
    {
        if (isAttacking || anim == null) return;

        if (isDashing) EndDash();

        isAttacking = true;
        attackCooldownTimer = comboCooldown;
        attackFacingLocked = true;
        lockedFlipX = player.position.x < transform.position.x;
        ApplyLockedFacing();
        slashSpawner?.SyncFacing(lockedFlipX, force: true);
        SyncAttackHitboxFacing(lockedFlipX);

        // Using bools instead of triggers
        if (Random.value <= comboChance)
        {
            isInCombo = true;
            isInComboVar = false;
            anim.SetBool(comboParam, true);
            anim.SetBool(comboVarParam, false);
            attackHitbox?.SetDamage(comboDamage);
        }
        else
        {
            isInCombo = false;
            isInComboVar = true;
            anim.SetBool(comboParam, false);
            anim.SetBool(comboVarParam, true);
            attackHitbox?.SetDamage(comboVarDamage);
        }
    }

    public void EnableAttackHitbox()
    {
        attackHitbox?.SetActive(true);
    }

    public void DisableAttackHitbox()
    {
        attackHitbox?.SetActive(false);
    }

    public void DealDamageToPlayer()
    {
        if (player == null) return;

        int damage = isInComboVar ? comboVarDamage : comboDamage;
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth == null)
            playerHealth = player.GetComponentInParent<Health>();

        if (playerHealth != null)
            playerHealth.TakeDamage(damage);

        // Apply knockback / push to the player
        ApplyKnockbackToPlayer();
    }

    private void ApplyKnockbackToPlayer()
    {
        if (playerRb == null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb == null) return;
        }

        // Direction: away from the boss (or based on boss facing direction)
        float direction = lockedFlipX ? -1f : 1f; // If boss faces left, push left; if faces right, push right
        Vector2 knockbackDir = new Vector2(direction, 0.5f).normalized;
        playerRb.linearVelocity = Vector2.zero;
        playerRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

        // Optional: Disable player control briefly (if you have a PlayerController script)
        // You can implement a coroutine to re-enable after knockbackDuration.
    }

    public void OnAttackFinished()
    {
        isAttacking = false;
        isInCombo = false;
        isInComboVar = false;
        attackFacingLocked = false;
        DisableAttackHitbox();

        // Reset animator bools
        if (anim != null)
        {
            anim.SetBool(comboParam, false);
            anim.SetBool(comboVarParam, false);
        }

        FacePlayer();
        if (spriteRenderer != null)
        {
            slashSpawner?.SyncFacing(spriteRenderer.flipX, force: true);
            SyncAttackHitboxFacing(spriteRenderer.flipX);
        }
    }

    public void OnHurtFinished()
    {
        isHurting = false;
        hurtTimer = 0f;
    }

    private void OnBossDamaged()
    {
        if (isDead) return;

        isAttacking = false;
        attackFacingLocked = false;
        DisableAttackHitbox();
        isHurting = true;
        hurtTimer = hurtLockDuration;

        if (anim == null) return;

        // Reset combo bools
        anim.SetBool(comboParam, false);
        anim.SetBool(comboVarParam, false);

        if (isInComboVar)
            anim.SetTrigger(hurtNoHorseParam);
        else if (isInCombo)
            anim.SetTrigger(hurtHorseParam);
        else
            anim.SetTrigger(hurtNoHorseParam);

        isInCombo = false;
        isInComboVar = false;
    }

    private void OnBossDeath()
    {
        isDead = true;
        isDashing = false;
        isAttacking = false;
        isHurting = false;
        DisableAttackHitbox();

        if (anim != null)
        {
            anim.SetBool(dashParam, false);
            anim.SetBool(comboParam, false);
            anim.SetBool(comboVarParam, false);
            anim.SetTrigger(deadParam);
        }

        enabled = false;
    }

    private void OnDestroy()
    {
        if (health == null) return;
        health.onDamage.RemoveListener(OnBossDamaged);
        health.onDeath.RemoveListener(OnBossDeath);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}