using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private bool isDead;

    private Animator animator;

    // Lưu Team trước khi Unit bị remove khỏi Cell
    private Team ownerTeam;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        ResetHealth();
    }

    // =========================================================
    // HEALTH
    // =========================================================

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetHealthPercent()
    {
        if (maxHealth <= 0)
            return 0f;

        return (float)currentHealth / maxHealth;
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetHealth()
    {
        CancelInvoke(nameof(ReturnToPool));

        currentHealth = maxHealth;
        isDead = false;

        if (animator != null)
        {
            animator.ResetTrigger("Die");
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Victory");

            animator.SetBool(
                "IsMoving",
                false
            );

            animator.Play(
                "Idle",
                0,
                0f
            );
        }

        // Cập nhật HP Bar về đầy
        HealthBar healthBar =
            GetComponentInChildren<HealthBar>();

        if (healthBar != null)
        {
            healthBar.Setup();
        }
    }

    // =========================================================
    // TAKE DAMAGE
    // =========================================================

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (damage <= 0)
            return;

        currentHealth -= damage;

        currentHealth =
            Mathf.Max(
                currentHealth,
                0
            );

        Debug.Log(
            gameObject.name +
            " RECEIVED DAMAGE: " +
            damage +
            " | HP: " +
            currentHealth +
            "/" +
            maxHealth
        );

        // Cập nhật HP Bar
        HealthBar healthBar =
            GetComponentInChildren<HealthBar>();

        if (healthBar != null)
        {
            healthBar.Refresh();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // =========================================================
    // DIE
    // =========================================================

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        CancelInvoke(nameof(ReturnToPool));

        Debug.Log(
            gameObject.name +
            " died!"
        );

        Unit unit =
            GetComponent<Unit>();

        if (unit != null &&
            unit.currentCell != null)
        {
            ownerTeam =
                unit.currentCell.team;

            unit.currentCell.RemoveUnit();

            unit.currentCell = null;

            GameManager gameManager =
                FindAnyObjectByType<GameManager>();

            if (gameManager != null)
            {
                gameManager.CheckVictory();
                gameManager.CheckDefeat();
            }
        }

        // HP Bar về 0
        HealthBar healthBar =
            GetComponentInChildren<HealthBar>();

        if (healthBar != null)
        {
            healthBar.Refresh();
        }

        // Death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Sau 2 giây trả về Pool
        Invoke(
            nameof(ReturnToPool),
            2f
        );
    }

    // =========================================================
    // RETURN TO POOL
    // =========================================================

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

        if (ownerTeam == Team.Player)
        {
            unitPool.ReturnPlayerUnit(unit);

            Debug.Log(
                "UnitHealth → Returned Player Unit → " +
                unit.name
            );

            return;
        }

        if (ownerTeam == Team.Enemy)
        {
            unitPool.ReturnEnemyUnit(unit);

            Debug.Log(
                "UnitHealth → Returned Enemy Unit → " +
                unit.name
            );

            return;
        }

        Debug.LogError(
            "UnitHealth → Unknown owner Team for Unit → " +
            unit.name
        );

        unit.gameObject.SetActive(false);
    }

    // =========================================================
    // STATUS
    // =========================================================

    public bool IsDead()
    {
        return isDead;
    }
}