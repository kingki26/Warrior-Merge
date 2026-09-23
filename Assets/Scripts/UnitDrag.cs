using UnityEngine;
using UnityEngine.InputSystem;

public class UnitDrag : MonoBehaviour
{
    [SerializeField] private LayerMask gridCellLayer;

    private Unit unit;
    private UnitCombat unitCombat;
    private RangedCombat rangedCombat;
    private GridCell originalCell;

    private bool isDragging;
    private float dragHeight;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        unitCombat = GetComponent<UnitCombat>();
        rangedCombat = GetComponent<RangedCombat>();
    }

    private void OnMouseDown()
    {
        if (unit.currentCell == null)
            return;

        if (unitCombat != null && unitCombat.IsFighting())
        {
            return;
        }

        if (rangedCombat != null && rangedCombat.IsFighting())
        {
            return;
        }

        isDragging = true;

        originalCell = unit.currentCell;

        dragHeight = transform.position.y;
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        Plane boardPlane = new Plane(Vector3.up,new Vector3(0,dragHeight,0));

        if (boardPlane.Raycast(ray,out float distance))
        {
            transform.position = ray.GetPoint(distance);
        }
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray,out RaycastHit hit,100f,gridCellLayer))
        {
            GridCell targetCell = hit.collider.GetComponent<GridCell>();

            if (targetCell != null)
            {
                if (!targetCell.IsOccupied)
                {
                    unit.SetCell(targetCell);
                    return;
                }


                Unit targetUnit = targetCell.currentUnit;

                if (CanMerge(targetUnit))
                {
                    Merge(targetUnit);
                    return;
                }
            }
        }

        unit.SetCell(originalCell);
    }

    private bool CanMerge(Unit targetUnit)
    {
        if (targetUnit == null)
            return false;

        if (targetUnit == unit)
            return false;

        if (targetUnit.unitType != unit.unitType)
            return false;

        if (targetUnit.level != unit.level)
            return false;

        return true;
    }

    // MERGE

    private void Merge(Unit targetUnit)
    {
        UnitPool unitPool = FindAnyObjectByType<UnitPool>();

        if (unitPool == null)
        {

            unit.SetCell(originalCell);
            return;
        }

        // LƯU THÔNG TIN

        UnitType unitType = unit.unitType;

        int currentLevel = unit.level;

        int nextLevel = currentLevel + 1;

        GridCell mergeCell = targetUnit.currentCell;

        if (mergeCell == null)
        {
            unit.SetCell(originalCell);
            return;
        }

        // KIỂM TRA LEVEL

        if (nextLevel > 5)
        {
            unit.SetCell(originalCell);
            return;
        }


        Unit newUnit = unitPool.GetPlayerUnit(unitType, nextLevel);

        if (newUnit == null)
        {
            unit.SetCell(originalCell);
            return;
        }

        unitPool.ReturnPlayerUnit(targetUnit);

        unitPool.ReturnPlayerUnit(unit);

        newUnit.SetCell(mergeCell);

    }
}