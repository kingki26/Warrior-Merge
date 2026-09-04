using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public Unit unitPrefab;
    public GridCell spawnCell;

    private void Start()
    {
        SpawnUnit();
    }

    public void SpawnUnit()
    {
        Unit newUnit = Instantiate(unitPrefab);

        newUnit.SetCell(spawnCell);
    }
}