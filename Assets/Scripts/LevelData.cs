using System;
using System.Collections.Generic;

[Serializable]
public class EnemyData
{
    public string type;
    public int level;
    public string bossLevel;
    public string cell;
}

[Serializable]
public class LevelData
{
    public int level;
    public List<EnemyData> enemies;
}

[Serializable]
public class LevelDataList
{
    public List<LevelData> levels;
}