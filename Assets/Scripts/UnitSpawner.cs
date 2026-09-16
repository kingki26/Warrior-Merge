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

    [SerializeField] private EnemySetup enemySetup;
    [SerializeField] private VictoryUI victoryUI;

    private bool isFighting;

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

        // TODO: Show Defeat UI
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