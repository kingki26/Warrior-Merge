using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Player Cells")]
    [SerializeField] private GridCell[] playerCells;

    [Header("Unit Prefabs")]
    [SerializeField] private Unit meleeLv1Prefab;
    [SerializeField] private Unit rangedLv1Prefab;

    [SerializeField] private Unit[] meleePrefabs;
    [SerializeField] private Unit[] rangedPrefabs;

    public void SpawnMelee()
    {
        Debug.Log("Spawn Melee button clicked!");

        SpawnUnit(meleeLv1Prefab);
    }

    public void SpawnRanged()
    {
        Debug.Log("Spawn Ranged button clicked!");

        SpawnUnit(rangedLv1Prefab);
    }

    private void SpawnUnit(Unit prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Unit prefab is NULL!");
            return;
        }

        foreach (GridCell cell in playerCells)
        {
            if (!cell.IsOccupied)
            {
                Debug.Log("Found empty cell: " + cell.name);

                Unit newUnit = Instantiate(prefab);
                newUnit.SetCell(cell);

                return;
            }
        }

        Debug.Log("Player grid is full!");
    }

    public Unit GetNextLevelPrefab(Unit unit)
    {
        int nextLevel = unit.level + 1;

        if (nextLevel > 5)
            return null;

        if (unit.unitType == UnitType.Melee)
        {
            return meleePrefabs[nextLevel - 1];
        }

        return rangedPrefabs[nextLevel - 1];
    }
}