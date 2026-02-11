using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public string objectTag = "Item";

    [Tooltip("Số lượng spawn khi bắt đầu")]
    public int initialSpawnCount = 10;

    [Tooltip("Giới hạn số lượng obstacle trên bản đồ")]
    public int maxAllowedObjects = 15;

    [Tooltip("Spawn thêm nếu dưới max, trong khoảng này")]
    public int minSpawnPerInterval = 1;
    public int maxSpawnPerInterval = 3;

    [Tooltip("Khoảng thời gian giữa các lần kiểm tra spawn thêm")]
    public float spawnCheckInterval = 2f;

    [Header("Spawn Area")]
    public Vector3 areaCenter = Vector3.zero;
    public Vector2 areaSize = new Vector2(10, 10);

    private float timer;
    private ObjectPooler objectPooler;

    // Lưu danh sách các object đã spawn (và còn đang tồn tại)
    private List<GameObject> activeObjects = new List<GameObject>();

    void Start()
    {
        objectPooler = ObjectPooler.Instance;

        // Spawn ban đầu
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnObject();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnCheckInterval)
        {
            // Xoá những object không còn active (vì đã được trả về pool hoặc destroy)
            activeObjects.RemoveAll(obj => obj == null || !obj.activeInHierarchy);

            if (activeObjects.Count < maxAllowedObjects)
            {
                int spawnCount = Random.Range(minSpawnPerInterval, maxSpawnPerInterval + 1);

                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnObject();
                }
            }

            timer = 0f;
        }
    }

    void SpawnObject()
    {
        Vector3 spawnPos = GetRandomPointInArea();
        GameObject obj = objectPooler.SpawnFromPool(objectTag, spawnPos, Quaternion.identity);
        if (obj != null)
            activeObjects.Add(obj);
    }

    Vector3 GetRandomPointInArea()
    {
        float halfWidth = areaSize.x / 2f;
        float halfDepth = areaSize.y / 2f;

        float randomX = Random.Range(-halfWidth, halfWidth);
        float randomZ = Random.Range(-halfDepth, halfDepth);

        Vector3 center = transform.position + areaCenter;
        return new Vector3(center.x + randomX, center.y, center.z + randomZ);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = transform.position + areaCenter;
        Vector3 size = new Vector3(areaSize.x, 0.1f, areaSize.y);
        Gizmos.DrawWireCube(center, size);
    }
}
