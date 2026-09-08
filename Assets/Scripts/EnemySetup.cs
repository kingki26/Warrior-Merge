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
            UnitCombat combat = enemy.GetComponent<UnitCombat>();

            if (combat != null)
            {
                combat.StartCombat();
            }
        }

        gameManager.StartPlayerCombat();
    }
}