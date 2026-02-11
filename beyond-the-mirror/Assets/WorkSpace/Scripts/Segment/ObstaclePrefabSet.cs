using UnityEngine;

[CreateAssetMenu(menuName = "EndlessRunner/ObstaclePrefabSet")]
public class ObstaclePrefabSet : ScriptableObject
{
    public ObstacleDefinition jumpPrefab;
    public ObstacleDefinition slidePrefab;
    public ObstacleDefinition deadPrefab;
    public ObstacleDefinition gateEndPrefab;
    public ObstacleDefinition itemInvisiblePrefab;
    public ObstacleDefinition nonePrefab;
}
