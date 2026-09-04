using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Cell")]
    public GridCell currentCell;
    public void SetCell(GridCell newCell)
    {
        // Không có cell mới thì thôi
        if (newCell == null)
            return;

        // Cell cũ không còn chứa Unit này
        if (currentCell != null)
        {
            currentCell.RemoveUnit();
        }

        // Cập nhật Cell mới
        currentCell = newCell;

        // Đánh dấu Unit đang chiếm Cell mới
        currentCell.SetUnit(this);

        // Snap Unit vào tâm Cell
        transform.position = newCell.transform.position;
    }
}