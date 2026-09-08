using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int damage = 10;

    private float attackTimer;

    private Unit unit;
    private Unit target;
    private GameManager gameManager;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    public void StartCombat()
    {
        FindClosestTarget();
    }

    private void Update()
    {
        if (target == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            target.transform.position
        );

        if (distance > attackRange)
        {
            MoveToTarget();
            return;
        }

        Attack();
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

        foreach (GridCell cell in targetCells)
        {
            if (!cell.IsOccupied)
                continue;

            Unit otherUnit = cell.currentUnit;

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
            Debug.Log(unit.name + " targets " + target.name);
        }
    }

    private void MoveToTarget()
    {
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

        UnitHealth targetHealth = target.GetComponent<UnitHealth>();

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }
    }
}