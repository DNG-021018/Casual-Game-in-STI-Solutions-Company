using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MirrorSegmentController : MonoBehaviour
{
    [Header("Danh sách các đoạn (segment) bên trái - gốc")]
    public List<SegmentElement> originalTracks = new List<SegmentElement>();

    [Header("Danh sách các đoạn (segment) bên phải - đảo ngược")]
    public List<SegmentElement> mirrorTracks = new List<SegmentElement>();

    [SerializeField] private SegmentManager manager;
    [SerializeField] private SpawnPatternGroupList patternGroupList;

    public IEnumerator AutoFindSegmentsCoroutine()
    {
        yield return null;
        FindSegmentsInScene();
        yield return StartGenerate();
    }

    public IEnumerator StartGenerate()
    {
        yield return null;
        GenerateAllMirroredPatterns();
    }

    [ContextMenu("🔍 Auto Find Segments In Scene")]
    public void FindSegmentsInScene()
    {
        var allSegments = FindObjectsOfType<SegmentElement>();

        originalTracks = allSegments
            .Where(s => !s.isMirror)
            .OrderBy(s => s.transform.position.z)
            .ToList();

        mirrorTracks = allSegments
            .Where(s => s.isMirror)
            .OrderBy(s => s.transform.position.z)
            .ToList();
    }

    [ContextMenu("🌀 Generate Pattern For All Pairs")]
    public void GenerateAllMirroredPatterns()
    {
        if (originalTracks.Count != mirrorTracks.Count)
        {
            return;
        }

        for (int i = 0; i < originalTracks.Count; i++)
        {
            var original = originalTracks[i];
            var mirror = mirrorTracks[i];

            if (original == null || mirror == null)
            {
                continue;
            }

            var originalGroups = original.AssignPatternSequenceAndReturn();
            if (originalGroups == null || originalGroups.Count != 3)
            {
                continue;
            }

            var mirrorGroups = GenerateMirrorGroups(originalGroups);
            mirror.AssignSpecificGroups(mirrorGroups);
        }
    }



    [ContextMenu("🧩 Force GateEnd Segment")]
    public void ForceGateEndNow()
    {
        if (manager == null || patternGroupList == null)
        {
            return;
        }

        var gateGroup = manager.FindGateGroup(patternGroupList);
        if (gateGroup == null)
        {
            return;
        }

        manager.ForceGatePattern(gateGroup);
    }

    private List<SpawnPointGroup> GenerateMirrorGroups(List<SpawnPointGroup> originalGroups)
    {
        return originalGroups.Select(original =>
        {
            if (original.isGateCombo)
                return null;

            bool allowItem = Random.value < 0.2f;

            var mirrorCandidates = patternGroupList.allCombinations
                .Where(g =>
                    g != original &&
                    !g.isGateCombo &&
                    (allowItem || !HasAliveItem(g)) &&
                    HasAtLeastOneMirroredAliveMatch(g, original)
                )
                .OrderBy(_ => Random.value)
                .ToList();

            return mirrorCandidates.FirstOrDefault() ?? original.GetMirrored();

        }).ToList();
    }


    private bool HasAtLeastOneMirroredAliveMatch(SpawnPointGroup a, SpawnPointGroup b)
    {
        return IsAliveMatch(a.left, b.right) ||
               IsAliveMatch(a.center, b.center) ||
               IsAliveMatch(a.right, b.left);
    }

    private bool IsAliveMatch(SpawnPointData a, SpawnPointData b)
    {
        if (a.category != ObstacleCategory.Alive || b.category != ObstacleCategory.Alive)
            return false;

        // Có thể mở rộng thêm if (a.aliveType == b.aliveType || ...)
        return a.aliveType == b.aliveType;
    }

    private bool HasAliveItem(SpawnPointGroup g)
    {
        return HasItem(g.left) || HasItem(g.center) || HasItem(g.right);
    }

    private bool HasItem(SpawnPointData data)
    {
        return data.category == ObstacleCategory.Alive &&
               data.aliveType == AliveObstacleType.ItemInvisible;
    }
}


public static class MirrorPatternUtility
{
    public static List<SpawnPointGroup> GenerateMirrorGroups(
        List<SpawnPointGroup> originalGroups,
        SpawnPatternGroupList patternGroupList)
    {
        return originalGroups.Select(original =>
        {
            if (original.isGateCombo)
                return null;

            bool allowItem = UnityEngine.Random.value < 0.2f;

            var mirrorCandidates = patternGroupList.allCombinations
                .Where(g =>
                    g != original &&
                    !g.isGateCombo &&
                    (allowItem || !HasAliveItem(g)) &&
                    HasAtLeastOneMirroredAliveMatch(g, original)
                )
                .OrderBy(_ => UnityEngine.Random.value)
                .ToList();

            return mirrorCandidates.FirstOrDefault() ?? original.GetMirrored();
        }).ToList();
    }

    private static bool HasAliveItem(SpawnPointGroup g)
    {
        return HasItem(g.left) || HasItem(g.center) || HasItem(g.right);
    }

    private static bool HasItem(SpawnPointData data)
    {
        return data.category == ObstacleCategory.Alive &&
               data.aliveType == AliveObstacleType.ItemInvisible;
    }

    private static bool HasAtLeastOneMirroredAliveMatch(SpawnPointGroup a, SpawnPointGroup b)
    {
        return IsAliveMatch(a.left, b.right) ||
               IsAliveMatch(a.center, b.center) ||
               IsAliveMatch(a.right, b.left);
    }

    private static bool IsAliveMatch(SpawnPointData a, SpawnPointData b)
    {
        return a.category == ObstacleCategory.Alive &&
               b.category == ObstacleCategory.Alive &&
               a.aliveType == b.aliveType;
    }
}
