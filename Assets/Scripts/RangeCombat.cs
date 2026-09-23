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

        gameManager = FindAnyObjectByType<GameManager>();

        health = GetComponent<UnitHealth>();

        animator = GetComponent<Animator>();
    }
    public void StartCombat()
    {
        if (health != null && health.IsDead())
            return;

        isFighting = true;
        isAttacking = false;

        attackTimer = 0f;

        FindClosestTarget();
    }
    private void Update()
    {
        if (!isFighting)
            return;

        if (health != null && health.IsDead())
            return;

        if (isAttacking)
            return;

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

        float distance = Vector3.Distance(transform.position,target.transform.position);

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
            return;

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

        float closestDistance = Mathf.Infinity;

        target = null;

        foreach (GridCell cell in targetCells)
        {
            if (!cell.IsOccupied)
                continue;

            Unit otherUnit = cell.currentUnit;

            if (otherUnit == null)
                continue;

            UnitHealth otherHealth = otherUnit.GetComponent<UnitHealth>();

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
            return true;

        if (target.currentCell == null)
            return true;

        if (!target.gameObject.activeInHierarchy)
            return true;

        UnitHealth targetHealth = target.GetComponent<UnitHealth>();

        if (targetHealth == null)
            return false;

        return targetHealth.IsDead();
    }

    private void FaceTarget()
    {
        if (target == null)
            return;

        Vector3 direction = target.transform.position - transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void Attack()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        attackTimer = unit.attackCooldown;

        isAttacking = true;


        if (animator != null)
        {
            animator.Play("Attack", 0, 0f);
        }

        StartCoroutine(
            ShootBulletAfterDelay()
        );
    }

    private IEnumerator ShootBulletAfterDelay()
    {
        yield return new WaitForSeconds(
            shootDelay
        );

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

        BulletProjectile bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);


        bullet.Setup( target,unit.damage);
        isAttacking = false;
    }
    private void SetIdle()
    {
        if (animator == null)
            return;

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

            animator.Play( "Idle", 0, 0f);
        }
    }
    public bool IsFighting()
    {
        return isFighting;
    }
}