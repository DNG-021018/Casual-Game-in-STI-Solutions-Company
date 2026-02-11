using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "NewObstacle", menuName = "EndlessRunner/Obstacle")]
public class ObstacleDefinition : ScriptableObject
{
    public ObstacleCategory category;
    
    [Tooltip("Chỉ dùng khi category = Alive")]
    public AliveObstacleType aliveType;

    public GameObject prefab;
}

[System.Serializable]
public class ObstacleData
{
    public string obstacleName;
    public ObstacleCategory category;

    [Header("Only for Alive")]
    public AliveObstacleType aliveType;
}

public enum AliveObstacleType
{
    None,
    Jump,
    Slide,
    GateEnd,
    ItemInvisible
}