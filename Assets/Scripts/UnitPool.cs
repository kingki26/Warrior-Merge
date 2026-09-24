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

    private Dictionary<string, Queue<Unit>> playerPools = new Dictionary<string, Queue<Unit>>();

    private Dictionary<string, Queue<Unit>> enemyPools = new Dictionary<string, Queue<Unit>>();

    private void Awake()
    {
        CreatePlayerPools();
        CreateEnemyPools();
    }

    private void CreatePlayerPools()
    {
        CreatePool( playerPools, UnitType.Melee,playerMeleePrefabs);

        CreatePool( playerPools, UnitType.Ranged, playerRangedPrefabs);
    }
    private void CreateEnemyPools()
    {
        CreatePool( enemyPools, UnitType.Melee,enemyMeleePrefabs);

        CreatePool(enemyPools,UnitType.Ranged,enemyRangedPrefabs);
    }

    private void CreatePool(Dictionary<string, Queue<Unit>> targetPools,UnitType unitType, Unit[] prefabs)
    {
        if (prefabs == null)
            return;

        for (int i = 0; i < prefabs.Length; i++)
        {
            Unit prefab = prefabs[i];

            if (prefab == null)
                continue;

            int level = i + 1;

            string key = GetPoolKey( unitType,level);

            Queue<Unit> unitQueue = new Queue<Unit>();

            for (int j = 0;j < initialSizePerLevel; j++)
            {
                Unit unit = Instantiate( prefab,transform);

                unit.gameObject.SetActive(false);

                unitQueue.Enqueue(unit);
            }

            targetPools[key] = unitQueue;
        }
    }
    public Unit GetPlayerUnit(UnitType unitType, int level)
    {
        return GetUnitFromPool(playerPools, unitType, level,"Player");
    }
    public Unit GetEnemyUnit(UnitType unitType,int level)
    {
        return GetUnitFromPool(enemyPools,unitType, level,"Enemy");
    }
    private Unit GetUnitFromPool(Dictionary<string, Queue<Unit>> targetPools, UnitType unitType,int level, string owner)
    {
        string key = GetPoolKey(unitType,level);

        if (!targetPools.ContainsKey(key))
        {
            return null;
        }

        Queue<Unit> pool = targetPools[key];

        if (pool.Count > 0)
        {
            Unit unit = pool.Dequeue();

            if (unit == null)
            {
                return GetUnitFromPool( targetPools,unitType, level, owner);
            }

            unit.ResetForReuse();

            unit.gameObject.SetActive(true);

            return unit;
        }

        Unit prefab = GetPrefab(owner, unitType,level);

        if (prefab == null)
        {
            return null;
        }

        Unit newUnit = Instantiate(prefab,transform);

        newUnit.ResetForReuse();

        newUnit.gameObject.SetActive(true);

        return newUnit;
    }
    public void ReturnPlayerUnit(Unit unit)
    {
        ReturnUnitToPool(playerPools, unit,"Player");
    }
    public void ReturnEnemyUnit(Unit unit)
    {
        ReturnUnitToPool(enemyPools,unit, "Enemy");
    }
    private void ReturnUnitToPool( Dictionary<string, Queue<Unit>> targetPools,Unit unit,string owner)
    {
        if (unit == null)
            return;

        string key = GetPoolKey(unit.unitType, unit.level);

        if (!targetPools.ContainsKey(key))
        {
            unit.gameObject.SetActive(false);
            return;
        }
        if (unit.currentCell != null)
        {
            unit.currentCell.RemoveUnit();
            unit.currentCell = null;
        }

        unit.ResetForReuse();

        unit.transform.SetParent(transform);

        unit.gameObject.SetActive(false);

        if (targetPools[key].Contains(unit))
        {
            return;
        }

        targetPools[key].Enqueue(unit);

    }
    private string GetPoolKey( UnitType unitType,int level)
    {
        return unitType.ToString() +  "_Lv" + level;
    }


    private Unit GetPrefab(string owner,UnitType unitType,int level)
    {
        int index = level - 1;

        if (index < 0)
            return null;

        if (owner == "Player")
        {
            if (unitType == UnitType.Melee)
            {
                if (playerMeleePrefabs == null || index >= playerMeleePrefabs.Length)
                {
                    return null;
                }

                return playerMeleePrefabs[index];
            }

            if (playerRangedPrefabs == null || index >= playerRangedPrefabs.Length)
            {
                return null;
            }

            return playerRangedPrefabs[index];
        }

        if (owner == "Enemy")
        {
            if (unitType == UnitType.Melee)
            {
                if (enemyMeleePrefabs == null ||index >= enemyMeleePrefabs.Length)
                {
                    return null;
                }

                return enemyMeleePrefabs[index];
            }

            if (enemyRangedPrefabs == null ||index >= enemyRangedPrefabs.Length)
            {
                return null;
            }

            return enemyRangedPrefabs[index];
        }

        return null;
    }
}