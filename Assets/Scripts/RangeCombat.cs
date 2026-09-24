using System.Collections;
using UnityEngine;

public class RangedCombat : MonoBehaviour
{
    [Header("Ranged Settings")]
    [SerializeField] private float detectionRange = 10f;

    [Header("Bullet")]
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private Transform firePoint;
    private BulletPool bulletPool;

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
        health = unit.Health;
        animator = unit.Animator;
    }

    public void Init(Unit unit, GameManager gameManager)
    {
        this.unit = unit;
        this.gameManager = gameManager;

        if (unit != null)
        {
            health = unit.Health;
            animator = unit.Animator;
        }

        bulletPool = FindAnyObjectByType<BulletPool>();
    }

    public void StartCombat()
    {
        if (health != null && health.IsDead())
        {
            return;
        }

        isFighting = true;
        isAttacking = false;
        attackTimer = 0f;

        FindClosestTarget();
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

        if (isAttacking)
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

            if (gameManager != null)
            {
                gameManager.CheckVictory();
            }

            return;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > detectionRange)
        {
            target = null;
            SetIdle();
            return;
        }

        FaceTarget();
        Attack();
    }

    private void FindClosestTarget()
    {
        if (gameManager == null)
        {
            return;
        }

        if (unit == null || unit.currentCell == null)
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
            if (cell == null || !cell.IsOccupied)
            {
                continue;
            }

            Unit otherUnit = cell.currentUnit;

            if (otherUnit == null || otherUnit == unit)
            {
                continue;
            }

            UnitHealth otherHealth = otherUnit.Health;

            if (otherHealth != null && otherHealth.IsDead())
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, otherUnit.transform.position);

            if (distance <= detectionRange && distance < closestDistance)
            {
                closestDistance = distance;
                target = otherUnit;
            }
        }
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

    private void FaceTarget()
    {
        if (target == null)
        {
            return;
        }

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void Attack()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
        {
            return;
        }

        attackTimer = unit.attackCooldown;
        isAttacking = true;

        if (animator != null)
        {
            animator.Play("Attack", 0, 0f);
        }

        StartCoroutine(ShootBulletAfterDelay());
    }

    private IEnumerator ShootBulletAfterDelay()
    {
        yield return new WaitForSeconds(shootDelay);

        if (!isFighting)
        {
            isAttacking = false;
            yield break;
        }

        if (health != null && health.IsDead())
        {
            isAttacking = false;
            yield break;
        }

        if (target == null || IsTargetDead())
        {
            isAttacking = false;
            target = null;
            SetIdle();
            FindClosestTarget();
            attackTimer = 0f;
            yield break;
        }

        if (bulletPrefab == null)
        {
            isAttacking = false;
            SetIdle();
            yield break;
        }

        if (firePoint == null)
        {
            isAttacking = false;
            SetIdle();
            yield break;
        }

        if (bulletPool == null)
        {
            isAttacking = false;
            SetIdle();
            yield break;
        }

        BulletProjectile bullet = bulletPool.GetBullet(bulletPrefab);

        if (bullet == null)
        {
            isAttacking = false;
            SetIdle();
            yield break;
        }

        bullet.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
        bullet.Setup(target, unit.damage);

        isAttacking = false;
    }

    private void SetIdle()
    {
        if (animator == null)
        {
            return;
        }

        animator.Play("Idle", 0, 0f);
    }

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
            animator.Play("Idle", 0, 0f);
        }
    }

    public bool IsFighting()
    {
        return isFighting;
    }
}