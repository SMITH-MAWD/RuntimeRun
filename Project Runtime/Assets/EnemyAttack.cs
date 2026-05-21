using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Example: press Space to attack during testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("Combo");
        }
    }
}