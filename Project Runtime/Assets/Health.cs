using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField, Min(1)] private int maxHP = 100;          
    [SerializeField, Min(0f)] private float iFrameDuration = 0.5f; // Invincibility duration after being hit

    [Header("Events")]
    public UnityEvent onDamage;   // Called every time damage is taken (even if 0 damage during i-frames)
    public UnityEvent onDeath;    // Called when HP reaches 0

    public int CurrentHP { get; private set; }   // Current HP, readable by other scripts
    public bool IsInvincible { get; private set; } // True during i-frames

    private float iFrameTimer;

    private void Awake()
    {
        CurrentHP = maxHP;
    }


    /// Applies damage to this health component. Respects i-frames: no damage during invincibility.

    /// <param name="amount">Amount of damage to deal.</param>
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;   // Ignore non-positive damage

        // If currently invincible, ignore the damage
        if (IsInvincible) return;

        // Apply damage
        CurrentHP -= amount;
        onDamage?.Invoke();

        // Check death
        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            onDeath?.Invoke();
            // Optionally disable/destroy the object here, or let other scripts handle it
            // gameObject.SetActive(false); // example
            return;
        }

        // Start invincibility frames
        StartIFrames();
    }

    /// <summary>
    /// Heals by the given amount, up to maxHP.
    /// </summary>
    /// <param name="amount">Amount to heal.</param>
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        CurrentHP = Mathf.Min(CurrentHP + amount, maxHP);
    }

    /// <summary>
    /// Activates i-frames for the set duration.
    /// </summary>
    private void StartIFrames()
    {
        if (iFrameDuration <= 0f) return; // No i-frames if duration is 0
        IsInvincible = true;
        iFrameTimer = iFrameDuration;
    }

    private void Update()
    {
        // Handle i-frame countdown
        if (IsInvincible)
        {
            iFrameTimer -= Time.deltaTime;
            if (iFrameTimer <= 0f)
            {
                IsInvincible = false;
                iFrameTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Resets health to max and clears invincibility. Useful for respawning.
    /// </summary>
    public void ResetHealth()
    {
        CurrentHP = maxHP;
        IsInvincible = false;
        iFrameTimer = 0f;
    }
}