using System.Collections.Generic;
using UnityEngine;

public class EnemySetup : MonoBehaviour
{
    [Header("Enemy Cells")]
    [SerializeField] private GridCell[] spawnCells;

    [Header("JSON")]
    [SerializeField]
    private string jsonFileName = "Levels/level_data_1_50_Melee_Ranged";

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UnitPool unitPool;

    [Header("Boss Prefabs")]
    [SerializeField] private BossUnit[] bossPrefabs;

    private List<Unit> enemyUnits = new List<Unit>();

    private List<BossUnit> bossUnits = new List<BossUnit>();

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
            return;
        }

        levelDatabase = JsonUtility.FromJson<LevelDataList> (jsonFile.text);

        if (levelDatabase == null ||
            levelDatabase.levels == null)
        {
            return;
        }
    }

    public void SetupLevel(int level)
    {

        ClearEnemies();

        if (levelDatabase == null)
        {
            return;
        }

        LevelData levelData = FindLevelData(level);

        if (levelData == null)
        {
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
                return data;
        }

        return null;
    }

    private void SpawnEnemy(EnemyData enemyData)
    {

        if (enemyData.type == "Boss")
        {
            SpawnBoss(enemyData);
            return;
        }

        UnitType unitType;

        if (enemyData.type == "Melee")
            unitType = UnitType.Melee;
        else if (enemyData.type == "Ranged")
            unitType = UnitType.Ranged;
        else
        {
            return;
        }

        if (unitPool == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(enemyData.cell))
        {
            GridCell cell =
                FindCellByName(enemyData.cell);

            if (cell == null)
            {
                return;
            }

            if (cell.IsOccupied)
            {
                return;
            }

            SpawnEnemyAtCell(
                unitType,
                cell,
                enemyData.level
            );

            return;
        }
    }

    private void SpawnEnemyAtCell(
        UnitType unitType,
        GridCell cell,
        int level)
    {
        Unit enemy =
            unitPool.GetEnemyUnit(
                unitType,
                level
            );

        if (enemy == null)
        {
            return;
        }

        enemy.level = level;
        enemy.SetCell(cell);
        enemyUnits.Add(enemy);
    }

    private void SpawnBoss(EnemyData enemyData)
    {
        if (string.IsNullOrEmpty(enemyData.bossLevel))
        {
            return;
        }

        GridCell cell = FindCellByName(enemyData.cell);

        if (cell == null)
        {
            return;
        }

        if (cell.IsOccupied)
        {
            return;
        }

        BossUnit boss =  FindBossPrefab(enemyData.bossLevel);

        if (boss == null)
        {
            return;
        }

        BossUnit newBoss = Instantiate(boss);

        newBoss.bossLevel = enemyData.bossLevel;

        newBoss.SetCell(cell);

        bossUnits.Add(newBoss);
    }

    private BossUnit FindBossPrefab(string bossLevel)
    {
        if (bossPrefabs == null)
            return null;

        foreach (BossUnit boss in bossPrefabs)
        {
            if (boss == null)
                continue;

            if (boss.bossLevel == bossLevel)
                return boss;
        }

        return null;
    }

    private GridCell FindCellByName(string cellName)
    {
        foreach (GridCell cell in spawnCells)
        {
            if (cell == null)
                continue;

            if (cell.name == cellName)
                return cell;
        }

        return null;
    }

    private void ClearEnemies()
    {
        foreach (Unit enemy in enemyUnits)
        {
            if (enemy == null)
                continue;

            if (unitPool != null)
                unitPool.ReturnEnemyUnit(enemy);
            else
                Debug.LogWarning(
                    "EnemySetup → UnitPool is NULL. Cannot return Enemy."
                );
        }

        enemyUnits.Clear();

        foreach (BossUnit boss in bossUnits)
        {
            if (boss == null)
                continue;

            if (boss.currentCell != null)
            {
                boss.currentCell.RemoveUnit();
                boss.currentCell = null;
            }

            Destroy(boss.gameObject);
        }

        bossUnits.Clear();
    }

    public void StartFight()
    {
        foreach (Unit enemy in enemyUnits)
        {
            if (enemy == null)
                continue;

            UnitCombat meleeCombat = enemy.GetComponent<UnitCombat>();

            if (meleeCombat != null)
            {
                meleeCombat.StartCombat();
            }

            RangedCombat rangedCombat = enemy.GetComponent<RangedCombat>();

            if (rangedCombat != null)
            {
                rangedCombat.StartCombat();
            }
        }

        foreach (BossUnit boss in bossUnits)
        {
            if (boss == null)
                continue;

            UnitCombat meleeCombat = boss.GetComponent<UnitCombat>();

            if (meleeCombat != null)
            {
                meleeCombat.StartCombat();
            }

            RangedCombat rangedCombat = boss.GetComponent<RangedCombat>();

            if (rangedCombat != null)
            {
                rangedCombat.StartCombat();
            }
        }

        if (gameManager != null)
        {
            gameManager.StartPlayerCombat();
        }

        else
            Debug.LogError(
                "EnemySetup → GameManager is NULL!"
            );
    }

    public int GetTotalEnemyMaxHealth()
    {
        int totalHealth = 0;

        // Normal enemies
        foreach (Unit enemy in enemyUnits)
        {
            if (enemy == null)
                continue;

            UnitHealth health =  enemy.GetComponent<UnitHealth>();

            if (health == null)
                continue;

            totalHealth += health.GetMaxHealth();
        }

        // Bosses
        foreach (BossUnit boss in bossUnits)
        {
            if (boss == null)
                continue;

            UnitHealth health = boss.GetComponent<UnitHealth>();

            if (health == null)
                continue;

            totalHealth += health.GetMaxHealth();
        }

        return totalHealth;
    }
}