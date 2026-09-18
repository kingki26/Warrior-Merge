using System.Collections.Generic;
using UnityEngine;

public class UnitPool : MonoBehaviour
{
    [Header("Player - Melee Prefabs")]
    [SerializeField] private Unit[] playerMeleePrefabs;

    [Header("Player - Ranged Prefabs")]
    [SerializeField] private Unit[] playerRangedPrefabs;

    [Header("Enemy - Melee Prefabs")]
    [SerializeField] private Unit[] enemyMeleePrefabs;

    [Header("Enemy - Ranged Prefabs")]
    [SerializeField] private Unit[] enemyRangedPrefabs;

    [Header("Pool Settings")]
    [SerializeField] private int initialSizePerLevel = 15;

    private Dictionary<string, Queue<Unit>> playerPools =
        new Dictionary<string, Queue<Unit>>();

    private Dictionary<string, Queue<Unit>> enemyPools =
        new Dictionary<string, Queue<Unit>>();

    private void Awake()
    {
        CreatePlayerPools();
        CreateEnemyPools();
    }

    // =========================================================
    // CREATE PLAYER POOLS
    // =========================================================

    private void CreatePlayerPools()
    {
        CreatePool(
            playerPools,
            UnitType.Melee,
            playerMeleePrefabs
        );

        CreatePool(
            playerPools,
            UnitType.Ranged,
            playerRangedPrefabs
        );
    }

    // =========================================================
    // CREATE ENEMY POOLS
    // =========================================================

    private void CreateEnemyPools()
    {
        CreatePool(
            enemyPools,
            UnitType.Melee,
            enemyMeleePrefabs
        );

        CreatePool(
            enemyPools,
            UnitType.Ranged,
            enemyRangedPrefabs
        );
    }

    // =========================================================
    // CREATE POOL
    // =========================================================

    private void CreatePool(
        Dictionary<string, Queue<Unit>> targetPools,
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
                continue;

            int level = i + 1;

            string key =
                GetPoolKey(
                    unitType,
                    level
                );

            Queue<Unit> unitQueue =
                new Queue<Unit>();

            for (
                int j = 0;
                j < initialSizePerLevel;
                j++
            )
            {
                Unit unit =
                    Instantiate(
                        prefab,
                        transform
                    );

                unit.gameObject.SetActive(false);

                unitQueue.Enqueue(unit);
            }

            targetPools[key] = unitQueue;
        }
    }

    // =========================================================
    // GET PLAYER UNIT
    // =========================================================

    public Unit GetPlayerUnit(
        UnitType unitType,
        int level
    )
    {
        return GetUnitFromPool(
            playerPools,
            unitType,
            level,
            "Player"
        );
    }

    // =========================================================
    // GET ENEMY UNIT
    // =========================================================

    public Unit GetEnemyUnit(
        UnitType unitType,
        int level
    )
    {
        return GetUnitFromPool(
            enemyPools,
            unitType,
            level,
            "Enemy"
        );
    }

    // =========================================================
    // GET UNIT FROM POOL
    // =========================================================

    private Unit GetUnitFromPool(
        Dictionary<string, Queue<Unit>> targetPools,
        UnitType unitType,
        int level,
        string owner
    )
    {
        string key =
            GetPoolKey(
                unitType,
                level
            );

        if (!targetPools.ContainsKey(key))
        {
            Debug.LogWarning(
                "UnitPool → " +
                owner +
                " pool not found: " +
                key
            );

            return null;
        }

        Queue<Unit> pool =
            targetPools[key];

        // -----------------------------------------------------
        // POOL HAS UNIT
        // -----------------------------------------------------

        if (pool.Count > 0)
        {
            Unit unit =
                pool.Dequeue();

            if (unit == null)
            {
                Debug.LogWarning(
                    "UnitPool → " +
                    owner +
                    " pool contained NULL Unit → " +
                    key
                );

                return GetUnitFromPool(
                    targetPools,
                    unitType,
                    level,
                    owner
                );
            }

            unit.ResetForReuse();

            unit.gameObject.SetActive(true);

            return unit;
        }

        // -----------------------------------------------------
        // POOL EMPTY → CREATE NEW
        // -----------------------------------------------------

        Debug.Log(
            "UnitPool → " +
            owner +
            " pool empty → " +
            key +
            " → Creating new Unit."
        );

        Unit prefab =
            GetPrefab(
                owner,
                unitType,
                level
            );

        if (prefab == null)
        {
            Debug.LogError(
                "UnitPool → Cannot create " +
                owner +
                " Unit → " +
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

    // =========================================================
    // RETURN PLAYER UNIT
    // =========================================================

    public void ReturnPlayerUnit(Unit unit)
    {
        ReturnUnitToPool(
            playerPools,
            unit,
            "Player"
        );
    }

    // =========================================================
    // RETURN ENEMY UNIT
    // =========================================================

    public void ReturnEnemyUnit(Unit unit)
    {
        ReturnUnitToPool(
            enemyPools,
            unit,
            "Enemy"
        );
    }

    // =========================================================
    // RETURN UNIT TO POOL
    // =========================================================

    private void ReturnUnitToPool(
        Dictionary<string, Queue<Unit>> targetPools,
        Unit unit,
        string owner
    )
    {
        if (unit == null)
            return;

        string key =
            GetPoolKey(
                unit.unitType,
                unit.level
            );

        if (!targetPools.ContainsKey(key))
        {
            Debug.LogWarning(
                "UnitPool → Cannot return " +
                owner +
                " Unit. Pool not found: " +
                key
            );

            unit.gameObject.SetActive(false);

            return;
        }

        // -----------------------------------------------------
        // REMOVE FROM CURRENT CELL
        // -----------------------------------------------------

        if (unit.currentCell != null)
        {
            unit.currentCell.RemoveUnit();
            unit.currentCell = null;
        }

        // -----------------------------------------------------
        // RESET UNIT
        // -----------------------------------------------------

        unit.ResetForReuse();

        unit.transform.SetParent(transform);

        unit.gameObject.SetActive(false);

        // -----------------------------------------------------
        // IMPORTANT:
        // PREVENT DUPLICATE RETURN
        // -----------------------------------------------------

        if (targetPools[key].Contains(unit))
        {
            Debug.LogWarning(
                "UnitPool → DUPLICATE RETURN BLOCKED → " +
                owner +
                " " +
                unit.name +
                " → " +
                key
            );

            return;
        }

        // -----------------------------------------------------
        // RETURN TO QUEUE
        // -----------------------------------------------------

        targetPools[key].Enqueue(unit);

        Debug.Log(
            "UnitPool → Returned " +
            owner +
            " " +
            unit.name +
            " → " +
            key
        );
    }

    // =========================================================
    // POOL KEY
    // =========================================================

    private string GetPoolKey(
        UnitType unitType,
        int level
    )
    {
        return unitType.ToString() +
               "_Lv" +
               level;
    }

    // =========================================================
    // GET PREFAB
    // =========================================================

    private Unit GetPrefab(
        string owner,
        UnitType unitType,
        int level
    )
    {
        int index = level - 1;

        if (index < 0)
            return null;

        // -----------------------------------------------------
        // PLAYER
        // -----------------------------------------------------

        if (owner == "Player")
        {
            if (unitType == UnitType.Melee)
            {
                if (
                    playerMeleePrefabs == null ||
                    index >= playerMeleePrefabs.Length
                )
                {
                    return null;
                }

                return playerMeleePrefabs[index];
            }

            if (
                playerRangedPrefabs == null ||
                index >= playerRangedPrefabs.Length
            )
            {
                return null;
            }

            return playerRangedPrefabs[index];
        }

        // -----------------------------------------------------
        // ENEMY
        // -----------------------------------------------------

        if (owner == "Enemy")
        {
            if (unitType == UnitType.Melee)
            {
                if (
                    enemyMeleePrefabs == null ||
                    index >= enemyMeleePrefabs.Length
                )
                {
                    return null;
                }

                return enemyMeleePrefabs[index];
            }

            if (
                enemyRangedPrefabs == null ||
                index >= enemyRangedPrefabs.Length
            )
            {
                return null;
            }

            return enemyRangedPrefabs[index];
        }

        return null;
    }
}