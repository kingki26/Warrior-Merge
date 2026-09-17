using UnityEngine;

public enum UnitType
{
    Melee,
    Ranged
}

public class Unit : MonoBehaviour
{
    [Header("Cell")]
    public GridCell currentCell;

    [Header("Unit Info")]
    public UnitType unitType;
    public int level = 1;

    [Header("Stats")]
    public int maxHealth = 100;
    public int damage = 10;
    public float attackCooldown = 0.8f;

    public void SetCell(GridCell newCell)
    {
        if (newCell == null)
            return;

        if (currentCell != null)
        {
            currentCell.RemoveUnit();
        }

        currentCell = newCell;
        currentCell.SetUnit(this);

        transform.position = newCell.transform.position;
    }

    public void FaceTarget(Transform target)
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public void ResetForReuse()
    {
        currentCell = null;

        transform.rotation =
            Quaternion.identity;

        UnitHealth health =
            GetComponent<UnitHealth>();

        if (health != null)
        {
            health.ResetHealth();
        }

        UnitCombat meleeCombat =
            GetComponent<UnitCombat>();

        if (meleeCombat != null)
        {
            meleeCombat.StopCombat();
        }

        RangedCombat rangedCombat =
            GetComponent<RangedCombat>();

        if (rangedCombat != null)
        {
            rangedCombat.StopCombat();
        }

        Animator animator =
            GetComponent<Animator>();

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Victory");
            animator.ResetTrigger("Die");

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
}