using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private int damage = 10;

    private Animator animator;
    private float attackTimer;
    private bool isFighting;

    private Unit unit;
    private Unit target;
    private GameManager gameManager;
    private UnitHealth health;

    private bool isMoving;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        gameManager = FindAnyObjectByType<GameManager>();
        animator = GetComponent<Animator>();
        health = GetComponent<UnitHealth>();
    }

    public void StartCombat()
    {
        isFighting = true;

        FindClosestTarget();

        if (target != null)
        {
            SetMoving();
        }
    }

    private void Update()
    {
        if (!isFighting)
            return;

        // Bản thân đã chết
        if (health != null && health.IsDead())
            return;

        // Target không tồn tại hoặc đã chết
        if (target == null || IsTargetDead())
        {
            target = null;

            FindClosestTarget();
        }

        // Không còn target
        if (target == null)
        {
            SetMoving();
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            target.transform.position
        );

        // =========================
        // TARGET Ở XA
        // =========================

        if (distance > attackRange)
        {
            SetMoving();
            MoveToTarget();
            return;
        }

        // =========================
        // TARGET Ở GẦN
        // =========================

        SetIdle();
        Attack();
    }

    private bool IsTargetDead()
    {
        if (target == null)
            return true;

        UnitHealth targetHealth =
            target.GetComponent<UnitHealth>();

        if (targetHealth == null)
            return false;

        return targetHealth.IsDead();
    }

    private void FindClosestTarget()
    {
        GridCell[] targetCells;

        if (unit.currentCell.team == Team.Player)
        {
            targetCells = gameManager.GetEnemyCells();
        }
        else
        {
            targetCells = gameManager.GetPlayerCells();
        }

        float closestDistance = Mathf.Infinity;

        target = null;

        foreach (GridCell cell in targetCells)
        {
            if (!cell.IsOccupied)
                continue;

            Unit otherUnit = cell.currentUnit;

            if (otherUnit == null)
                continue;

            UnitHealth otherHealth =
                otherUnit.GetComponent<UnitHealth>();

            // Không chọn Unit đã chết
            if (otherHealth != null && otherHealth.IsDead())
                continue;

            float distance = Vector3.Distance(
                transform.position,
                otherUnit.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = otherUnit;
            }
        }

        if (target != null)
        {
            Debug.Log(
                unit.name + " targets " + target.name
            );
        }
    }

    private void SetMoving()
    {
        if (isMoving)
            return;

        isMoving = true;

        // Xóa các trigger cũ
        animator.ResetTrigger("Attack");

        animator.SetBool("IsMoving", true);

        // Ép Animator vào Run
        animator.Play("Run", 0, 0f);
    }

    private void SetIdle()
    {
        if (!isMoving)
            return;

        isMoving = false;

        animator.SetBool("IsMoving", false);

        animator.ResetTrigger("Attack");

        // Ép Animator vào Idle
        animator.Play("Idle", 0, 0f);

    }

    private void MoveToTarget()
    {
        if (target == null)
            return;

        Vector3 direction =
            target.transform.position - transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.transform.position,
            moveSpeed * Time.deltaTime
        );
    }

    private void Attack()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;

        animator.SetTrigger("Attack");

        UnitHealth targetHealth =
            target.GetComponent<UnitHealth>();

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

    animator.ResetTrigger("Attack");
    animator.ResetTrigger("Victory");

    animator.SetBool("IsMoving", false);

    animator.Play("Idle", 0, 0f);
}
}