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

    private List<PlayerUnitSnapshot> playerSnapshot =
        new List<PlayerUnitSnapshot>();

    private bool isFighting;
    private bool battleEnded;


    // ========================================
    // SAVE PLAYER SNAPSHOT
    // ========================================

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

            PlayerUnitSnapshot snapshot =
                new PlayerUnitSnapshot();

            snapshot.unitType = unit.unitType;
            snapshot.level = unit.level;
            snapshot.cellName = cell.name;

            playerSnapshot.Add(snapshot);

            Debug.Log(
                "Snapshot → " +
                snapshot.unitType +
                " Lv" +
                snapshot.level +
                " | Cell " +
                snapshot.cellName
            );
        }

        Debug.Log(
            "GameManager → Player snapshot saved: " +
            playerSnapshot.Count +
            " units."
        );
    }


    // ========================================
    // RESTORE PLAYER SNAPSHOT
    // ========================================

    public void RestorePlayerSnapshot()
    {
        Debug.Log(
            "GameManager → Restoring Player snapshot..."
        );

        if (unitPool == null)
        {
            Debug.LogError(
                "GameManager → UnitPool is NULL!"
            );

            return;
        }

        // ========================================
        // CLEAR TOÀN BỘ PLAYER UNITS
        // ========================================

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            Unit unit = cell.currentUnit;

            if (unit == null)
                continue;

            Debug.Log(
                "Restore → Returning existing Player → " +
                unit.name +
                " | Cell " +
                cell.name
            );

            unitPool.ReturnPlayerUnit(unit);
        }

        // ========================================
        // KIỂM TRA CELL ĐÃ TRỐNG
        // ========================================

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

        // ========================================
        // RESTORE SNAPSHOT
        // ========================================

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
                Debug.LogError(
                    "GameManager → Cannot find Player Cell: " +
                    snapshot.cellName
                );

                continue;
            }

            // ====================================
            // CELL PHẢI TRỐNG
            // ====================================

            if (targetCell.IsOccupied)
            {
                Debug.LogError(
                    "Restore → Cannot restore to occupied Cell: " +
                    targetCell.name
                );

                continue;
            }

            // ====================================
            // GET PLAYER UNIT
            // ====================================

            Unit unit =
                unitPool.GetPlayerUnit(
                    snapshot.unitType,
                    snapshot.level
                );

            if (unit == null)
            {
                Debug.LogError(
                    "GameManager → Cannot restore Player Unit → " +
                    snapshot.unitType +
                    " Lv" +
                    snapshot.level
                );

                continue;
            }

            // ====================================
            // SET CELL
            // ====================================

            unit.SetCell(targetCell);

            Debug.Log(
                "RESTORE PLAYER → " +
                snapshot.unitType +
                " Lv" +
                snapshot.level +
                " | Cell " +
                snapshot.cellName
            );
        }

        // ========================================
        // RESET BATTLE STATE
        // ========================================

        isFighting = false;
        battleEnded = false;

        Debug.Log(
            "GameManager → Player snapshot restored."
        );
    }


    // ========================================
    // SPAWN MELEE
    // ========================================

    public void SpawnMelee()
    {
        if (isFighting)
            return;

        Debug.Log("Buy Melee button clicked!");

        TryBuyAndSpawn(
            meleeLv1Prefab,
            UnitType.Melee
        );
    }


    // ========================================
    // SPAWN RANGED
    // ========================================

    public void SpawnRanged()
    {
        if (isFighting)
            return;

        Debug.Log("Buy Ranged button clicked!");

        TryBuyAndSpawn(
            rangedLv1Prefab,
            UnitType.Ranged
        );
    }


    // ========================================
    // START FIGHT
    // ========================================

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
            Debug.Log(
                "Cannot start fight! Player has no units."
            );

            return;
        }

        // ========================================
        // SAVE SNAPSHOT
        // ========================================

        SavePlayerSnapshot();

        // ========================================
        // START BATTLE
        // ========================================

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


    // ========================================
    // GET ENEMY CELLS
    // ========================================

    public GridCell[] GetEnemyCells()
    {
        return enemyCells;
    }


    // ========================================
    // SPAWN PLAYER UNIT
    // ========================================

    private void TryBuyAndSpawn(
    Unit prefab,
    UnitType unitType)
    {
        if (prefab == null)
        {
            Debug.LogError(
                "GameManager → Unit prefab is NULL!"
            );
            return;
        }

        if (unitPool == null)
        {
            Debug.LogError(
                "GameManager → UnitPool is NULL!"
            );
            return;
        }

        if (goldManager == null)
        {
            Debug.LogError(
                "GameManager → GoldManager is NULL!"
            );
            return;
        }

        // --------------------------------
        // 1. Tìm ô trống trước
        // --------------------------------

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
            Debug.Log(
                "GameManager → Player grid is full! " +
                "Cannot buy Unit."
            );

            return;
        }

        // --------------------------------
        // 2. Kiểm tra giá hiện tại
        // --------------------------------

        int price;

        if (unitType == UnitType.Melee)
            price = goldManager.GetMeleePrice();
        else
            price = goldManager.GetRangedPrice();

        if (!goldManager.CanAfford(price))
        {
            Debug.Log(
                "GameManager → Not enough Gold! " +
                "Need: " +
                price +
                " | Current: " +
                goldManager.GetGold()
            );

            return;
        }

        // --------------------------------
        // 3. Lấy Unit từ Pool
        // --------------------------------

        Unit newUnit =
            unitPool.GetPlayerUnit(
                prefab.unitType,
                prefab.level
            );

        if (newUnit == null)
        {
            Debug.LogError(
                "GameManager → Cannot get Player Unit from Pool!"
            );

            return;
        }

        // --------------------------------
        // 4. Trừ Gold + tăng giá
        // --------------------------------

        bool purchaseSuccess;

        if (unitType == UnitType.Melee)
        {
            purchaseSuccess =
                goldManager.BuyMelee();
        }
        else
        {
            purchaseSuccess =
                goldManager.BuyRanged();
        }

        // --------------------------------
        // 5. Nếu mua thất bại → trả Unit về Pool
        // --------------------------------

        if (!purchaseSuccess)
        {
            unitPool.ReturnPlayerUnit(newUnit);

            Debug.Log(
                "GameManager → Purchase failed → " +
                "Unit returned to Pool."
            );

            return;
        }

        // --------------------------------
        // 6. Spawn Unit
        // --------------------------------

        newUnit.SetCell(emptyCell);

        Debug.Log(
            "PURCHASE SUCCESS → " +
            unitType +
            " Lv" +
            newUnit.level +
            " | Price: " +
            price +
            " | Cell: " +
            emptyCell.name
        );
    }


    // ========================================
    // GET NEXT LEVEL PREFAB
    // ========================================

    public Unit GetNextLevelPrefab(Unit unit)
    {
        if (unit == null)
            return null;

        int nextLevel =
            unit.level + 1;

        if (nextLevel > 5)
            return null;

        if (unit.unitType == UnitType.Melee)
        {
            if (
                meleePrefabs == null ||
                nextLevel - 1 >= meleePrefabs.Length
            )
                return null;

            return meleePrefabs[nextLevel - 1];
        }

        if (
            rangedPrefabs == null ||
            nextLevel - 1 >= rangedPrefabs.Length
        )
            return null;

        return rangedPrefabs[nextLevel - 1];
    }


    // ========================================
    // GET PLAYER CELLS
    // ========================================

    public GridCell[] GetPlayerCells()
    {
        return playerCells;
    }


    // ========================================
    // START PLAYER COMBAT
    // ========================================

    public void StartPlayerCombat()
    {
        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (!cell.IsOccupied)
                continue;

            Unit unit =
                cell.currentUnit;

            if (unit == null)
                continue;

            // ====================================
            // MELEE
            // ====================================

            UnitCombat meleeCombat =
                unit.GetComponent<UnitCombat>();

            if (meleeCombat != null)
            {
                meleeCombat.StartCombat();
            }

            // ====================================
            // RANGED
            // ====================================

            RangedCombat rangedCombat =
                unit.GetComponent<RangedCombat>();

            if (rangedCombat != null)
            {
                rangedCombat.StartCombat();
            }
        }
    }


    // ========================================
    // CHECK VICTORY
    // ========================================

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

        Victory();
    }


    // ========================================
    // VICTORY
    // ========================================

    private void Victory()
    {
        Debug.Log(
            "PLAYER VICTORY!"
        );
        GiveBattleReward(true);


        // ====================================
        // STOP PLAYER COMBAT
        // ====================================

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (!cell.IsOccupied)
                continue;

            Unit unit =
                cell.currentUnit;

            if (unit == null)
                continue;

            UnitCombat meleeCombat =
                unit.GetComponent<UnitCombat>();

            if (meleeCombat != null)
            {
                meleeCombat.StopCombat();
            }

            RangedCombat rangedCombat =
                unit.GetComponent<RangedCombat>();

            if (rangedCombat != null)
            {
                rangedCombat.StopCombat();
            }
        }

        // ====================================
        // PLAYER VICTORY ANIMATION
        // ====================================

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (!cell.IsOccupied)
                continue;

            Animator animator =
                cell.currentUnit.GetComponent<Animator>();

            if (animator != null)
            {
                animator.ResetTrigger("Attack");

                animator.SetBool(
                    "IsMoving",
                    false
                );

                animator.SetTrigger(
                    "Victory"
                );
            }
        }

        // ====================================
        // SHOW UI
        // ====================================

        if (victoryUI != null)
        {
            victoryUI.ShowVictory();
        }
    }


    // ========================================
    // CHECK DEFEAT
    // ========================================

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

        Defeat();
    }


    // ========================================
    // DEFEAT
    // ========================================

    private void Defeat()
    {
        Debug.Log(
            "PLAYER DEFEAT!"
        );

        GiveBattleReward(false);

        isFighting = false;

        if (defeatUI != null)
        {
            defeatUI.ShowDefeat();
        }
    }


    // ========================================
    // RESET PLAYER FOR NEXT LEVEL
    // ========================================

    public void ResetPlayerForNextLevel()
    {
        Debug.Log(
            "GameManager → Reset Player for next level"
        );

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (!cell.IsOccupied)
                continue;

            Unit unit =
                cell.currentUnit;

            if (unit == null)
                continue;

            // ====================================
            // STOP MELEE
            // ====================================

            UnitCombat meleeCombat =
                unit.GetComponent<UnitCombat>();

            if (meleeCombat != null)
            {
                meleeCombat.StopCombat();
            }

            // ====================================
            // STOP RANGED
            // ====================================

            RangedCombat rangedCombat =
                unit.GetComponent<RangedCombat>();

            if (rangedCombat != null)
            {
                rangedCombat.StopCombat();
            }

            // ====================================
            // RESET POSITION
            // ====================================

            if (unit.currentCell != null)
            {
                unit.transform.position =
                    unit.currentCell.transform.position;
            }

            // ====================================
            // RESET ROTATION
            // ====================================

            if (unit.currentCell != null)
            {
                unit.transform.rotation =
                    unit.currentCell.transform.rotation;
            }

            // ====================================
            // RESET ANIMATION
            // ====================================

            Animator animator =
                unit.GetComponent<Animator>();

            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                animator.ResetTrigger("Victory");

                animator.SetBool(
                    "IsMoving",
                    false
                );

                animator.Play(
                    "Idle",
                    0,
                    0f
                );
            }
        }

        isFighting = false;
    }

    private void GiveBattleReward(bool victory)
    {
        if (goldManager == null)
        {
            Debug.LogError(
                "GameManager → GoldManager is NULL!"
            );
            return;
        }

        if (enemySetup == null)
        {
            Debug.LogError(
                "GameManager → EnemySetup is NULL!"
            );
            return;
        }

        int totalEnemyHealth =
            enemySetup.GetTotalEnemyMaxHealth();

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
            Debug.LogWarning(
                "GameManager → Battle reward is 0."
            );
            return;
        }

        goldManager.AddGold(reward);

        Debug.Log(
            "BATTLE REWARD → " +
            (victory ? "VICTORY" : "DEFEAT") +
            " | Enemy Max HP: " +
            totalEnemyHealth +
            " | Gold Reward: +" +
            reward
        );
    }
}