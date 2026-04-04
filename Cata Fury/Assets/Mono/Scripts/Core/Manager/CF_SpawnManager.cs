using System;
using System.Collections;
using System.Collections.Generic;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CataFury
{
    public enum EDirection { Up, Down, Left, Right }

    public class CF_SpawnManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform centerPoint;

        [Header("Spawn Zones (BoxCollider)")]
        [SerializeField] private BoxCollider spawnZoneUp;
        [SerializeField] private BoxCollider spawnZoneDown;
        [SerializeField] private BoxCollider spawnZoneLeft;
        [SerializeField] private BoxCollider spawnZoneRight;

        [Header("Spawn Zones (Effects)")]
        [SerializeField] private ParticleSystem spawnZoneUpEffect;
        [SerializeField] private ParticleSystem spawnZoneDownEffect;
        [SerializeField] private ParticleSystem spawnZoneLeftEffect;
        [SerializeField] private ParticleSystem spawnZoneRightEffect;

        [Header("Wave Settings")]
        [SerializeField] private float firstWaveDelay = 1.5f;

        [Header("Spacing")]
        [SerializeField] private float enemyLocalScale = 2f;
        [SerializeField] private float enemyBaseRadius = 0.5f;
        [SerializeField] private float spacingPadding = 0.2f;
        [SerializeField] private float inGroupDelay = 0.2f;

        [Header("Difficulty Tiers")]
        [SerializeField]
        private List<DifficultyTier> tiers = new()
        {
            new DifficultyTier { minScore = 0,  waveInterval = 3.0f, groupSizeMin = 1, groupSizeMax = 2 },
            new DifficultyTier { minScore = 5,  waveInterval = 2.5f, groupSizeMin = 1, groupSizeMax = 3 },
            new DifficultyTier { minScore = 15, waveInterval = 2.0f, groupSizeMin = 2, groupSizeMax = 3 },
            new DifficultyTier { minScore = 30, waveInterval = 1.5f, groupSizeMin = 2, groupSizeMax = 4 },
            new DifficultyTier { minScore = 50, waveInterval = 1.0f, groupSizeMin = 3, groupSizeMax = 5 },
            new DifficultyTier { minScore = 80, waveInterval = 0.7f, groupSizeMin = 3, groupSizeMax = 6 },
        };

        private CF_PoolManager _poolManager;
        private Pooler<CF_EnemyController> _enemyPool;
        private CF_ScoreManager _scoreManager;

        private readonly List<EDirection> _allDirections = new()
        {
            EDirection.Up, EDirection.Down, EDirection.Left, EDirection.Right
        };

        private bool _isRunning;
        public bool IsRunning => _isRunning;
        private Coroutine _waveCoroutine;

        private DifficultyTier _currentTier;

        private float EnemySpacing => enemyLocalScale * enemyBaseRadius * 2f + spacingPadding;

        void Awake()
        {
            _poolManager = ServiceLocator.Get<CF_PoolManager>();
            _enemyPool = _poolManager.EnemyPool;
            _scoreManager = ServiceLocator.Get<CF_ScoreManager>();
            _currentTier = tiers[0];
        }

        void OnEnable()
        {
            if (_scoreManager != null)
                _scoreManager.OnScoreChanged += OnScoreChanged;
        }

        void OnDisable()
        {
            if (_scoreManager != null)
                _scoreManager.OnScoreChanged -= OnScoreChanged;
        }

        private void OnScoreChanged(int score)
        {
            DifficultyTier best = tiers[0];
            foreach (var tier in tiers)
                if (score >= tier.minScore)
                    best = tier;

            if (best.minScore != _currentTier.minScore)
                _currentTier = best;
        }

        // ─── Public Control ───────────────────────────────────────────

        public void BeginSpawning()
        {
            if (_isRunning) return;
            _isRunning = true;
            _currentTier = tiers[0];
            _waveCoroutine = StartCoroutine(WaveLoop());
        }

        public void PauseSpawning()
        {
            _isRunning = false;
            if (_waveCoroutine != null) { StopCoroutine(_waveCoroutine); _waveCoroutine = null; }
        }

        public void StopSpawning()
        {
            PauseSpawning();
            _enemyPool?.ReturnAll();
            StopAllEffects();
        }

        public void ResetSpawning()
        {
            StopSpawning();
            _currentTier = tiers[0];
        }

        // ─── Wave Loop ────────────────────────────────────────────────

        private IEnumerator WaveLoop()
        {
            yield return new WaitForSeconds(firstWaveDelay);

            while (_isRunning)
            {
                SpawnWave();
                yield return new WaitForSeconds(_currentTier.waveInterval);
            }
        }

        private void SpawnWave()
        {
            EDirection dir = _allDirections[Random.Range(0, _allDirections.Count)];
            int groupSize = Random.Range(_currentTier.groupSizeMin, _currentTier.groupSizeMax + 1);
            StartCoroutine(SpawnGroup(dir, groupSize));
        }

        private IEnumerator SpawnGroup(EDirection direction, int count)
        {
            BoxCollider zone = GetSpawnZone(direction);
            if (zone == null) yield break;

            // Play effect đúng hướng khi bắt đầu spawn group
            PlaySpawnEffect(direction);

            Vector3 center = centerPoint ? centerPoint.position : Vector3.zero;
            Vector3[] positions = CalculateSpawnPositions(zone, direction, count);

            for (int i = 0; i < positions.Length; i++)
            {
                CF_EnemyController enemy = _enemyPool.GetRandom(positions[i], Quaternion.identity);
                enemy?.Init(direction, positions[i], center);

                if (i < positions.Length - 1)
                    yield return new WaitForSeconds(inGroupDelay);
            }
        }

        // ─── Spawn Effects ────────────────────────────────────────────

        private void PlaySpawnEffect(EDirection direction)
        {
            ParticleSystem effect = direction switch
            {
                EDirection.Up => spawnZoneUpEffect,
                EDirection.Down => spawnZoneDownEffect,
                EDirection.Left => spawnZoneLeftEffect,
                EDirection.Right => spawnZoneRightEffect,
                _ => null
            };

            if (effect == null) return;

            // Stop trước để reset, rồi play lại
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            effect.Play(true);
        }

        private void StopAllEffects()
        {
            spawnZoneUpEffect?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            spawnZoneDownEffect?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            spawnZoneLeftEffect?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            spawnZoneRightEffect?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // ─── Position Calculation ─────────────────────────────────────

        private Vector3[] CalculateSpawnPositions(BoxCollider zone, EDirection direction, int count)
        {
            Bounds bounds = zone.bounds;
            Vector3 perp = GetPerpendicular(direction);
            float zoneWidth = GetZoneWidth(bounds, perp);
            float spacing = EnemySpacing;
            float totalWidth = spacing * (count - 1);

            if (count > 1 && totalWidth > zoneWidth)
            {
                spacing = zoneWidth / (count - 1);
                totalWidth = zoneWidth;
            }

            Vector3 zoneCenter = bounds.center;
            float startOffset = -totalWidth / 2f;
            Vector3[] positions = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                positions[i] = zoneCenter + perp * (startOffset + spacing * i);
                positions[i].y = bounds.center.y;
            }

            return positions;
        }

        private float GetZoneWidth(Bounds bounds, Vector3 perp) => Mathf.Abs(Vector3.Dot(bounds.size, perp));

        private BoxCollider GetSpawnZone(EDirection direction) => direction switch
        {
            EDirection.Up => spawnZoneUp,
            EDirection.Down => spawnZoneDown,
            EDirection.Left => spawnZoneLeft,
            EDirection.Right => spawnZoneRight,
            _ => null
        };

        private Vector3 GetPerpendicular(EDirection direction) => direction switch
        {
            EDirection.Up => Vector3.right,
            EDirection.Down => Vector3.right,
            EDirection.Left => Vector3.forward,
            EDirection.Right => Vector3.forward,
            _ => Vector3.right
        };

        // ─── Gizmos ───────────────────────────────────────────────────

        void OnDrawGizmosSelected()
        {
            DrawZoneGizmo(spawnZoneUp, Color.green);
            DrawZoneGizmo(spawnZoneDown, Color.red);
            DrawZoneGizmo(spawnZoneLeft, Color.blue);
            DrawZoneGizmo(spawnZoneRight, Color.yellow);
        }

        private void DrawZoneGizmo(BoxCollider zone, Color color)
        {
            if (zone == null) return;
            Gizmos.color = color;
            Gizmos.matrix = zone.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(zone.center, zone.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }

    [Serializable]
    public class DifficultyTier
    {
        [Tooltip("Score tối thiểu để kích hoạt tier này")]
        public int minScore;
        [Tooltip("Khoảng thời gian giữa 2 wave (giây)")]
        public float waveInterval;
        [Tooltip("Số enemy tối thiểu mỗi nhóm")]
        public int groupSizeMin;
        [Tooltip("Số enemy tối đa mỗi nhóm")]
        public int groupSizeMax;
    }
}