using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossAttackHitbox : MonoBehaviour
{
    [SerializeField] private BossAI boss;
    [SerializeField] private int damage = 15;
    [SerializeField] private bool hitboxActive;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (boss == null)
            boss = GetComponentInParent<BossAI>();
    }

    public void SetActive(bool active)
    {
        hitboxActive = active;
        if (col != null)
            col.enabled = active;
    }

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hitboxActive || boss == null)
            return;

        if (!other.CompareTag("Player"))
            return;

        Health playerHealth = other.GetComponent<Health>();
        if (playerHealth == null)
            playerHealth = other.GetComponentInParent<Health>();

        if (playerHealth != null)
            playerHealth.TakeDamage(damage);
    }
}
