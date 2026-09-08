using UnityEngine;

public enum UnitType
{
    Melee,
    Ranged
}

public class Unit : MonoBehaviour
{
    [Header("Unit Info")]
    public UnitType unitType;
    public int level = 1;

    [Header("Cell")]
    public GridCell currentCell;

    public bool SetCell(GridCell newCell)
    {
        if (newCell == null)
            return false;

        if (newCell.IsOccupied && newCell != currentCell)
            return false;

        if (currentCell != null)
        {
            currentCell.RemoveUnit();
        }

        currentCell = newCell;
        currentCell.SetUnit(this);

        transform.position = newCell.transform.position;

        return true;
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
}