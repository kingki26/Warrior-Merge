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

            animator.SetBool( "IsMoving",false);

            animator.Play("Idle", 0, 0f);
        }

        // Cập nhật HP Bar về đầy
        HealthBar healthBar = GetComponentInChildren<HealthBar>();

        if (healthBar != null)
        {
            healthBar.Setup();
        }
    }
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (damage <= 0)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0);


        // Cập nhật HP Bar
        HealthBar healthBar = GetComponentInChildren<HealthBar>();

        if (healthBar != null)
        {
            healthBar.Refresh();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        CancelInvoke(nameof(ReturnToPool));


        Unit unit = GetComponent<Unit>();

        if (unit != null && unit.currentCell != null)
        {
            ownerTeam = unit.currentCell.team;

            unit.currentCell.RemoveUnit();

            unit.currentCell = null;

            GameManager gameManager = FindAnyObjectByType<GameManager>();

            if (gameManager != null)
            {
                gameManager.CheckVictory();
                gameManager.CheckDefeat();
            }
        }

        // HP Bar về 0
        HealthBar healthBar = GetComponentInChildren<HealthBar>();

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
        Invoke( nameof(ReturnToPool),2f);
    }

    private void ReturnToPool()
    {
        Unit unit = GetComponent<Unit>();

        if (unit == null)
        {
            gameObject.SetActive(false);
            return;
        }

        UnitPool unitPool = FindAnyObjectByType<UnitPool>();

        if (unitPool == null)
        {
            gameObject.SetActive(false);

            return;
        }

        if (ownerTeam == Team.Player)
        {
            unitPool.ReturnPlayerUnit(unit);

            return;
        }

        if (ownerTeam == Team.Enemy)
        {
            unitPool.ReturnEnemyUnit(unit);

            return;
        }

        unit.gameObject.SetActive(false);
    }

    public bool IsDead()
    {
        return isDead;
    }
}