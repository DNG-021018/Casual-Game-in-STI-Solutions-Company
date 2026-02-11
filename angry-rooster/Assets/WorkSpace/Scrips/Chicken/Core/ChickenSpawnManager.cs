using UnityEngine;
using UnityEngine.AI;

public class ChickenSpawnManager : MonoBehaviour
{
    private static ChickenSpawnManager _instance;
    public static ChickenSpawnManager Instance => _instance;

    public GameObject prefabToSpawn;
    public int spawnAmount;
    public Vector2 spawnAreaSize = new Vector2(10f, 5f);

    [Tooltip("Maximum attempts to find a valid spawn position within the NavMesh.")]
    public int maxAttempts = 30;
    public Color gizmoColor = new Color(1, 1, 0, 0.2f);

    private InGame _inGame;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        _inGame = FindFirstObjectByType<InGame>();
    }

    void Start()
    {
        spawnAmount = _inGame.currentLevelGame.ChickenSpawn;
        for (int i = 0; i < spawnAmount; i++)
        {
            SpawnChicken();
        }
    }

    public void SpawnChicken()
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPos = GetRandomPointInRectangle();

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                Instantiate(prefabToSpawn, hit.position, Quaternion.identity);
                return;
            }
        }
        Debug.LogWarning("Could not find valid position in NavMesh.");
    }

    public void HideAllChicken()
    {
        ChickenController[] chicken = FindObjectsOfType<ChickenController>();

        foreach (var c in chicken)
        {
            c.gameObject.SetActive(false);
        }
    }

    Vector3 GetRandomPointInRectangle()
    {
        Vector3 center = transform.position;
        float halfWidth = spawnAreaSize.x / 2f;
        float halfDepth = spawnAreaSize.y / 2f;

        float x = Random.Range(center.x - halfWidth, center.x + halfWidth);
        float z = Random.Range(center.z - halfDepth, center.z + halfDepth);

        return new Vector3(x, center.y, z);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y);
        Gizmos.DrawCube(center, size);
    }
}
