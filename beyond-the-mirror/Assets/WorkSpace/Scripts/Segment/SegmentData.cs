using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SegmentData", menuName = "Game/SegmentData")]
public class SegmentData : ScriptableObject
{
    public GameObject[] originalPrefabs;
    public GameObject[] mirrorPrefabs;

    public int preloadPerPrefab = 3;
    public int segmentsOnScreen = 6;
    public float moveSpeed = 10f;
    public float segmentSpacing = 20f;
    public int timeLimit = 30; // Time limit in seconds

    public GameObject[] GetAllPrefabs() => originalPrefabs.Concat(mirrorPrefabs).ToArray();

    public GameObject GetRandomOriginalPrefab() =>
        originalPrefabs[UnityEngine.Random.Range(0, originalPrefabs.Length)];

    public GameObject GetRandomMirrorPrefab() =>
        mirrorPrefabs[UnityEngine.Random.Range(0, mirrorPrefabs.Length)];
}
