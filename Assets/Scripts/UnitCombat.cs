using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private int damage = 10;

    private Animator animator;

    private float attackTimer;

    private bool isFighting;
    private bool isMoving;

    private Unit unit;
    private Unit target;

    private GameManager gameManager;
    private UnitHealth health;

    private void Awake()
    {
        unit =
            GetComponent<Unit>();

        gameManager =
            FindAnyObjectByType<GameManager>();

        animator =
            GetComponent<Animator>();

        health =
            GetComponent<UnitHealth>();
    }

    // =========================================================
    // START COMBAT
    // =========================================================

    public void StartCombat()
    {
        if (health != null &&
            health.IsDead())
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

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!isFighting)
            return;

        // -----------------------------------------------------
        // THIS UNIT IS DEAD
        // -----------------------------------------------------

        if (health != null &&
            health.IsDead())
        {
            return;
        }

        // -----------------------------------------------------
        // TARGET INVALID
        // -----------------------------------------------------

        if (target == null ||
            IsTargetDead())
        {
            target = null;

            FindClosestTarget();
        }

        // -----------------------------------------------------
        // NO TARGET
        // -----------------------------------------------------

        if (target == null)
        {
            SetIdle();
            return;
        }

        // -----------------------------------------------------
        // DISTANCE
        // -----------------------------------------------------

        float distance =
            Vector3.Distance(
                transform.position,
                target.transform.position
            );

        // -----------------------------------------------------
        // MOVE
        // -----------------------------------------------------

        if (distance > attackRange)
        {
            SetMoving();

            MoveToTarget();

            return;
        }

        // -----------------------------------------------------
        // ATTACK
        // -----------------------------------------------------

        SetIdle();

        Attack();
    }

    // =========================================================
    // CHECK TARGET
    // =========================================================

    private bool IsTargetDead()
    {
        if (target == null)
            return true;

        // Target no longer belongs to a cell
        if (target.currentCell == null)
            return true;

        // Target object has been returned to pool
        if (!target.gameObject.activeInHierarchy)
            return true;

        UnitHealth targetHealth =
            target.GetComponent<UnitHealth>();

        if (targetHealth == null)
            return false;

        return targetHealth.IsDead();
    }

    // =========================================================
    // FIND CLOSEST TARGET
    // =========================================================

    private void FindClosestTarget()
    {
        if (gameManager == null)
            return;

        if (unit == null)
            return;

        if (unit.currentCell == null)
            return;

        GridCell[] targetCells;

        // -----------------------------------------------------
        // PLAYER → FIND ENEMY
        // -----------------------------------------------------

        if (unit.currentCell.team == Team.Player)
        {
            targetCells =
                gameManager.GetEnemyCells();
        }
        // -----------------------------------------------------
        // ENEMY → FIND PLAYER
        // -----------------------------------------------------
        else
        {
            targetCells =
                gameManager.GetPlayerCells();
        }

        if (targetCells == null)
            return;

        float closestDistance =
            Mathf.Infinity;

        target = null;

        // -----------------------------------------------------
        // SEARCH ALL CELLS
        // -----------------------------------------------------

        foreach (GridCell cell in targetCells)
        {
            if (cell == null)
                continue;

            if (!cell.IsOccupied)
                continue;

            Unit otherUnit =
                cell.currentUnit;

            if (otherUnit == null)
                continue;

            if (otherUnit == unit)
                continue;

            // -------------------------------------------------
            // CHECK OTHER UNIT HEALTH
            // -------------------------------------------------

            UnitHealth otherHealth =
                otherUnit.GetComponent<UnitHealth>();

            if (otherHealth != null &&
                otherHealth.IsDead())
            {
                continue;
            }

            // -------------------------------------------------
            // CHECK ACTIVE
            // -------------------------------------------------

            if (!otherUnit.gameObject.activeInHierarchy)
                continue;

            // -------------------------------------------------
            // CHECK DISTANCE
            // -------------------------------------------------

            float distance =
                Vector3.Distance(
                    transform.position,
                    otherUnit.transform.position
                );

            if (distance < closestDistance)
            {
                closestDistance =
                    distance;

                target =
                    otherUnit;
            }
        }

        // -----------------------------------------------------
        // DEBUG
        // -----------------------------------------------------

        if (target != null)
        {
            Debug.Log(
                unit.name +
                " targets " +
                target.name
            );
        }
    }

    // =========================================================
    // SET MOVING
    // =========================================================

    private void SetMoving()
    {
        if (isMoving)
            return;

        isMoving = true;

        if (animator == null)
            return;

        animator.ResetTrigger("Attack");

        animator.SetBool(
            "IsMoving",
            true
        );

        animator.Play(
            "Run",
            0,
            0f
        );
    }

    // =========================================================
    // SET IDLE
    // =========================================================

    private void SetIdle()
    {
        if (!isMoving)
        {
            if (animator != null)
            {
                animator.SetBool(
                    "IsMoving",
                    false
                );
            }

            return;
        }

        isMoving = false;

        if (animator == null)
            return;

        animator.SetBool(
            "IsMoving",
            false
        );

        animator.ResetTrigger("Attack");

        animator.Play(
            "Idle",
            0,
            0f
        );
    }

    // =========================================================
    // MOVE TO TARGET
    // =========================================================

    private void MoveToTarget()
    {
        if (target == null)
            return;

        if (IsTargetDead())
        {
            target = null;
            return;
        }

        Vector3 direction =
            target.transform.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    direction
                );
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                target.transform.position,
                moveSpeed *
                Time.deltaTime
            );
    }

    // =========================================================
    // ATTACK
    // =========================================================

    private void Attack()
    {
        if (target == null)
            return;

        if (IsTargetDead())
        {
            target = null;
            return;
        }

        attackTimer -=
            Time.deltaTime;

        if (attackTimer > 0f)
            return;

        attackTimer =
            attackCooldown;

        if (animator != null)
        {
            animator.SetTrigger(
                "Attack"
            );
        }

        UnitHealth targetHealth =
            target.GetComponent<UnitHealth>();

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(
                damage
            );
        }
    }

    // =========================================================
    // STATUS
    // =========================================================

    public bool IsFighting()
    {
        return isFighting;
    }

    // =========================================================
    // STOP COMBAT
    // =========================================================

    public void StopCombat()
    {
        isFighting = false;

        target = null;

        attackTimer = 0f;

        isMoving = false;

        if (animator == null)
            return;

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
}