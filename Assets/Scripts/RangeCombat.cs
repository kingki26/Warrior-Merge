using System.Collections;
using UnityEngine;

public class RangedCombat : MonoBehaviour
{
    [Header("Ranged Settings")]
    [SerializeField] private float detectionRange = 10f;

    [Header("Bullet")]
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Attack Animation")]
    [SerializeField] private float shootDelay = 0.6f;

    private Unit unit;
    private Unit target;

    private GameManager gameManager;
    private UnitHealth health;
    private Animator animator;

    private float attackTimer;

    private bool isFighting;
    private bool isAttacking;

    private void Awake()
    {
        unit = GetComponent<Unit>();

        gameManager =
            FindAnyObjectByType<GameManager>();

        health =
            GetComponent<UnitHealth>();

        animator =
            GetComponent<Animator>();
    }

    // ========================================
    // START COMBAT
    // ========================================

    public void StartCombat()
    {
        if (health != null && health.IsDead())
            return;

        isFighting = true;
        isAttacking = false;

        attackTimer = 0f;

        FindClosestTarget();
    }

    // ========================================
    // UPDATE
    // ========================================

    private void Update()
    {
        if (!isFighting)
            return;

        if (health != null && health.IsDead())
            return;

        // Đang chạy Attack animation
        if (isAttacking)
            return;

        // Target chết hoặc biến mất
        if (target == null || IsTargetDead())
        {
            target = null;

            FindClosestTarget();
        }

        // Không có target
        if (target == null)
        {
            SetIdle();

            if (gameManager != null)
            {
                gameManager.CheckVictory();
            }

            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                target.transform.position
            );

        // Target ra ngoài tầm
        if (distance > detectionRange)
        {
            target = null;

            SetIdle();

            return;
        }

        // Target trong tầm
        FaceTarget();

        Attack();
    }

    // ========================================
    // FIND CLOSEST TARGET
    // ========================================

    private void FindClosestTarget()
    {
        if (gameManager == null)
            return;

        if (unit.currentCell == null)
        {
            Debug.LogError(
        unit.name +
        " → currentCell is NULL! Cannot find target."
            );

            return;

        }
            

        GridCell[] targetCells;

        if (unit.currentCell.team == Team.Player)
        {
            targetCells =
                gameManager.GetEnemyCells();
        }
        else
        {
            targetCells =
                gameManager.GetPlayerCells();
        }

        float closestDistance =
            Mathf.Infinity;


        Debug.Log(
        unit.name +
        " → Searching target from Cell: " +
        unit.currentCell.name +
        " | Team: " +
        unit.currentCell.team
        );
        target = null;

        foreach (GridCell cell in targetCells)
        {
            if (!cell.IsOccupied)
                continue;

            Unit otherUnit =
                cell.currentUnit;

            if (otherUnit == null)
                continue;

            UnitHealth otherHealth =
                otherUnit.GetComponent<UnitHealth>();

            if (otherHealth != null &&
                otherHealth.IsDead())
            {
                continue;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    otherUnit.transform.position
                );

            if (distance <= detectionRange &&
                distance < closestDistance)
            {
                closestDistance =
                    distance;

                target =
                    otherUnit;
            }
        }

        if (target != null)
        {
            //Debug.Log(
            //    unit.name +
            //    " targets " +
            //    target.name
            //);
        }
    }

    // ========================================
    // TARGET DEAD?
    // ========================================

    private bool IsTargetDead()
    {
        if (target == null)
            return true;

        // Target không còn nằm trên Cell
        if (target.currentCell == null)
            return true;

        // Target đã bị disable / trả về Pool
        if (!target.gameObject.activeInHierarchy)
            return true;

        UnitHealth targetHealth =
            target.GetComponent<UnitHealth>();

        if (targetHealth == null)
            return false;

        return targetHealth.IsDead();
    }

    // ========================================
    // FACE TARGET
    // ========================================

    private void FaceTarget()
    {
        if (target == null)
            return;

        Vector3 direction =
            target.transform.position -
            transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    // ========================================
    // ATTACK
    // ========================================

    private void Attack()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        attackTimer =
            unit.attackCooldown;

        isAttacking = true;

        //Debug.Log(
        //    unit.name +
        //    " ATTACK!"
        //);

        if (animator != null)
        {
            animator.Play(
                "Attack",
                0,
                0f
            );
        }

        StartCoroutine(
            ShootBulletAfterDelay()
        );
    }

    // ========================================
    // SHOOT BULLET AFTER DELAY
    // ========================================

    private IEnumerator ShootBulletAfterDelay()
    {
        yield return new WaitForSeconds(
            shootDelay
        );

        // ====================================
        // COMBAT ĐÃ DỪNG
        // ====================================

        if (!isFighting)
        {
            isAttacking = false;
            yield break;
        }

        // ====================================
        // BẢN THÂN CHẾT
        // ====================================

        if (health != null &&
            health.IsDead())
        {
            isAttacking = false;
            yield break;
        }

        // ====================================
        // TARGET ĐÃ CHẾT
        // ====================================

        if (target == null ||
            IsTargetDead())
        {
            // Hủy attack hiện tại
            isAttacking = false;

            // Bỏ target cũ
            target = null;

            // Về Idle
            SetIdle();

            // Tìm target mới
            FindClosestTarget();

            // Cho phép attack lại ngay
            attackTimer = 0f;

            yield break;
        }

        // ====================================
        // BULLET PREFAB
        // ====================================

        if (bulletPrefab == null)
        {
            Debug.LogError(
                unit.name +
                " chưa có Bullet Prefab!"
            );

            isAttacking = false;
            SetIdle();

            yield break;
        }

        // ====================================
        // FIRE POINT
        // ====================================

        if (firePoint == null)
        {
            Debug.LogError(
                unit.name +
                " chưa có Fire Point!"
            );

            isAttacking = false;
            SetIdle();

            yield break;
        }

        // ====================================
        // SPAWN BULLET
        // ====================================

        BulletProjectile bullet =
            Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );

        // Damage lấy từ Unit
        bullet.Setup(
            target,
            unit.damage
        );

        Debug.Log(
            unit.name +
            " FIRED BULLET → " +
            target.name +
            " | Damage: " +
            unit.damage
        );

        // Attack đã hoàn thành
        isAttacking = false;
    }

    // ========================================
    // IDLE
    // ========================================

    private void SetIdle()
    {
        if (animator == null)
            return;

        animator.Play(
            "Idle",
            0,
            0f
        );
    }

    // ========================================
    // STOP COMBAT
    // ========================================

    public void StopCombat()
    {
        isFighting = false;
        isAttacking = false;

        target = null;

        attackTimer = 0f;

        StopAllCoroutines();

        if (animator != null)
        {
            animator.ResetTrigger("Attack");

            animator.Play(
                "Idle",
                0,
                0f
            );
        }
    }

    // ========================================
    // IS FIGHTING
    // ========================================

    public bool IsFighting()
    {
        return isFighting;
    }
}