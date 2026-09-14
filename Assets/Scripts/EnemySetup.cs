using UnityEngine;

public class EnemySetup : MonoBehaviour
{
    [Header("Enemy Setup")]
    [SerializeField] private GridCell[] spawnCells;

    [Header("References")]
    [SerializeField] private GameManager gameManager;

    private Unit[] enemyUnits;

    private void Start()
    {
        enemyUnits = GetComponentsInChildren<Unit>();

        for (int i = 0; i < enemyUnits.Length; i++)
        {
            enemyUnits[i].SetCell(spawnCells[i]);
        }
    }

    public void StartFight()
    {
        foreach (Unit enemy in enemyUnits)
        {
            if (enemy == null)
                continue;

            // =========================
            // MELEE
            // =========================

            UnitCombat meleeCombat =
                enemy.GetComponent<UnitCombat>();

            if (meleeCombat != null)
            {
                meleeCombat.StartCombat();
            }

            // =========================
            // RANGED
            // =========================

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