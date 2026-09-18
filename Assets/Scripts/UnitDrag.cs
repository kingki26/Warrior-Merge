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

        Debug.Log(
        "DRAG CHECK → " +
        unit.name +
        " | Cell: " +
        unit.currentCell.name +
        " | Melee Fighting: " +
        (unitCombat != null && unitCombat.IsFighting()) +
        " | Ranged Fighting: " +
        (rangedCombat != null && rangedCombat.IsFighting())
        );

        if (unit.currentCell == null)
            return;

        // ========================================
        // ĐANG COMBAT → KHÔNG CHO KÉO
        // ========================================

        if (unitCombat != null &&
            unitCombat.IsFighting())
        {
            return;
        }

        if (rangedCombat != null &&
            rangedCombat.IsFighting())
        {
            return;
        }

        isDragging = true;

        originalCell =
            unit.currentCell;

        dragHeight =
            transform.position.y;
    }

    private void OnMouseDrag()
    {
        if (!isDragging)
            return;

        Ray ray =
            Camera.main.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        Plane boardPlane =
            new Plane(
                Vector3.up,
                new Vector3(
                    0,
                    dragHeight,
                    0
                )
            );

        if (boardPlane.Raycast(
            ray,
            out float distance
        ))
        {
            transform.position =
                ray.GetPoint(distance);
        }
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        isDragging = false;

        Ray ray =
            Camera.main.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            100f,
            gridCellLayer
        ))
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

    // ========================================
    // CAN MERGE
    // ========================================

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

    // ========================================
    // MERGE
    // ========================================

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
        // LƯU THÔNG TIN
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
        // KIỂM TRA LEVEL
        // ========================================

        if (nextLevel > 5)
        {
            Debug.Log(
                "UnitDrag → Cannot merge beyond Lv5."
            );

            unit.SetCell(originalCell);
            return;
        }

        // ========================================
        // LẤY UNIT LEVEL MỚI TRƯỚC
        // ========================================
        // Quan trọng:
        // Không return 2 unit cũ trước khi biết
        // Unit level mới có tồn tại trong Pool.

        Unit newUnit =
            unitPool.GetPlayerUnit(
                unitType,
                nextLevel
            );

        if (newUnit == null)
        {
            Debug.LogError(
                "UnitDrag → Cannot get next level Player Unit from Pool! " +
                unitType +
                " Lv" +
                nextLevel
            );

            unit.SetCell(originalCell);
            return;
        }

        // ========================================
        // TRẢ 2 UNIT CŨ VỀ PLAYER POOL
        // ========================================

        unitPool.ReturnPlayerUnit(
            targetUnit
        );

        unitPool.ReturnPlayerUnit(
            unit
        );

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
        // ĐẶT UNIT MỚI VÀO Ô MERGE
        // ========================================

        newUnit.SetCell(
            mergeCell
        );

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