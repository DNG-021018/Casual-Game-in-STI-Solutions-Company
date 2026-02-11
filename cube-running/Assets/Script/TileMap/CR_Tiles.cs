using System.Collections.Generic;
using UnityEngine;

namespace CB_CubeRunner
{
    public class CR_Tiles : MonoBehaviour
    {
        CR_TileMap[] tiles;

        public int RowIndex { get; set; }

        [Header("Spawn Settings")]
        [SerializeField] private GameObject trapPrefab;
        [SerializeField] private GameObject coinPrefab;
        [Tooltip("Coin spawn world Y (fixed)")]
        [SerializeField] private float coinSpawnY = 1f;

        [Header("Trap count (min..max). If min==max it's fixed)")]
        [SerializeField, Min(0)] private int minTraps = 1;
        [SerializeField, Min(0)] private int maxTraps = 1;

        [Header("Coin count (min..max). If min==max it's fixed)")]
        [SerializeField, Min(0)] private int minCoins = 0;
        [SerializeField, Min(0)] private int maxCoins = 1;

        [Header("Disabled Tiles count (min..max). If min==max it's fixed)")]
        [SerializeField, Min(0)] private int minDisabledTiles = 0;
        [SerializeField, Min(0)] private int maxDisabledTiles = 1;

        List<GameObject> _spawned = new List<GameObject>();
        HashSet<int> _disabledTileIndices = new HashSet<int>();

        void Awake()
        {
            tiles = GetComponentsInChildren<CR_TileMap>(true);
        }

        public void RespawnContent(bool allowSpawn)
        {
            ResetAllTiles();

            if (allowSpawn)
            {
                SpawnTrapsAndCoins();
            }
        }

        public CR_TileMap[] GetAllChildren() => tiles;

        public void SetChildrenFall(bool isFalling, float duration = 1f, float fallDistance = 10f)
        {
            if (!isFalling) return;

            foreach (var tile in tiles)
            {
                if (tile != null)
                {
                    tile.EnableFall(duration, fallDistance);
                }
            }
        }

        public void ResetAllTiles()
        {
            foreach (var tile in tiles)
            {
                if (tile != null)
                {
                    tile.gameObject.SetActive(true);
                    tile.ResetTile();

                    var renderers = tile.GetComponentsInChildren<Renderer>();
                    foreach (var rend in renderers)
                    {
                        rend.enabled = true;
                    }
                }
            }

            ClearSpawned();
            _disabledTileIndices.Clear();
        }

        void ClearSpawned()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                var go = _spawned[i];
                if (go != null) Destroy(go);
            }
            _spawned.Clear();
        }

        void SpawnTrapsAndCoins()
        {
            ClearSpawned();
            _disabledTileIndices.Clear();

            if (tiles == null || tiles.Length == 0)
            {
                Debug.LogWarning("[CR_Tiles] No tiles found for spawning!");
                return;
            }

            List<int> floorIndices = new List<int>();
            for (int i = 0; i < tiles.Length; i++)
            {
                var t = tiles[i];
                if (t != null && t.tileType == CR_TileType.Floor)
                    floorIndices.Add(i);
            }


            if (floorIndices.Count == 0)
            {
                Debug.LogWarning("[CR_Tiles] No floor tiles found!");
                return;
            }

            int trapCount = Mathf.Clamp(RandomRangeInclusive(minTraps, maxTraps), 0, floorIndices.Count);
            int coinCount = Mathf.Clamp(RandomRangeInclusive(minCoins, maxCoins), 0, floorIndices.Count);
            int disabledCount = Mathf.Clamp(RandomRangeInclusive(minDisabledTiles, maxDisabledTiles), 0, floorIndices.Count);


            HashSet<int> chosenIndices = new HashSet<int>();
            List<int> availableForTraps = new List<int>(floorIndices);

            for (int i = 0; i < trapCount; i++)
            {
                int pick = PickRandomFromListAndRemove(availableForTraps);
                if (pick < 0) break;
                chosenIndices.Add(pick);
                SpawnTrapAtTileIndex(pick);
            }

            List<int> remainingForDisabled = new List<int>();
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null && tiles[i].tileType == CR_TileType.Floor && !chosenIndices.Contains(i))
                    remainingForDisabled.Add(i);
            }

            disabledCount = Mathf.Min(disabledCount, remainingForDisabled.Count);

            for (int i = 0; i < disabledCount; i++)
            {
                int choiceIndex = PickRandomFromListAndRemove(remainingForDisabled);
                if (choiceIndex < 0) break;
                chosenIndices.Add(choiceIndex);
                DisableTileAtIndex(choiceIndex);
            }

            List<int> remainingForCoins = new List<int>();
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null && tiles[i].tileType == CR_TileType.Floor && !chosenIndices.Contains(i))
                    remainingForCoins.Add(i);
            }

            coinCount = Mathf.Min(coinCount, remainingForCoins.Count);

            for (int i = 0; i < coinCount; i++)
            {
                int choiceIndex = PickRandomFromListAndRemove(remainingForCoins);
                if (choiceIndex < 0) break;
                SpawnCoinAtTileIndex(choiceIndex);
            }

        }

        int RandomRangeInclusive(int a, int b)
        {
            if (a > b) { var tmp = a; a = b; b = tmp; }
            return Random.Range(a, b + 1);
        }

        int PickRandomFromListAndRemove(List<int> list)
        {
            if (list == null || list.Count == 0) return -1;
            int idx = Random.Range(0, list.Count);
            int val = list[idx];
            list.RemoveAt(idx);
            return val;
        }

        void SpawnTrapAtTileIndex(int tileIndex)
        {
            if (trapPrefab == null)
            {
                Debug.LogWarning("[CR_Tiles] Trap prefab is not assigned!");
                return;
            }

            var tile = tiles[tileIndex];
            if (tile == null) return;

            Vector3 spawnPos = tile.transform.position;
            spawnPos = new Vector3(spawnPos.x, tile.transform.position.y, spawnPos.z);

            GameObject spawned = Instantiate(trapPrefab, spawnPos, trapPrefab.transform.rotation, transform);
            _spawned.Add(spawned);


            tile.gameObject.SetActive(false);
        }

        void SpawnCoinAtTileIndex(int tileIndex)
        {
            if (coinPrefab == null)
            {
                Debug.LogWarning("[CR_Tiles] Coin prefab is not assigned!");
                return;
            }

            var tile = tiles[tileIndex];
            if (tile == null) return;

            Vector3 spawnPos = tile.transform.position;
            spawnPos = new Vector3(spawnPos.x, coinSpawnY, spawnPos.z);

            GameObject spawned = Instantiate(coinPrefab, spawnPos, coinPrefab.transform.rotation, tile.transform);
            _spawned.Add(spawned);

        }

        void DisableTileAtIndex(int tileIndex)
        {
            var tile = tiles[tileIndex];
            if (tile == null) return;

            _disabledTileIndices.Add(tileIndex);

            tile.gameObject.SetActive(false);

        }

        public bool IsTileDisabled(CR_TileMap tile)
        {
            if (tile == null) return false;

            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] == tile)
                {
                    return _disabledTileIndices.Contains(i);
                }
            }

            return false;
        }
    }
}
