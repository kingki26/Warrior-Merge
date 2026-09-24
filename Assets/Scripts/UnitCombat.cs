using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private int damage = 10;

    private float attackTimer;

    private bool isFighting;
    private bool isMoving;

    private Unit unit;
    private Unit target;

    private GameManager gameManager;
    private Animator animator;
    private UnitHealth health;

    public void Init(Unit unit, GameManager gameManager)
    {
        this.unit = unit;
        this.gameManager = gameManager;

        if (unit != null)
        {
            animator = unit.Animator;
            health = unit.Health;
        }
    }

    public void StartCombat()
    {
        if (health != null && health.IsDead())
        {
            return;
        }

        isFighting = true;
        attackTimer = 0f;
        target = null;

        FindClosestTarget();

        if (target != null)
        {
            SetMoving();
        }
        else
        {
            SetIdle();
        }
    }

    private void Update()
    {
        if (!isFighting)
        {
            return;
        }

        if (health != null && health.IsDead())
        {
            return;
        }

        if (target == null || IsTargetDead())
        {
            target = null;
            FindClosestTarget();
        }

        if (target == null)
        {
            SetIdle();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > attackRange)
        {
            SetMoving();
            MoveToTarget();
            return;
        }

        SetIdle();
        Attack();
    }

    private bool IsTargetDead()
    {
        if (target == null)
        {
            return true;
        }

        if (target.currentCell == null)
        {
            return true;
        }

        if (!target.gameObject.activeInHierarchy)
        {
            return true;
        }

        UnitHealth targetHealth = target.Health;

        if (targetHealth == null)
        {
            return false;
        }

        return targetHealth.IsDead();
    }

    private void FindClosestTarget()
    {
        if (gameManager == null)
        {
            return;
        }

        if (unit == null)
        {
            return;
        }

        if (unit.currentCell == null)
        {
            return;
        }

        GridCell[] targetCells;

        if (unit.currentCell.team == Team.Player)
        {
            targetCells = gameManager.GetEnemyCells();
        }
        else
        {
            targetCells = gameManager.GetPlayerCells();
        }

        if (targetCells == null)
        {
            return;
        }

        float closestDistance = Mathf.Infinity;
        target = null;

        foreach (GridCell cell in targetCells)
        {
            if (cell == null)
            {
                continue;
            }

            if (!cell.IsOccupied)
            {
                continue;
            }

            Unit otherUnit = cell.currentUnit;

            if (otherUnit == null)
            {
                continue;
            }

            if (otherUnit == unit)
            {
                continue;
            }

            UnitHealth otherHealth = otherUnit.Health;

            if (otherHealth != null && otherHealth.IsDead())
            {
                continue;
            }

            if (!otherUnit.gameObject.activeInHierarchy)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, otherUnit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = otherUnit;
            }
        }
    }

    private void SetMoving()
    {
        if (isMoving)
        {
            return;
        }

        isMoving = true;

        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger("Attack");
        animator.SetBool("IsMoving", true);
        animator.Play("Run", 0, 0f);
    }

    private void SetIdle()
    {
        if (!isMoving)
        {
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }

            return;
        }

        isMoving = false;

        if (animator == null)
        {
            return;
        }

        animator.SetBool("IsMoving", false);
        animator.ResetTrigger("Attack");
        animator.Play("Idle", 0, 0f);
    }

    private void MoveToTarget()
    {
        if (target == null)
        {
            return;
        }

        if (IsTargetDead())
        {
            target = null;
            return;
        }

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
    }

    private void Attack()
    {
        if (target == null)
        {
            return;
        }

        if (IsTargetDead())
        {
            target = null;
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
        {
            return;
        }

        attackTimer = attackCooldown;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        UnitHealth targetHealth = target.Health;

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }
    }

    public bool IsFighting()
    {
        return isFighting;
    }

    public void StopCombat()
    {
        isFighting = false;
        target = null;
        attackTimer = 0f;
        isMoving = false;

        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Victory");
        animator.SetBool("IsMoving", false);
        animator.Play("Idle", 0, 0f);
    }
}