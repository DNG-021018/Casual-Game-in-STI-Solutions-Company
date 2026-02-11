using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefabs;
        public int size;
    }

    #region  Singleton
    public static ObjectPooler Instance;

    void Awake()
    {
        Instance = this;
    }
    #endregion

    public List<Pool> pools;

    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void OnEnable()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectsPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefabs);
                obj.SetActive(false);
                obj.tag = pool.tag;
                objectsPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectsPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        IPoolObject pooledObj = objectToSpawn.GetComponent<IPoolObject>();

        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }

        poolDictionary[tag].Enqueue(objectToSpawn);
        return objectToSpawn;
    }

    public void ReturnToPool(GameObject obj)
    {
        string tag = obj.tag;

        if (!poolDictionary.ContainsKey(tag))
        {
            // Debug.LogWarning($"No pool for tag {tag} → object name: {obj.name}, tag: {obj.tag}");
            obj.SetActive(false);
            return;
        }

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }

}
