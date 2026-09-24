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

    // Components
    public UnitHealth Health { get; private set; }
    public UnitCombat Combat { get; private set; }
    public RangedCombat RangedCombat { get; private set; }
    public Animator Animator { get; private set; }

    private void Awake()
    {
        Health = GetComponent<UnitHealth>();
        Combat = GetComponent<UnitCombat>();
        RangedCombat = GetComponent<RangedCombat>();
        Animator = GetComponent<Animator>();
    }

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

        transform.rotation = Quaternion.identity;

        if (Health != null)
        {
            Health.ResetHealth();
        }

        Canvas healthBarCanvas = GetComponentInChildren<Canvas>();

        if (healthBarCanvas != null)
        {
            healthBarCanvas.gameObject.SetActive(true);
        }

        if (Combat != null)
        {
            Combat.StopCombat();
        }

        if (RangedCombat != null)
        {
            RangedCombat.StopCombat();
        }

        if (Animator != null)
        {
            Animator.ResetTrigger("Attack");
            Animator.ResetTrigger("Victory");
            Animator.ResetTrigger("Die");

            Animator.SetBool("IsMoving", false);

            Animator.Play("Idle", 0, 0f);
        }
    }
}