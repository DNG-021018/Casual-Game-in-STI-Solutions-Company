using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class ObstaclePoolManager : MonoBehaviour
{
    public static ObstaclePoolManager Instance;

    private Dictionary<GameObject, ObjectPool<GameObject>> pools = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Preload(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = Get(prefab, transform);
            Release(obj);
        }
    }

    public GameObject Get(GameObject prefab, Transform parent)
    {
        if (!pools.TryGetValue(prefab, out var pool))
        {
            pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    var obj = Instantiate(prefab);
                    SetupPoolable(obj, pool); // gán pool callback nếu có
                    return obj;
                },
                actionOnGet: (obj) =>
                {
                    obj.SetActive(true);
                },
                actionOnRelease: (obj) =>
                {
                    obj.SetActive(false);
                    // Reset transform, animation, state... nếu cần
                },
                actionOnDestroy: (obj) =>
                {
                    Destroy(obj);
                },
                collectionCheck: false,
                defaultCapacity: 5
            );

            pools[prefab] = pool;
            Debug.Log($"🟢 Tạo pool cho prefab: {prefab.name}, CountAll = {pool.CountAll}");
            Preload(prefab, 5); // preload 5 instance
        }

        GameObject instance = pool.Get();

        instance.transform.SetParent(parent, false);
        // instance.transform.localPosition = Vector3.zero;
        // instance.transform.localRotation = Quaternion.identity;
        // instance.transform.localScale = Vector3.one;

        return instance;
    }

    public void Release(GameObject instance)
    {
        // Trả về đúng pool đã tạo ra object này
        var poolable = instance.GetComponent<PoolableObstacle>();
        if (poolable != null)
        {
            poolable.ReturnToPool(); // sẽ gọi pool.Release(instance)
        }
        else
        {
            Debug.LogWarning("🟡 Obstacle không có PoolableObstacle, sẽ bị destroy.");
            Destroy(instance);
        }
    }

    private void SetupPoolable(GameObject obj, ObjectPool<GameObject> pool)
    {
        var poolable = obj.GetComponent<PoolableObstacle>();
        if (poolable != null)
        {
            poolable.SetPool(() => pool.Release(obj));
        }
    }

#if UNITY_EDITOR
    [ContextMenu("📊 Debug Pool Sizes")]
    public void DebugPoolSizes()
    {
        foreach (var kvp in pools)
        {
            Debug.Log($"📦 Pool for {kvp.Key.name}: CountAll = {kvp.Value.CountAll}, Inactive = {kvp.Value.CountInactive}");
        }
    }
#endif
}
