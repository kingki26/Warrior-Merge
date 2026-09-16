using System.Collections.Generic;
using UnityEngine;

public class EnemySetup : MonoBehaviour
{
    [Header("Enemy Cells")]
    [SerializeField] private GridCell[] spawnCells;

    [Header("Enemy Prefabs")]
    [SerializeField] private Unit[] meleePrefabs;
    [SerializeField] private Unit[] rangedPrefabs;

    [Header("JSON")]
    [SerializeField]
    private string jsonFileName =
        "Levels/level_data_1_50_Melee_Ranged";

    [Header("References")]
    [SerializeField] private GameManager gameManager;

    private List<Unit> enemyUnits =
        new List<Unit>();

    private LevelDataList levelDatabase;

    private void Awake()
    {
        LoadLevelData();
    }

    private void LoadLevelData()
    {
        TextAsset jsonFile =
            Resources.Load<TextAsset>(jsonFileName);

        if (jsonFile == null)
        {
            Debug.LogError(
                "EnemySetup → Cannot find JSON: " +
                jsonFileName
            );

            return;
        }

        levelDatabase =
            JsonUtility.FromJson<LevelDataList>(
                jsonFile.text
            );

        if (levelDatabase == null ||
            levelDatabase.levels == null)
        {
            Debug.LogError(
                "EnemySetup → Failed to read JSON!"
            );

            return;
        }

        Debug.Log(
            "EnemySetup → JSON loaded! Levels: " +
            levelDatabase.levels.Count
        );
    }

    public void SetupLevel(int level)
    {
        Debug.Log(
            "EnemySetup → Setup Level " +
            level
        );

        ClearEnemies();

        if (levelDatabase == null)
        {
            Debug.LogError(
                "EnemySetup → Level database is NULL!"
            );

            return;
        }

        LevelData levelData =
            FindLevelData(level);

        if (levelData == null)
        {
            Debug.LogError(
                "EnemySetup → No data for Level " +
                level
            );

            return;
        }

        foreach (EnemyData enemyData in levelData.enemies)
        {
            SpawnEnemy(enemyData);
        }
    }

    private LevelData FindLevelData(int level)
    {
        foreach (LevelData data in levelDatabase.levels)
        {
            if (data.level == level)
            {
                return data;
            }
        }

        return null;
    }

    private void SpawnEnemy(EnemyData enemyData)
    {
        UnitType unitType;

        if (enemyData.type == "Melee")
        {
            unitType = UnitType.Melee;
        }
        else if (enemyData.type == "Ranged")
        {
            unitType = UnitType.Ranged;
        }
        else
        {
            Debug.LogError(
                "EnemySetup → Unknown enemy type: " +
                enemyData.type
            );

            return;
        }

        Unit prefab =
            GetEnemyPrefab(
                unitType,
                enemyData.level
            );

        if (prefab == null)
        {
            Debug.LogError(
                "EnemySetup → Prefab not found → " +
                enemyData.type +
                " Lv" +
                enemyData.level
            );

            return;
        }

        // ========================================
        // CÓ VỊ TRÍ CELL
        // ========================================

        if (!string.IsNullOrEmpty(enemyData.cell))
        {
            GridCell cell =
                FindCellByName(enemyData.cell);

            if (cell == null)
            {
                Debug.LogError(
                    "EnemySetup → Cell not found: " +
                    enemyData.cell
                );

                return;
            }

            if (cell.IsOccupied)
            {
                Debug.LogError(
                    "EnemySetup → Cell already occupied: " +
                    enemyData.cell
                );

                return;
            }

            SpawnEnemyAtCell(
                prefab,
                cell,
                enemyData.level
            );

            return;
        }

        // ========================================
        // KHÔNG CÓ CELL
        // ========================================

        Debug.LogWarning(
            "EnemySetup → Enemy has no cell → " +
            enemyData.type +
            " Lv" +
            enemyData.level
        );
    }

    private void SpawnEnemyAtCell(
        Unit prefab,
        GridCell cell,
        int level
    )
    {
        Unit enemy =
            Instantiate(prefab);

        enemy.level = level;

        enemy.SetCell(cell);

        enemyUnits.Add(enemy);

        Debug.Log(
            "Spawn Enemy → " +
            enemy.name +
            " | " +
            enemy.unitType +
            " Lv" +
            level +
            " | Cell " +
            cell.name
        );
    }

    private GridCell FindCellByName(
        string cellName
    )
    {
        foreach (GridCell cell in spawnCells)
        {
            if (cell == null)
                continue;

            if (cell.name == cellName)
            {
                return cell;
            }
        }

        return null;
    }

    private Unit GetEnemyPrefab(
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

    private void ClearEnemies()
    {
        foreach (Unit enemy in enemyUnits)
        {
            if (enemy == null)
                continue;

            if (enemy.currentCell != null)
            {
                enemy.currentCell.RemoveUnit();
            }

            Destroy(enemy.gameObject);
        }

        enemyUnits.Clear();

        Debug.Log(
            "EnemySetup → Old enemies cleared."
        );
    }

    public void StartFight()
    {
        foreach (Unit enemy in enemyUnits)
        {
            if (enemy == null)
                continue;

            UnitCombat meleeCombat =
                enemy.GetComponent<UnitCombat>();

            if (meleeCombat != null)
            {
                meleeCombat.StartCombat();
            }

            RangedCombat rangedCombat =
                enemy.GetComponent<RangedCombat>();

            if (rangedCombat != null)
            {
                rangedCombat.StartCombat();
            }
        }

        gameManager.StartPlayerCombat();
    }
}