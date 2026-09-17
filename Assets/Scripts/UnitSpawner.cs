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
    //
    [SerializeField] private EnemySetup enemySetup;
    [SerializeField] private VictoryUI victoryUI;
    [SerializeField] private DefeatUI defeatUI;
    [SerializeField] private UnitPool unitPool;

    [System.Serializable]
    private class PlayerUnitSnapshot
    {
        public UnitType unitType;
        public int level;
        public string cellName;
    }

    private List<PlayerUnitSnapshot> playerSnapshot = new List<PlayerUnitSnapshot>();

    private bool isFighting;

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
        // CLEAR PLAYER UNITS HIỆN TẠI
        // ========================================

        foreach (GridCell cell in playerCells)
        {
            if (cell == null)
                continue;

            if (!cell.IsOccupied)
                continue;

            Unit unit =
                cell.currentUnit;

            if (unit != null)
            {
                unitPool.ReturnUnit(unit);
            }
        }

        // ========================================
        // RESTORE SNAPSHOT
        // ========================================

        foreach (
            PlayerUnitSnapshot snapshot
            in playerSnapshot
        )
        {
            GridCell targetCell = null;

            // Tìm đúng Cell theo tên
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

            if (targetCell.IsOccupied)
            {
                Debug.LogError(
                    "GameManager → Cell already occupied: " +
                    snapshot.cellName
                );

                continue;
            }

            // Lấy Unit đúng Type + Level từ Pool
            Unit unit =
                unitPool.GetUnit(
                    snapshot.unitType,
                    snapshot.level
                );

            if (unit == null)
            {
                Debug.LogError(
                    "GameManager → Cannot restore Unit → " +
                    snapshot.unitType +
                    " Lv" +
                    snapshot.level
                );

                continue;
            }

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

        isFighting = false;

        Debug.Log(
            "GameManager → Player snapshot restored."
        );
    }


    public void SpawnMelee()
    {
        if (isFighting)
            return;

        Debug.Log("Spawn Melee button clicked!");

        SpawnUnit(meleeLv1Prefab);
    }

    public void StartFight()
    {
        if (isFighting)
            return;

        bool hasPlayerUnit = false;

        foreach (GridCell cell in playerCells)
        {
            if (cell.IsOccupied)
            {
                hasPlayerUnit = true;
                break;
            }
        }

        // Chưa có Unit → không bắt đầu Fight
        if (!hasPlayerUnit)
        {
            Debug.Log("Cannot start fight! Player has no units.");
            return;
        }

        // ========================================
        // START FIGHT
        // ========================================

        SavePlayerSnapshot();

        isFighting = true;

        enemySetup.StartFight();
    }

    public GridCell[] GetEnemyCells()
    {
        return enemyCells;
    }

    public void SpawnRanged()
    {
        if (isFighting)
            return;
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

        if (unitPool == null)
        {
            Debug.LogError("GameManager → UnitPool is NULL!");
            return;
        }

        foreach (GridCell cell in playerCells)
        {
            if (!cell.IsOccupied)
            {
                Debug.Log(
                    "Found empty cell: " +
                    cell.name
                );

                Unit newUnit =
                    unitPool.GetUnit(
                        prefab.unitType,
                        prefab.level
                    );

                if (newUnit == null)
                {
                    Debug.LogError(
                        "GameManager → Cannot get Unit from Pool!"
                    );

                    return;
                }

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

    public GridCell[] GetPlayerCells()
    {
        return playerCells;
    }

    public void StartPlayerCombat()
    {
        foreach (GridCell cell in playerCells)
        {
            if (!cell.IsOccupied)
                continue;

            Unit unit = cell.currentUnit;

            // =========================
            // MELEE
            // =========================

            UnitCombat meleeCombat =
                unit.GetComponent<UnitCombat>();

            if (meleeCombat != null)
            {
                meleeCombat.StartCombat();
            }

            // =========================
            // RANGED
            // =========================

            RangedCombat rangedCombat =
                unit.GetComponent<RangedCombat>();

            if (rangedCombat != null)
            {
                rangedCombat.StartCombat();
            }
        }
    }

    public void CheckVictory()
    {
        foreach (GridCell cell in enemyCells)
        {
            if (cell.IsOccupied)
            {
                return;
            }
        }

        Victory();
    }

    private void Victory()
    {
        Debug.Log("PLAYER VICTORY!");

        // Dừng toàn bộ Player
        foreach (GridCell cell in playerCells)
        {
            if (!cell.IsOccupied)
                continue;

            Unit unit = cell.currentUnit;

            UnitCombat combat =
                unit.GetComponent<UnitCombat>();

            if (combat != null)
            {
                combat.StopCombat();
            }
        }

        // Kích Victory cho Player
        foreach (GridCell cell in playerCells)
        {
            if (!cell.IsOccupied)
                continue;

            Animator animator =
                cell.currentUnit.GetComponent<Animator>();

            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                animator.SetBool("IsMoving", false);
                animator.SetTrigger("Victory");
            }
        }

        // Hiện Victory UI
        if (victoryUI != null)
        {
            victoryUI.ShowVictory();
        }
    }
    public void CheckDefeat()
    {
        foreach (GridCell cell in playerCells)
        {
            if (cell.IsOccupied)
            {
                return;
            }
        }

        Defeat();
    }

    private void Defeat()
    {
        Debug.Log("PLAYER DEFEAT!");

        isFighting = false;

        if (defeatUI != null)
        {
            defeatUI.ShowDefeat();
        }
    }
    public void ResetPlayerForNextLevel()
    {
        Debug.Log("GameManager → Reset Player for next level");

        foreach (GridCell cell in playerCells)
        {
            if (!cell.IsOccupied)
                continue;

            Unit unit = cell.currentUnit;

            if (unit == null)
                continue;

            // ========================================
            // STOP MELEE COMBAT
            // ========================================

            UnitCombat meleeCombat =
                unit.GetComponent<UnitCombat>();

            if (meleeCombat != null)
            {
                meleeCombat.StopCombat();
            }

            // ========================================
            // STOP RANGED COMBAT
            // ========================================

            RangedCombat rangedCombat =
                unit.GetComponent<RangedCombat>();

            if (rangedCombat != null)
            {
                rangedCombat.StopCombat();
            }

            // ========================================
            // RESET POSITION
            // ========================================

            if (unit.currentCell != null)
            {
                unit.transform.position =
                    unit.currentCell.transform.position;
            }

            // ========================================
            // RESET ROTATION
            // ========================================

            unit.transform.rotation =
                unit.currentCell.transform.rotation;

            // ========================================
            // RESET ANIMATION
            // ========================================

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
}