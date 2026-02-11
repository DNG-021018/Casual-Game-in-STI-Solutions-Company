using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SegmentElement : MonoBehaviour
{
    [Header("Danh sách 3 Pattern Element trong segment")]
    public List<PatternElement> patternElements = new List<PatternElement>(3);

    [Header("Danh sách các SpawnPointGroup hợp lệ")]
    public SpawnPatternGroupList spawnPatternGroupList;

    [Header("Có phải segment đầu không?")]
    public bool isInitialSegment = false;

    [Header("Khác")]
    public bool allowGateGroup = false;
    public bool isMirror = false;
    public bool isLevel1 = false;

    private List<SpawnPointGroup> currentGroups = new List<SpawnPointGroup>(3);

    public List<SpawnPointGroup> AssignPatternSequenceAndReturn()
    {
        if (!IsValidForAssignment()) return new List<SpawnPointGroup>();

        var groups = isLevel1 ? GetLevel1Groups() : GetDefaultGroups();

        if (groups.Count < 3)
        {
            return new List<SpawnPointGroup>();
        }

        currentGroups = groups;

        if (isInitialSegment)
            return null; // Segment đầu không spawn obstacle

        for (int i = 0; i < patternElements.Count; i++)
        {
            patternElements[i].SpawnObstacles(currentGroups[i]);
        }

        return currentGroups;
    }

    private List<SpawnPointGroup> GetLevel1Groups()
    {
        float r = Random.value;

        if (r < 0.8f)
        {
            var filtered = GetFilteredGroups(g =>
                !g.isGateCombo || allowGateGroup,
                g => CountAliveNone(g) == 2 && !HasAliveItem(g)
            );

            if (filtered.Count >= 3)
                return filtered.Take(3).ToList();

            return GetFilteredGroups(g =>
                !g.isGateCombo || allowGateGroup,
                g => !HasAliveItem(g)
            ).Take(3).ToList();
        }
        else
        {
            return GetFilteredGroups(g =>
                !g.isGateCombo || allowGateGroup
            ).Take(3).ToList();
        }
    }

    private List<SpawnPointGroup> GetDefaultGroups()
    {
        return GetFilteredGroups(
            g => !g.isGateCombo || allowGateGroup,
            g => !HasAliveItem(g)
        ).Take(3).ToList();
    }

    private List<SpawnPointGroup> GetFilteredGroups(params System.Func<SpawnPointGroup, bool>[] predicates)
    {
        return spawnPatternGroupList.allCombinations
            .Where(g => predicates.All(p => p(g)))
            .OrderBy(_ => Random.value)
            .ToList();
    }

    private int CountAliveNone(SpawnPointGroup g)
    {
        int count = 0;
        if (IsAliveNone(g.left)) count++;
        if (IsAliveNone(g.center)) count++;
        if (IsAliveNone(g.right)) count++;
        return count;
    }

    private bool IsAliveNone(SpawnPointData data)
    {
        return data.category == ObstacleCategory.Alive && data.aliveType == AliveObstacleType.None;
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

    public void AssignSpecificGroups(List<SpawnPointGroup> groups)
    {
        if (groups == null || groups.Count != 3 || patternElements.Count != 3)
        {
            return;
        }

        for (int i = 0; i < patternElements.Count; i++)
        {
            patternElements[i].SpawnObstacles(groups[i]);
        }
    }

    public void ClearAllPatterns()
    {
        foreach (var element in patternElements)
        {
            element.ClearSpawnedObstacles();
        }
    }

    public void SetInitialSegment(bool isInitial)
    {
        isInitialSegment = isInitial;
        if (isInitial)
            ClearAllPatterns();
    }

    public void SpawnGateEndPatternAtLastOnly(SpawnPointGroup gateGroup)
    {
        if (patternElements.Count != 3)
        {
            return;
        }

        patternElements[2].ClearSpawnedObstacles();
        patternElements[2].SpawnObstacles(gateGroup);
    }

    private bool IsValidForAssignment()
    {
        if (spawnPatternGroupList == null || spawnPatternGroupList.allCombinations.Count < 3)
        {
            return false;
        }

        if (patternElements.Count != 3)
        {
            return false;
        }

        return true;
    }
}
