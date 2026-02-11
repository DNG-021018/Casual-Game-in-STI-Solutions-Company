using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewMapData", menuName = "MapData")]
public class MapData : ScriptableObject
{
    public List<int> levelDefine;
    public GameObject mapPrefabs;
    public List<LevelDetail> listMapLevel;
    public void InitLevelDetail()
    {
        listMapLevel = new();

        if (mapPrefabs == null)
        {
            Debug.LogError("mapPrefabs is null!");
            return;
        }

        List<Tilemap> levels = mapPrefabs.GetComponentsInChildren<Tilemap>().ToList();
        levels = levels.Where(item => item.tag.Equals("TileMapLevel")).ToList();

        if (levels.Count != levelDefine.Count)
        {
            Debug.LogError($"Wrong level Define, levels.count {levels.Count} / {levelDefine.Count}");
            return;
        }

        // Tìm tất cả spawn point trong map prefab
        var allSpawnPoints1 = mapPrefabs.GetComponentsInChildren<Transform>()
            .Where(t => t.CompareTag("SpawnPoint1"))
            .ToList();

        var allSpawnPoints2 = mapPrefabs.GetComponentsInChildren<Transform>()
            .Where(t => t.CompareTag("SpawnPoint2"))
            .ToList();

        if (allSpawnPoints1.Count != levelDefine.Count || allSpawnPoints2.Count != levelDefine.Count)
        {
            Debug.LogError("Số lượng spawn point không khớp với số level");
            return;
        }

        for (int i = 0; i < levelDefine.Count; i++)
        {
            listMapLevel.Add(new LevelDetail
            {
                level = levelDefine[i],
                tilemap = levels[i],
                spawnPoint1 = allSpawnPoints1[i],
                spawnPoint2 = allSpawnPoints2[i]
            });
        }
    }
}

[Serializable]
public struct LevelDetail
{
    public int level;
    public Tilemap tilemap;
    public Transform spawnPoint1;
    public Transform spawnPoint2;
}
