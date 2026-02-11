using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class SegmentManager : MonoBehaviour
{
    [Header("Segment Data")]
    public SegmentData segmentData;
    public Transform playerTransform;

    [Header("Spawn Points")]
    public Transform spawnPointOriginal;
    public Transform spawnPointMirror;

    private Transform segmentContainer;
    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools = new();
    private readonly Queue<ActiveSegment> activeSegments = new();
    [SerializeField] private SpawnPatternGroupList patternGroupList;

    private int segmentIndex = 0;
    private bool isRunning = true;
    private bool allowGateGroup = false;
    private bool isWin = false;

    public int totalSegmentsSpawned = 0;
    private const int maxSegmentsAllowed = 10;

    public static event Action OnSegmentsChanged;



    private class ActiveSegment
    {
        public GameObject instance;
        public GameObject prefab;
        public SegmentElement element;

        public ActiveSegment(GameObject instance, GameObject prefab)
        {
            this.instance = instance;
            this.prefab = prefab;
            this.element = instance.GetComponent<SegmentElement>();
        }
    }

    private void Awake()
    {
        segmentContainer ??= transform;

        foreach (var prefab in segmentData.GetAllPrefabs())
        {
            pools[prefab] = new ObjectPool<GameObject>(
                () => Instantiate(prefab, segmentContainer),
                obj => obj.SetActive(true),
                obj => obj.SetActive(false),
                obj => Destroy(obj),
                false,
                segmentData.preloadPerPrefab
            );

            // Preload
            for (int i = 0; i < segmentData.preloadPerPrefab; i++)
                pools[prefab].Release(pools[prefab].Get());
        }

        Debug.Log($"SegmentManager initialized with {pools.Count} pools.");
    }
    private void FixedUpdate()
    {
        if (!isRunning) return;

        float moveSpeed = isWin ? 50f : segmentData.moveSpeed;
        float moveStep = moveSpeed * Time.deltaTime;

        foreach (var seg in activeSegments)
        {
            seg.instance.transform.Translate(0, 0, -moveStep, Space.World);
        }

        RecycleSegments();
    }

    public IEnumerator InitializeSegmentsCoroutine()
    {
        yield return null;

        for (int i = 0; i < segmentData.segmentsOnScreen; i++)
            SpawnSegmentPair();

        Debug.Log($"Spawned {activeSegments.Count} segments on screen.");
    }

    private void SpawnSegmentPair()
    {
        if (totalSegmentsSpawned >= maxSegmentsAllowed)
        {
            Debug.Log("🟥 Đã đạt giới hạn spawn segment.");
            return;
        }

        var originalPrefab = segmentData.originalPrefabs[segmentIndex % segmentData.originalPrefabs.Length];
        var mirrorPrefab = segmentData.mirrorPrefabs[segmentIndex % segmentData.mirrorPrefabs.Length];
        segmentIndex++;


        var original = CreateSegment(originalPrefab, spawnPointOriginal, -3);
        var mirror = CreateSegment(mirrorPrefab, spawnPointMirror, 3);

        AssignPatternsToPair(original.element, mirror.element);

        activeSegments.Enqueue(original);
        activeSegments.Enqueue(mirror);

        totalSegmentsSpawned += 2;

        OnSegmentsChanged?.Invoke();
    }

    private ActiveSegment CreateSegment(GameObject prefab, Transform spawnPoint, float fallbackX)
    {
        var pool = pools[prefab];

        float spawnZ = activeSegments.Count > 0
            ? activeSegments.Last().instance.transform.position.z + segmentData.segmentSpacing
            : 0f;

        Vector3 basePos = spawnPoint != null
            ? spawnPoint.position
            : new Vector3(fallbackX, 0, 0); // fallback khi không có transform

        Vector3 spawnPos = new Vector3(basePos.x, basePos.y, spawnZ);

        var instance = pool.Get();
        instance.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        instance.transform.localScale = Vector3.one;

        return new ActiveSegment(instance, prefab);
    }


    private void RecycleSegments()
    {
        if (activeSegments.Count < 2 || totalSegmentsSpawned < maxSegmentsAllowed)
            return;

        // const float recycleThreshold = 3f; // có thể điều chỉnh (đơn vị: khoảng cách z)
        float playerZ = playerTransform.position.z;
        var original = activeSegments.Peek();
        var mirror = activeSegments.ElementAt(1);

        float segZ = original.instance.transform.position.z;
        float segmentEndZ = segZ + segmentData.segmentSpacing;
        float distanceBehindPlayer = playerZ - segmentEndZ;

        if (playerZ - (segZ + segmentData.segmentSpacing) > segmentData.segmentSpacing * 1.5f)
        {
            original = activeSegments.Dequeue();
            mirror = activeSegments.Dequeue();

            float lastZ = activeSegments.Count > 0
                ? activeSegments.Last().instance.transform.position.z
                : 0f;

            float newZ = lastZ + segmentData.segmentSpacing;

            original.instance.transform.position = new Vector3(original.instance.transform.position.x, 0, newZ);
            mirror.instance.transform.position = new Vector3(mirror.instance.transform.position.x, 0, newZ);

            ResetInitialSegment(original.element);
            ResetInitialSegment(mirror.element);

            AssignPatternsToPair(original.element, mirror.element);

            activeSegments.Enqueue(original);
            activeSegments.Enqueue(mirror);
        }
    }



    private void ResetInitialSegment(SegmentElement element)
    {
        if (element.isInitialSegment)
        {
            element.SetInitialSegment(false);
            element.AssignPatternSequenceAndReturn();
        }
    }

    private void AssignPatternsToPair(SegmentElement original, SegmentElement mirror)
    {
        var originalGroups = original.AssignPatternSequenceAndReturn();

        if (originalGroups?.Count == 3)
        {
            var mirrorGroups = MirrorPatternUtility.GenerateMirrorGroups(originalGroups, patternGroupList);
            mirror.AssignSpecificGroups(mirrorGroups);
        }
    }

    public void ReloadTotalSegments()
    {
        foreach (var pool in pools.Values)
            pool.Clear();

        activeSegments.Clear();
        totalSegmentsSpawned = 0;
        segmentIndex = 0;

        StartCoroutine(InitializeSegmentsCoroutine());
    }

    public void WinGame()
    {
        isWin = true;
        Debug.Log("🏆 Bạn đã thắng!");
    }

    public void StopAllSegments() => isRunning = false;

    [ContextMenu("🔍 Get Furthest Segment Pair")]
    public (SegmentElement original, SegmentElement mirror) GetFurthestSegmentPair()
    {
        if (activeSegments.Count < 2)
        {
            Debug.LogWarning("⚠️ Không đủ segment để lấy cặp xa nhất.");
            return (null, null);
        }

        ActiveSegment furthestOriginal = null, furthestMirror = null;
        float maxZ = float.MinValue;
        var segments = activeSegments.ToArray();

        for (int i = 0; i < segments.Length - 1; i += 2)
        {
            float avgZ = (segments[i].instance.transform.position.z + segments[i + 1].instance.transform.position.z) * 0.5f;
            if (avgZ > maxZ)
            {
                maxZ = avgZ;
                furthestOriginal = segments[i];
                furthestMirror = segments[i + 1];
            }
        }

        return (furthestOriginal?.element, furthestMirror?.element);
    }

    public void ForceGatePattern(SpawnPointGroup gateGroup)
    {
        var (original, mirror) = GetFurthestSegmentPair();
        if (original == null || mirror == null)
        {
            Debug.LogWarning("❌ Không tìm thấy cặp segment xa nhất.");
            return;
        }

        original.ClearAllPatterns();
        mirror.ClearAllPatterns();

        original.SpawnGateEndPatternAtLastOnly(gateGroup);
        mirror.SpawnGateEndPatternAtLastOnly(gateGroup.GetMirrored());

        allowGateGroup = true;
    }

    public SpawnPointGroup FindGateGroup(SpawnPatternGroupList list)
    {
        return list.allCombinations.FirstOrDefault(g =>
            g.left.category == ObstacleCategory.Alive &&
            g.center.category == ObstacleCategory.Alive && g.center.aliveType == AliveObstacleType.GateEnd &&
            g.right.category == ObstacleCategory.Alive);
    }
}
