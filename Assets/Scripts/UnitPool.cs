using System.Collections.Generic;
using UnityEngine;

public class UnitPool : MonoBehaviour
{
    [Header("Melee Prefabs")]
    [SerializeField] private Unit[] meleePrefabs;

    [Header("Ranged Prefabs")]
    [SerializeField] private Unit[] rangedPrefabs;

    [Header("Pool Settings")]
    [SerializeField] private int initialSizePerLevel = 15;

    private Dictionary<string, Queue<Unit>> pools =
        new Dictionary<string, Queue<Unit>>();

    private void Awake()
    {
        CreatePools();
    }

    private void CreatePools()
    {
        CreatePool(UnitType.Melee, meleePrefabs);
        CreatePool(UnitType.Ranged, rangedPrefabs);
    }

    private void CreatePool(
        UnitType unitType,
        Unit[] prefabs
    )
    {
        if (prefabs == null)
            return;

        for (int i = 0; i < prefabs.Length; i++)
        {
            Unit prefab = prefabs[i];

            if (prefab == null)
            {
                Debug.LogWarning(
                    "UnitPool → Missing prefab at index: " +
                    i +
                    " | Type: " +
                    unitType
                );

                continue;
            }

            int level = i + 1;

            string key =
                GetPoolKey(unitType, level);

            Queue<Unit> unitQueue =
                new Queue<Unit>();

            for (int j = 0; j < initialSizePerLevel; j++)
            {
                Unit unit =
                    Instantiate(
                        prefab,
                        transform
                    );

                unit.gameObject.SetActive(false);

                unitQueue.Enqueue(unit);
            }

            pools.Add(
                key,
                unitQueue
            );

            Debug.Log(
                "UnitPool → Created " +
                key +
                " | Size: " +
                initialSizePerLevel
            );
        }
    }

    // ========================================
    // GET UNIT
    // ========================================

    public Unit GetUnit(
        UnitType unitType,
        int level
    )
    {
        string key =
            GetPoolKey(unitType, level);

        if (!pools.ContainsKey(key))
        {
            Debug.LogWarning(
                "UnitPool → Pool not found: " +
                key
            );

            return null;
        }

        Queue<Unit> pool =
            pools[key];

        if (pool.Count == 0)
        {
            Debug.Log(
                "UnitPool → Pool empty → " +
                key +
                " → Creating new Unit."
            );

            Unit prefab =
                GetPrefab(
                    unitType,
                    level
                );

            if (prefab == null)
            {
                Debug.LogError(
                    "UnitPool → Cannot create Unit. " +
                    "Prefab not found: " +
                    key
                );

                return null;
            }

            Unit newUnit =
                Instantiate(
                    prefab,
                    transform
                );

            newUnit.ResetForReuse();

            newUnit.gameObject.SetActive(true);

            return newUnit;
        }

        Unit unit =
    pool.Dequeue();

        unit.gameObject.SetActive(true);
        unit.ResetForReuse();

        return unit;
    }

    // ========================================
    // RETURN UNIT
    // ========================================

    public void ReturnUnit(Unit unit)
    {
        if (unit == null)
            return;

        string key =
            GetPoolKey(
                unit.unitType,
                unit.level
            );

        if (!pools.ContainsKey(key))
        {
            Debug.LogWarning(
                "UnitPool → Cannot return Unit. " +
                "Pool not found: " +
                key
            );

            unit.gameObject.SetActive(false);

            return;
        }

        if (unit.currentCell != null)
        {
            unit.currentCell.RemoveUnit();
            unit.currentCell = null;
        }

        unit.transform.SetParent(transform);

        unit.gameObject.SetActive(false);

        pools[key].Enqueue(unit);

        Debug.Log(
            "UnitPool → Returned " +
            unit.name +
            " → " +
            key
        );
    }

    // ========================================
    // HELPERS
    // ========================================

    private string GetPoolKey(
        UnitType unitType,
        int level
    )
    {
        return unitType.ToString() +
               "_Lv" +
               level;
    }

    private Unit GetPrefab(
        UnitType unitType,
        int level
    )
    {
        int index = level - 1;

        if (index < 0)
            return null;

        if (unitType == UnitType.Melee)
        {
            if (index >= meleePrefabs.Length)
                return null;

            return meleePrefabs[index];
        }

        if (index >= rangedPrefabs.Length)
            return null;

        return rangedPrefabs[index];
    }
}