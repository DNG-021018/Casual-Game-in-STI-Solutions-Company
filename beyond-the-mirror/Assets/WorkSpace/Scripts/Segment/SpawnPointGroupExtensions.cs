using UnityEngine;

public static class SpawnPointGroupExtensions
{
    public static SpawnPointGroup GetMirrored(this SpawnPointGroup original)
    {
        return new SpawnPointGroup
        {
            left = CloneWithNewName(original.right, "Left"),
            center = CloneWithNewName(original.center, "Center"),
            right = CloneWithNewName(original.left, "Right")
        };
    }

    private static SpawnPointData CloneWithNewName(SpawnPointData source, string newName)
    {
        return new SpawnPointData
        {
            name = newName,
            category = source.category,
            aliveType = source.aliveType,
            obstacleDefinitions = source.obstacleDefinitions
        };
    }
}
