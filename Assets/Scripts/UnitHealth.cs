using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private bool isDead;

    private Animator animator;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        Debug.Log(gameObject.name + " RECEIVED DAMAGE: " + damage);
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log(
            gameObject.name + " HP: " + currentHealth
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        Debug.Log(gameObject.name + " died!");

        Unit unit = GetComponent<Unit>();

        if (unit != null && unit.currentCell != null)
        {
            unit.currentCell.RemoveUnit();

            GameManager gameManager =
                FindAnyObjectByType<GameManager>();

            if (gameManager != null)
            {
                if (unit.GetComponent<UnitHealth>() != null)
                {
                    gameManager.CheckVictory();
                    gameManager.CheckDefeat();
                }
            }
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Destroy(gameObject, 2f);
    }

    public bool IsDead()
    {
        return isDead;
    }
}