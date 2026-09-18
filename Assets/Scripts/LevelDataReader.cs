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
            //Debug.LogError(
            //    "Không tìm thấy JSON: " + fileName
            //);

            return;
        }

        levelDatabase =
            JsonUtility.FromJson<LevelDataList>(
                jsonFile.text
            );

        if (levelDatabase == null ||
            levelDatabase.levels == null)
        {
            //Debug.LogError(
            //    "JSON không đọc được hoặc không có levels!"
            //);

            return;
        }

        //Debug.Log(
        //    "Đã load JSON thành công! Tổng số level: " +
        //    levelDatabase.levels.Count
        //);

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

        //if (levelData == null)
        //{
        //    Debug.LogError(
        //        "Không tìm thấy Level " + levelNumber
        //    );

        //    return;
        //}

        //Debug.Log(
        //    "===== LEVEL " +
        //    levelData.level +
        //    " ====="
        //);

        //foreach (EnemyData enemy in levelData.enemies)
        //{
        //    Debug.Log(
        //        enemy.type +
        //        " Lv" +
        //        enemy.level +
        //        " → Cell: " +
        //        enemy.cell
        //    );
        //}
    }
}