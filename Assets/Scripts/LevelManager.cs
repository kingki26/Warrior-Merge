using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private int currentLevel = 1;

    [Header("References")]
    [SerializeField] private EnemySetup enemySetup;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private VictoryUI victoryUI;
    [SerializeField] private DefeatUI defeatUI;

    [Header("UI")]
    [SerializeField] private TMP_Text levelText;

    public int CurrentLevel
    {
        get { return currentLevel; }
    }

    private void Start()
    {
        UpdateLevelUI();

        LoadCurrentLevel();
    }

    public void NextLevel()
    {
        currentLevel++;

        Debug.Log(
            "NEXT LEVEL → " +
            currentLevel
        );

        if (gameManager != null)
        {
            gameManager.RestorePlayerSnapshot();
        }

        if (victoryUI != null)
        {
            victoryUI.HideVictory();
        }

        if (defeatUI != null)
        {
            defeatUI.HideDefeat();
        }

        UpdateLevelUI();

        LoadCurrentLevel();
    }

    public void RetryLevel()
    {
        Debug.Log(
            "RETRY LEVEL → " +
            currentLevel
        );

        if (gameManager != null)
        {
            gameManager.RestorePlayerSnapshot();
        }

        if (victoryUI != null)
        {
            victoryUI.HideVictory();
        }

        if (defeatUI != null)
        {
            defeatUI.HideDefeat();
        }

        UpdateLevelUI();

        LoadCurrentLevel();
    }

    private void UpdateLevelUI()
    {
        if (levelText == null)
            return;

        levelText.text =
            "LEVEL " +
            currentLevel;
    }

    private void LoadCurrentLevel()
    {
        if (enemySetup == null)
        {
            Debug.LogError(
                "LevelManager → EnemySetup is NULL!"
            );

            return;
        }

        enemySetup.SetupLevel(currentLevel);
    }
}