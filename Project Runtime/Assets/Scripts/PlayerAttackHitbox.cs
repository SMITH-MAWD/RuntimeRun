using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerAttackHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private bool hitboxActive;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public void SetActive(bool active)
    {
        hitboxActive = active;
        if (col != null)
            col.enabled = active;
    }

    public void EnableHitbox() => SetActive(true);
    public void DisableHitbox() => SetActive(false);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hitboxActive)
            return;

        Health health = other.GetComponent<Health>();
        if (health == null)
            health = other.GetComponentInParent<Health>();

        if (health != null)
            health.TakeDamage(damage);
    }
}
