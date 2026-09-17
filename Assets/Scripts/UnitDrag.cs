using UnityEngine;
using UnityEngine.InputSystem;

public class UnitDrag : MonoBehaviour
{
    [SerializeField] private LayerMask gridCellLayer;

    private Unit unit;
    private UnitCombat unitCombat;
    private GridCell originalCell;

    private bool isDragging;
    private float dragHeight;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        unitCombat = GetComponent<UnitCombat>();
    }

    private void OnMouseDown()
    {
        if (unit.currentCell == null)
            return;

        // Đang combat thì không cho kéo
        if (unitCombat != null && unitCombat.IsFighting())
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

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            100f,
            gridCellLayer))
        {
            GridCell targetCell =
                hit.collider.GetComponent<GridCell>();

            if (targetCell != null)
            {
                // ========================================
                // Ô TRỐNG
                // ========================================

                if (!targetCell.IsOccupied)
                {
                    unit.SetCell(targetCell);
                    return;
                }

                // ========================================
                // Ô CÓ UNIT → THỬ MERGE
                // ========================================

                Unit targetUnit =
                    targetCell.currentUnit;

                if (CanMerge(targetUnit))
                {
                    Merge(targetUnit);
                    return;
                }
            }
        }

        // ========================================
        // KHÔNG HỢP LỆ → QUAY VỀ Ô CŨ
        // ========================================

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
        UnitPool unitPool =
        FindAnyObjectByType<UnitPool>();

        if (unitPool == null)
        {
            Debug.LogError(
                "UnitDrag → UnitPool not found!"
            );

            unit.SetCell(originalCell);
            return;
        }

        // ========================================
        // LƯU THÔNG TIN TRƯỚC KHI RETURN
        // ========================================

        UnitType unitType =
            unit.unitType;

        int currentLevel =
            unit.level;

        int nextLevel =
            currentLevel + 1;

        GridCell mergeCell =
            targetUnit.currentCell;

        if (mergeCell == null)
        {
            Debug.LogError(
                "UnitDrag → Merge cell is NULL!"
            );

            unit.SetCell(originalCell);
            return;
        }

        // ========================================
        // TRẢ 2 UNIT CŨ VỀ POOL
        // ========================================

        unitPool.ReturnUnit(targetUnit);
        unitPool.ReturnUnit(unit);

        Debug.Log(
            "MERGE → " +
            unitType +
            " Lv" +
            currentLevel +
            " + " +
            unitType +
            " Lv" +
            currentLevel
        );

        // ========================================
        // LẤY UNIT LEVEL MỚI
        // ========================================

        Unit newUnit =
            unitPool.GetUnit(
                unitType,
                nextLevel
            );

        if (newUnit == null)
        {
            Debug.LogError(
                "UnitDrag → Cannot get next level Unit from Pool! " +
                unitType +
                " Lv" +
                nextLevel
            );

            return;
        }

        // ========================================
        // ĐẶT VÀO Ô MERGE
        // ========================================

        newUnit.SetCell(mergeCell);

        Debug.Log(
            "MERGE SUCCESS → " +
            newUnit.unitType +
            " Lv" +
            newUnit.level +
            " | Cell " +
            mergeCell.name
        );
    }
}