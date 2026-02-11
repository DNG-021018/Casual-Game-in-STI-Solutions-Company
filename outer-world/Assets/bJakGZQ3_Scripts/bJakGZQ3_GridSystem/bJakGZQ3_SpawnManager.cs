using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    [DefaultExecutionOrder(0)]
    public class bJakGZQ3_SpawnManager : MonoBehaviour
    {
        [Serializable]
        public struct SpawnRoundRule
        {
            public int startRound;

            public int roundsCount;

            public int enemyCount;

            public int itemCount;

            public bool infinite;
        }

        [Header("===== RULES =====")]
        [SerializeField] private List<SpawnRoundRule> rules = new();

        [Header("===== PREFABS / LIMIT =====")]
        [SerializeField] private GameObject[] enemyPrefabs;

        [SerializeField] private GameObject[] itemPrefabs;

        [SerializeField] private int maxTotalEnemies = 2;
        [SerializeField] private int maxTotalItems = 5;

        [Header("===== SPAWN TUNING =====")]
        [SerializeField, Range(0.0f, 1.0f)]
        private float spawnDelayBetween = 0.2f;

        [SerializeField] private float checkRadius = 0.2f;

        [Header("===== FLOW CONTROL =====")]
        [SerializeField] private bool autoStartSpawning = false;

        private bool spawningStarted = false;
        private bool isSpawningRightNow = false;

        private bJakGZQ3_GridSystem grid;
        private bJakGZQ3_LevelManager levelManager;
        private bJakGZQ3_Player player;

        void Awake()
        {
            spawningStarted = false;
            isSpawningRightNow = false;

            grid = GetComponent<bJakGZQ3_GridSystem>();
            levelManager = bJakGZQ3_LevelManager.Instance;
            player = FindFirstObjectByType<bJakGZQ3_Player>();
        }


        void Start()
        {
            if (autoStartSpawning)
            {
                BeginSpawning();
            }
        }

        void OnEnable()
        {
            if (levelManager != null)
            {
                levelManager.OnPlayerStep += HandleOnPlayerStep; // đổi tên cho clear
            }
        }

        void OnDisable()
        {
            if (levelManager != null)
            {
                levelManager.OnPlayerStep -= HandleOnPlayerStep;
            }
        }

        void HandleOnPlayerStep()
        {
            if (!spawningStarted) return;

            var gm = bJakGZQ3_GameManager.Instance;
            if (gm != null && gm.GetState() != GameState.Play)
                return;

            StartCoroutine(SpawnMissingNowCoroutine());
        }


        public void BeginSpawning()
        {
            if (spawningStarted) return;
            spawningStarted = true;
            StartCoroutine(InitialSpawnRoutine());
        }

        IEnumerator InitialSpawnRoutine()
        {
            float timeout = 10f;
            float t = 0f;
            while ((grid == null || !grid.IsInitialized) && t < timeout)
            {
                t += Time.deltaTime;
                yield return null;
            }

            if (grid == null || !grid.IsInitialized)
            {
                Debug.LogWarning("[SpawnManager] Grid not ready => skip InitialSpawn");
                yield break;
            }

            var gm = bJakGZQ3_GameManager.Instance;
            if (gm != null)
            {
                t = 0f;
                while (gm.GetState() != GameState.Play && t < timeout)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }

            yield return SpawnMissingNowCoroutine();
        }

        void HandleOnPlayerMoveStart()
        {
            if (!spawningStarted) return;

            var gm = bJakGZQ3_GameManager.Instance;
            if (gm != null && gm.GetState() != GameState.Play)
                return;

            StartCoroutine(SpawnMissingNowCoroutine());
        }

        IEnumerator SpawnMissingNowCoroutine()
        {
            if (levelManager == null || grid == null) yield break;
            if (!grid.IsInitialized) yield break;
            if (isSpawningRightNow) yield break;

            var ruleOpt = GetRuleForRound(levelManager.RoundCount);
            if (ruleOpt == null) yield break;
            var rule = ruleOpt.Value;

            int wantEnemies = Mathf.Min(rule.enemyCount, maxTotalEnemies);
            int wantItems = Mathf.Min(rule.itemCount, maxTotalItems);

            int haveEnemies = CountActiveEnemies();
            int haveItems = CountActiveItems();

            int missingEnemies = Mathf.Clamp(wantEnemies - haveEnemies, 0, maxTotalEnemies - haveEnemies);
            int missingItems = Mathf.Clamp(wantItems - haveItems, 0, maxTotalItems - haveItems);

            if (missingEnemies == 0 && missingItems == 0)
                yield break;

            isSpawningRightNow = true;

            var move = player != null ? player.GetComponent<bJakGZQ3_GridMovement>() : null;
            move?.DisableMovement();

            // track ô đã pick trong đợt spawn này
            HashSet<Vector2Int> pickedCells = new HashSet<Vector2Int>();

            // ENEMY
            for (int i = 0; i < missingEnemies; i++)
            {
                Vector3 pos;
                bool ok = grid.TryGetRandomFreeCellExclusive(
                    pickedCells,
                    out pos,
                    checkRadius,
                    1 // check ở đúng Y mà enemy sẽ đứng
                );

                if (!ok)
                {
                    Debug.Log("[SpawnManager] no free cell for enemy");
                }
                else
                {
                    // ép y
                    pos.y = 1;

                    // nhớ cell này để không xài lại
                    if (grid.WorldToCellIndices(pos, out var cellID))
                        pickedCells.Add(cellID);

                    SpawnEnemyAt(pos);
                    yield return new WaitForSeconds(spawnDelayBetween);
                }
            }

            // ITEM
            for (int i = 0; i < missingItems; i++)
            {
                Vector3 pos;
                bool ok = grid.TryGetRandomFreeCellExclusive(
                    pickedCells,
                    out pos,
                    checkRadius,
                    2
                );

                if (!ok)
                {
                    Debug.Log("[SpawnManager] no free cell for item");
                }
                else
                {
                    pos.y = 2;

                    if (grid.WorldToCellIndices(pos, out var cellID))
                        pickedCells.Add(cellID);

                    SpawnItemAt(pos);
                    yield return new WaitForSeconds(spawnDelayBetween);
                }
            }

            move?.EnableMovement();
            isSpawningRightNow = false;
        }


        void SpawnEnemyAt(Vector3 worldPos)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

            GameObject prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
            if (prefab == null) return;

            worldPos.y = 1;

            var go = Instantiate(prefab, worldPos, Quaternion.identity);

            if (grid != null)
            {
                Transform container = grid.transform.Find("Grid Container");
                if (container != null)
                    go.transform.SetParent(container, true);
            }
        }


        void SpawnItemAt(Vector3 worldPos)
        {
            if (itemPrefabs == null || itemPrefabs.Length == 0) return;

            GameObject prefab = itemPrefabs[UnityEngine.Random.Range(0, itemPrefabs.Length)];
            if (prefab == null) return;

            worldPos.y = 2;

            var go = Instantiate(prefab, worldPos, Quaternion.identity);

            if (grid != null)
            {
                Transform container = grid.transform.Find("Grid Container");
                if (container != null)
                    go.transform.SetParent(container, true);
            }
        }



        int CountActiveEnemies()
        {
            var arr = FindObjectsByType<bJakGZQ3_Enemy>(FindObjectsSortMode.None);
            return arr != null ? arr.Length : 0;
        }

        int CountActiveItems()
        {
            int count = 0;
            try
            {
                var objs = GameObject.FindGameObjectsWithTag("Item");
                if (objs != null) count = objs.Length;
            }
            catch
            {
                count = 0;
            }

            return count;
        }

        SpawnRoundRule? GetRuleForRound(int round)
        {
            if (rules == null || rules.Count == 0) return null;

            rules.Sort((a, b) => a.startRound.CompareTo(b.startRound));

            for (int k = 0; k < rules.Count; k++)
            {
                var r = rules[k];

                int from = r.startRound;
                int len = Mathf.Max(1, r.roundsCount);
                int to = from + len - 1;

                if (round >= from && round <= to)
                {
                    return r;
                }
            }

            var last = rules[rules.Count - 1];
            if (last.infinite)
                return last;

            return last;
        }
    }
}
