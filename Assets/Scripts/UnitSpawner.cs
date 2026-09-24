using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Enemy Cells")]
    [SerializeField] private GridCell[] enemyCells;

    [Header("Player Cells")]
    [SerializeField] private GridCell[] playerCells;

    [Header("Unit Prefabs")]
    [SerializeField] private Unit meleeLv1Prefab;
    [SerializeField] private Unit rangedLv1Prefab;

    [SerializeField] private Unit[] meleePrefabs;
    [SerializeField] private Unit[] rangedPrefabs;

    [Header("References")]
    [SerializeField] private EnemySetup enemySetup;
    [SerializeField] private VictoryUI victoryUI;
    [SerializeField] private DefeatUI defeatUI;
    [SerializeField] private UnitPool unitPool;
    [SerializeField] private GoldManager goldManager;

    [System.Serializable]
    private class PlayerUnitSnapshot
    {
        public UnitType unitType;
        public int level;
        public string cellName;
    }

    private List<PlayerUnitSnapshot> playerSnapshot = new List<PlayerUnitSnapshot>();

    private bool isFighting;
    private bool battleEnded;

    private void SavePlayerSnapshot()
    {
        playerSnapshot.Clear();

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (!cell.IsOccupied)
                continue;

            Unit unit = cell.currentUnit;

            if (unit == null)
                continue;

            PlayerUnitSnapshot snapshot = new PlayerUnitSnapshot();

            snapshot.unitType = unit.unitType;
            snapshot.level = unit.level;
            snapshot.cellName = cell.name;

            playerSnapshot.Add(snapshot);
        }
    }

    public void RestorePlayerSnapshot()
    {
        if (unitPool == null)
        {
            return;
        }

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            Unit unit = cell.currentUnit;

            if (unit == null)
                continue;

            unitPool.ReturnPlayerUnit(unit);
        }

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (cell.IsOccupied)
            {
                Debug.LogError(
                    "Restore → Cell still occupied after clear: " +
                    cell.name
                );
            }
        }

        foreach (PlayerUnitSnapshot snapshot in playerSnapshot)
        {
            GridCell targetCell = null;

            foreach (GridCell cell in playerCells)
            {
                if (cell == null)
                    continue;

                if (cell.name == snapshot.cellName)
                {
                    targetCell = cell;
                    break;
                }
            }

            if (targetCell == null)
            {
                continue;
            }

            if (targetCell.IsOccupied)
            {
                continue;
            }

            Unit unit = unitPool.GetPlayerUnit(snapshot.unitType, snapshot.level);

            if (unit == null)
            {
                continue;
            }

            unit.SetCell(targetCell);

            InitializeCombat(unit);
        }

        isFighting = false;
        battleEnded = false;
    }

    public void SpawnMelee()
    {
        if (isFighting)
            return;

        TryBuyAndSpawn(meleeLv1Prefab,UnitType.Melee);
    }

    public void SpawnRanged()
    {
        if (isFighting)
            return;

        TryBuyAndSpawn(rangedLv1Prefab, UnitType.Ranged);
    }

    public void StartFight()
    {
        if (isFighting)
            return;

        bool hasPlayerUnit = false;

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (cell.IsOccupied)
            {
                hasPlayerUnit = true;
                break;
            }
        }

        if (!hasPlayerUnit)
        {
            return;
        }

        SavePlayerSnapshot();

        isFighting = true;
        battleEnded = false;

        if (enemySetup != null)
        {
            enemySetup.StartFight();
        }
        else
        {
            Debug.LogError(
                "GameManager → EnemySetup is NULL!"
            );
        }
    }

    public GridCell[] GetEnemyCells()
    {
        return enemyCells;
    }

    private void TryBuyAndSpawn(
        Unit prefab,
        UnitType unitType)
    {
        if (prefab == null)
        {
            return;
        }

        if (unitPool == null)
        {
            return;
        }

        if (goldManager == null)
        {
            return;
        }

        GridCell emptyCell = null;

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (!cell.IsOccupied)
            {
                emptyCell = cell;
                break;
            }
        }

        if (emptyCell == null)
        {
            return;
        }

        int price;

        if (unitType == UnitType.Melee)
        {
            price = goldManager.GetMeleePrice();
        }
        else
        {
            price = goldManager.GetRangedPrice();
        }

        if (!goldManager.CanAfford(price))
        {
            return;
        }

        Unit newUnit = unitPool.GetPlayerUnit(prefab.unitType,prefab.level);

        if (newUnit == null)
        {
            return;
        }

        bool purchaseSuccess;

        if (unitType == UnitType.Melee)
        {
            purchaseSuccess = goldManager.BuyMelee();
        }
        else
        {
            purchaseSuccess = goldManager.BuyRanged();
        }

        if (!purchaseSuccess)
        {
            unitPool.ReturnPlayerUnit(newUnit);
            return;
        }

        newUnit.SetCell(emptyCell);

        InitializeCombat(newUnit);
    }

    private void InitializeCombat(Unit unit)
    {
        if (unit == null)
            return;

        if (unit.Combat != null)
            unit.Combat.Init(unit, this);

        if (unit.RangedCombat != null)
            unit.RangedCombat.Init(unit, this);
    }

    public Unit GetNextLevelPrefab(Unit unit)
    {
        if (unit == null)
            return null;

        int nextLevel = unit.level + 1;

        if (nextLevel > 5)
            return null;

        if (unit.unitType == UnitType.Melee)
        {
            if (meleePrefabs == null || nextLevel - 1 >= meleePrefabs.Length)
            {
                return null;
            }

            return meleePrefabs[nextLevel - 1];
        }

        if (rangedPrefabs == null || nextLevel - 1 >= rangedPrefabs.Length)
        {
            return null;
        }

        return rangedPrefabs[nextLevel - 1];
    }

    public GridCell[] GetPlayerCells()
    {
        return playerCells;
    }

    public void StartPlayerCombat()
    {
        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (!cell.IsOccupied)
                continue;

            Unit unit = cell.currentUnit;

            if (unit == null)
                continue;

            InitializeCombat(unit);

            if (unit.Combat != null)
            {
                unit.Combat.StartCombat();
            }

            if (unit.RangedCombat != null)
            {
                unit.RangedCombat.StartCombat();
            }
        }
    }

    public void CheckVictory()
    {
        if (battleEnded)
            return;

        foreach (GridCell cell in enemyCells)
        {
            if (cell == null)
                continue;

            if (cell.IsOccupied)
            {
                return;
            }
        }

        battleEnded = true;

        StartCoroutine(VictoryAfterDelay());
    }

    private IEnumerator VictoryAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        Victory();
    }

    private void Victory()
    {
        GiveBattleReward(true);

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (!cell.IsOccupied)
                continue;

            Unit unit = cell.currentUnit;

            if (unit == null)
                continue;

            if (unit.Combat != null)
            {
                unit.Combat.StopCombat();
            }

            if (unit.RangedCombat != null)
            {
                unit.RangedCombat.StopCombat();
            }
        }

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (!cell.IsOccupied)
                continue;

            Unit unit = cell.currentUnit;

            if (unit == null)
                continue;

            if (unit.Animator != null)
            {
                unit.Animator.ResetTrigger("Attack");
                unit.Animator.SetBool("IsMoving", false);
                unit.Animator.SetTrigger("Victory");
            }
        }

        if (victoryUI != null)
        {
            victoryUI.ShowVictory();
        }
    }

    public void CheckDefeat()
    {
        if (battleEnded)
            return;

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (cell.IsOccupied)
            {
                return;
            }
        }

        battleEnded = true;

        StartCoroutine(DefeatAfterDelay());
    }

    private IEnumerator DefeatAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        Defeat();
    }

    private void Defeat()
    {
        GiveBattleReward(false);

        isFighting = false;

        if (defeatUI != null)
        {
            defeatUI.ShowDefeat();
        }
    }

    public void ResetPlayerForNextLevel()
    {
        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (!cell.IsOccupied)
                continue;

            Unit unit = cell.currentUnit;

            if (unit == null)
                continue;

            if (unit.Combat != null)
            {
                unit.Combat.StopCombat();
            }

            if (unit.RangedCombat != null)
            {
                unit.RangedCombat.StopCombat();
            }

            if (unit.currentCell != null)
            {
                unit.transform.position = unit.currentCell.transform.position;
            }

            if (unit.currentCell != null)
            {
                unit.transform.rotation = unit.currentCell.transform.rotation;
            }

            if (unit.Animator != null)
            {
                unit.Animator.ResetTrigger("Attack");
                unit.Animator.ResetTrigger("Victory");

                unit.Animator.SetBool("IsMoving",false);

                unit.Animator.Play("Idle", 0, 0f);
            }
        }

        isFighting = false;
    }

    private void GiveBattleReward(bool victory)
    {
        if (goldManager == null)
        {
            return;
        }

        if (enemySetup == null)
        {
            return;
        }

        int totalEnemyHealth = enemySetup.GetTotalEnemyMaxHealth();

        int reward;

        if (victory)
        {
            reward = totalEnemyHealth;
        }
        else
        {
            reward = totalEnemyHealth / 3;
        }

        if (reward <= 0)
        {
            return;
        }

        goldManager.AddGold(reward);
    }

    public void ClearData()
    {
        if (unitPool != null)
        {
            foreach (GridCell cell in playerCells)
            {
                if (cell == null)
                {
                    continue;
                }

                Unit unit = cell.currentUnit;

                if (unit == null)
                {
                    continue;
                }

                unitPool.ReturnPlayerUnit(unit);
            }
        }

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
            {
                continue;
            }

            cell.RemoveUnit();
        }

        playerSnapshot.Clear();

        isFighting = false;
        battleEnded = false;

        if (goldManager != null)
        {
            goldManager.ClearData();
        }
    }
}