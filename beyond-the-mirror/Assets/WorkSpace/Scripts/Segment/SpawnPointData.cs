using UnityEngine;

public enum ObstacleCategory { Alive, Dead }


[System.Serializable]
public class SpawnPointData
{
    public string name;
    public ObstacleCategory category;
    public AliveObstacleType aliveType = AliveObstacleType.None;
    public ObstacleDefinition obstacleDefinitions;
}

[System.Serializable]
public class SpawnPointGroup
{
    public SpawnPointData left = new SpawnPointData { name = "Left" };
    public SpawnPointData center = new SpawnPointData { name = "Center" };
    public SpawnPointData right = new SpawnPointData { name = "Right" };

    public bool isGateCombo = false; // ✅ mới thêm

}
