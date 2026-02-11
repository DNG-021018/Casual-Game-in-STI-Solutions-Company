using UnityEngine;
using System.Collections.Generic;

public class PatternElement : MonoBehaviour
{
    public Transform spawnPointLeft;
    public Transform spawnPointCenter;
    public Transform spawnPointRight;
    public bool isMirror = false;

    [Header("Dành cho test trong Editor")]
    public SpawnPointGroup testGroup;

    private List<GameObject> spawnedObstacles = new();

    public void SpawnObstacles(SpawnPointGroup group)
    {
        ClearSpawnedObstacles();
        if (group == null)
        {
            Debug.LogWarning("❌ Dữ liệu group bị null khi spawn.");
            return;
        }
        SpawnOne(group.left, spawnPointLeft);
        SpawnOne(group.center, spawnPointCenter);
        SpawnOne(group.right, spawnPointRight);
    }

    private void SpawnOne(SpawnPointData data, Transform point)
    {
        if (data == null || point == null || data.obstacleDefinitions == null)
        {
            Debug.LogWarning($"❌ Dữ liệu bị null khi spawn: {data?.name ?? "Unknown"}");
            return;
        }

        if (data.category == ObstacleCategory.Alive && data.aliveType == AliveObstacleType.None)
        {
            Debug.Log($"🟡 {data.name} là loại None, bỏ qua.");
            return;
        }

        GameObject instance;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(data.obstacleDefinitions.prefab, point);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            // instance.transform.localScale = Vector3.one;
        }
        else
#endif
        {
            instance = ObstaclePoolManager.Instance.Get(data.obstacleDefinitions.prefab, point);
        }

        // Điều chỉnh riêng cho GateEnd
        if (data.aliveType == AliveObstacleType.GateEnd)
        {
            instance.transform.localPosition = new Vector3(-7f, 0f, 0f); // Dịch x = -7
            instance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // Quay y = 180 độ
        }
        if(data.aliveType == AliveObstacleType.Slide)
        {
            instance.transform.localPosition = new Vector3(0f, 2.07f, 0f); // Dịch y = 2.07
            instance.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); // Quay y = 0 độ
        }

        spawnedObstacles.Add(instance);
    }




    public void ClearSpawnedObstacles()
    {
        foreach (var obj in spawnedObstacles)
        {
            if (obj != null)
                ObstaclePoolManager.Instance.Release(obj);
        }

        spawnedObstacles.Clear();
    }

    public void SpawnTestGroup()
    {
        if (testGroup != null)
            SpawnObstacles(testGroup);
    }
}
