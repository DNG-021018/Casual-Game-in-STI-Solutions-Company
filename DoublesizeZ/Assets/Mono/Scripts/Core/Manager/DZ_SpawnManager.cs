using System.Collections;
using System.Collections.Generic;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace DoublesideZ
{
    [System.Serializable]
    public class SpawnPattern
    {
        [Tooltip("True = Left, False = Right")]
        public bool[] sides;
    }

    public class DZ_SpawnManager : MonoBehaviour
    {
        [Header("References")]
        public GameObject enemyPrefab;
        public Transform leftSpawnPoint;
        public Transform rightSpawnPoint;

        [Header("Patterns")]
        public List<SpawnPattern> patterns;

        [Header("Spawn Timing")]
        public float startInterval = 0.8f;
        public float minInterval = 0.3f;
        public float timeToMaxDifficulty = 60f;

        [Header("Enemy Speed")]
        public float startMoveSpeed = 2f;
        public float maxMoveSpeed = 6f;

        [Header("Lane Settings")]
        public float minDistanceBetweenEnemies = 1.5f;

        private float elapsedTime;
        private float currentInterval;
        private float currentMoveSpeed;

        private readonly Queue<bool> spawnQueue = new();
        private bool isSpawning;

        private readonly List<DZ_EnemyController> leftLane = new();
        private readonly List<DZ_EnemyController> rightLane = new();

        private Pooler<DZ_EnemyController> enemyPool;

        void Start()
        {
            enemyPool = ServiceLocator.Get<DZ_PoolManager>().EnemyPool;
            currentMoveSpeed = startMoveSpeed;
        }

        void OnEnable()
        {
            DZ_PlayerController.OnPlayerDeath += StopSpawning;
        }

        void OnDisable()
        {
            DZ_PlayerController.OnPlayerDeath -= StopSpawning;
        }

        void Update()
        {
            if (!isSpawning) return;

            elapsedTime += Time.deltaTime;

            float difficultyPercent = Mathf.Clamp01(elapsedTime / timeToMaxDifficulty);

            currentInterval = Mathf.Lerp(startInterval, minInterval, difficultyPercent);
            currentMoveSpeed = Mathf.Lerp(startMoveSpeed, maxMoveSpeed, difficultyPercent);

            CleanLane(leftLane);
            CleanLane(rightLane);
        }

        public void BeginSpawning()
        {
            if (isSpawning) return;

            isSpawning = true;
            StartCoroutine(SpawnLoop());
        }

        public void StopSpawning()
        {
            if (!isSpawning) return;

            isSpawning = false;
            StopAllCoroutines();
        }

        public void ResetSpawning()
        {
            StopSpawning();

            ReturnAllEnemiesToPool();

            elapsedTime = 0f;
            spawnQueue.Clear();

            currentMoveSpeed = startMoveSpeed;
            currentInterval = startInterval;
        }

        private void ReturnAllEnemiesToPool()
        {
            foreach (var enemy in leftLane)
                if (enemy != null) enemy.OnReturnToPool();

            foreach (var enemy in rightLane)
                if (enemy != null) enemy.OnReturnToPool();

            leftLane.Clear();
            rightLane.Clear();
        }

        private IEnumerator SpawnLoop()
        {
            while (isSpawning)
            {
                if (spawnQueue.Count == 0)
                    LoadRandomPattern();

                bool spawnLeft = spawnQueue.Dequeue();

                TrySpawn(spawnLeft);

                yield return new WaitForSeconds(currentInterval);
            }
        }

        private void TrySpawn(bool spawnLeft)
        {
            List<DZ_EnemyController> lane = spawnLeft ? leftLane : rightLane;
            Transform spawnPoint = spawnLeft ? leftSpawnPoint : rightSpawnPoint;

            if (!CanSpawnInLane(lane, spawnPoint.position))
                return;

            DZ_EnemyController enemy = enemyPool.GetRandom(spawnPoint.position, Quaternion.identity);

            if (enemy != null)
            {
                lane.Add(enemy);

                enemy.InitPlayerPos(ServiceLocator.Get<DZ_PlayerController>().transform);

                float laneSpeed = currentMoveSpeed;

                if (lane.Count > 1)
                {
                    DZ_EnemyController previous = lane[lane.Count - 2];
                    laneSpeed = Mathf.Min(laneSpeed, previous.GetMoveSpeed());
                }

                enemy.SetMoveSpeed(laneSpeed);
            }
        }

        private bool CanSpawnInLane(List<DZ_EnemyController> lane, Vector3 spawnPos)
        {
            if (lane.Count == 0)
                return true;

            DZ_EnemyController lastEnemy = lane[lane.Count - 1];

            if (lastEnemy == null)
                return true;

            float distance = Vector3.Distance(spawnPos, lastEnemy.transform.position);

            return distance >= minDistanceBetweenEnemies;
        }

        private void CleanLane(List<DZ_EnemyController> lane)
        {
            lane.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);
        }

        private void LoadRandomPattern()
        {
            if (patterns.Count == 0) return;

            SpawnPattern pattern = patterns[Random.Range(0, patterns.Count)];

            foreach (bool side in pattern.sides)
                spawnQueue.Enqueue(side);
        }
    }
}