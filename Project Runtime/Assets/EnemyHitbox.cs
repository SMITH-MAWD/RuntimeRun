using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public RushEnemy parentEnemy;
    public int attackIndex;      // 0,1,2,... matches the enemy's attacks list

    private RushEnemy.AttackHitbox myAttack;

    void Start()
    {
        if (parentEnemy == null)
        {
            parentEnemy = GetComponentInParent<RushEnemy>();
            if (parentEnemy == null)
            {
                Debug.LogError($"EnemyHitbox on {name} cannot find parent RushEnemy!");
                return;
            }
        }

        if (attackIndex >= 0 && attackIndex < parentEnemy.attacks.Count)
            myAttack = parentEnemy.attacks[attackIndex];
        else
            Debug.LogWarning($"Hitbox {name} has invalid attackIndex {attackIndex}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && myAttack != null && parentEnemy != null)
        {
            parentEnemy.OnHitboxTrigger(other, myAttack);
        }
    }
}