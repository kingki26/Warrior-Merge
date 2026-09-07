using UnityEngine;
using UnityEngine.InputSystem;

public class UnitDrag : MonoBehaviour
{
    [SerializeField] private LayerMask gridCellLayer;

    private Unit unit;
    private GridCell originalCell;

    private bool isDragging;
    private float dragHeight;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void OnMouseDown()
    {
        if (unit.currentCell == null)
            return;

        isDragging = true;
        originalCell = unit.currentCell;

        dragHeight = transform.position.y;
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        Ray ray = Camera.main.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        Plane boardPlane = new Plane(
            Vector3.up,
            new Vector3(0, dragHeight, 0)
        );

        if (boardPlane.Raycast(ray, out float distance))
        {
            transform.position = ray.GetPoint(distance);
        }
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;

        Ray ray = Camera.main.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, gridCellLayer))
        {
            GridCell targetCell = hit.collider.GetComponent<GridCell>();

            if (targetCell != null)
            {
                // Ô trống
                if (!targetCell.IsOccupied)
                {
                    unit.SetCell(targetCell);
                    return;
                }

                // Ô có Unit → thử Merge
                Unit targetUnit = targetCell.currentUnit;

                if (CanMerge(targetUnit))
                {
                    Merge(targetUnit);
                    return;
                }
            }
        }

        // Không hợp lệ → quay lại ô cũ
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

    private void Merge(Unit targetUnit)
    {
        GameManager gameManager = FindAnyObjectByType<GameManager>();

        Unit nextLevelPrefab = gameManager.GetNextLevelPrefab(unit);

        if (nextLevelPrefab == null)
        {
            Debug.Log("Already at max level!");

            unit.SetCell(originalCell);
            return;
        }

        GridCell mergeCell = targetUnit.currentCell;

        targetUnit.currentCell.RemoveUnit();
        unit.currentCell.RemoveUnit();

        Destroy(targetUnit.gameObject);
        Destroy(unit.gameObject);

        Unit newUnit = Instantiate(nextLevelPrefab);

        newUnit.SetCell(mergeCell);
    }
}