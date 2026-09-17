using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private bool isDead;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ResetHealth();
    }

    // ========================================
    // RESET
    // ========================================

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    // ========================================
    // DAMAGE
    // ========================================

    public void TakeDamage(int damage)
    {
        Debug.Log(
            gameObject.name +
            " RECEIVED DAMAGE: " +
            damage
        );

        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log(
            gameObject.name +
            " HP: " +
            currentHealth
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ========================================
    // DIE
    // ========================================

    private void Die()
    {
        isDead = true;

        Debug.Log(
            gameObject.name +
            " died!"
        );

        Unit unit =
            GetComponent<Unit>();

        if (unit != null &&
            unit.currentCell != null)
        {
            unit.currentCell.RemoveUnit();

            GameManager gameManager =
                FindAnyObjectByType<GameManager>();

            if (gameManager != null)
            {
                gameManager.CheckVictory();
                gameManager.CheckDefeat();
            }
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Invoke(
            nameof(ReturnToPool),
            2f
        );
    }

    // ========================================
    // RETURN POOL
    // ========================================

    private void ReturnToPool()
    {
        Unit unit =
            GetComponent<Unit>();

        if (unit == null)
        {
            gameObject.SetActive(false);
            return;
        }

        UnitPool unitPool =
            FindAnyObjectByType<UnitPool>();

        if (unitPool == null)
        {
            Debug.LogError(
                "UnitHealth → UnitPool not found!"
            );

            gameObject.SetActive(false);
            return;
        }

        unitPool.ReturnUnit(unit);
    }

    // ========================================
    // PUBLIC
    // ========================================

    public bool IsDead()
    {
        return isDead;
    }
}