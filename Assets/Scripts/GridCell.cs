using UnityEngine;

public class GridCell : MonoBehaviour
{
    [Header("Cell Info")]
    public Team team;

    [Header("Current Unit")]
    public Unit currentUnit;

    public bool IsOccupied
    {
        get
        {
            return currentUnit != null;
        }
    }
    public void SetUnit(Unit unit)
    {
        currentUnit = unit;
    }

    public void RemoveUnit()
    {
        currentUnit = null;
    }
}