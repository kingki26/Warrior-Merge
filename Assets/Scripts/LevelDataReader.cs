using UnityEngine;

public class LevelDataReader : MonoBehaviour
{
    [Header("JSON")]
    [SerializeField] private string fileName = "Levels/level_data_1_50_Melee_Ranged";

    private LevelDataList levelDatabase;

    private void Start()
    {
        LoadLevelData();
    }

    private void LoadLevelData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);

        if (jsonFile == null)
        {
            return;
        }

        levelDatabase =
            JsonUtility.FromJson<LevelDataList>(
                jsonFile.text
            );

        if (levelDatabase == null ||
            levelDatabase.levels == null)
        {

            return;
        }


        PrintLevel(1);
    }

    private void PrintLevel(int levelNumber)
    {
        LevelData levelData = null;

        foreach (LevelData level in levelDatabase.levels)
        {
            if (level.level == levelNumber)
            {
                levelData = level;
                break;
            }
        }
    }
}