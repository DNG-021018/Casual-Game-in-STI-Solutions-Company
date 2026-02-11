using UnityEngine;

public class HoeSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject hoePrefab;
    private int maxHoes;
    private InGame inGame;
    
    [Header("Spawn Area")]
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 10f, 10f);
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
    [SerializeField] private GameObject container;
    
    [Header("No-Spawn Zone")]
    [SerializeField] private bool useNoSpawnZone = false;
    [SerializeField] private Vector3 noSpawnZoneSize = new Vector3(3f, 3f, 3f);
    [SerializeField] private Vector3 noSpawnZoneCenter = Vector3.zero;


    void Awake()
    {
        inGame = FindFirstObjectByType<InGame>();
    }

    void Start()
    {
        SetUpQuantityHoes(inGame.currentLevelGame);
    }

    void SetUpQuantityHoes(LevelGame levelGame)
    {
        maxHoes = levelGame.HoeSpawn;
        SpawnAllHoes();
    }

    void SpawnAllHoes()
    {
        Debug.Log("Spawning " + maxHoes + " hoes");
        for (int i = 0; i < maxHoes; i++)
        {
            SpawnHoe();
        }
    }

    void SpawnHoe()
    {
        Instantiate(hoePrefab, GetRandomSpawnPosition(), Quaternion.identity, container.transform);
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        Vector3 position;
        int attempts = 0;
        int maxAttempts = 50;
        
        do
        {
            float x = Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, 
                                  spawnAreaCenter.x + spawnAreaSize.x / 2);
            float y = Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2, 
                                  spawnAreaCenter.y + spawnAreaSize.y / 2);
            float z = Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2, 
                                  spawnAreaCenter.z + spawnAreaSize.z / 2);
            
            position = new Vector3(x, y, z);
            attempts++;
            
        } while (useNoSpawnZone && IsInNoSpawnZone(position) && attempts < maxAttempts);
        
        return position;
    }
    
    bool IsInNoSpawnZone(Vector3 position)
    {
        Vector3 distance = position - noSpawnZoneCenter;
        
        return Mathf.Abs(distance.x) <= noSpawnZoneSize.x / 2 &&
               Mathf.Abs(distance.y) <= noSpawnZoneSize.y / 2 &&
               Mathf.Abs(distance.z) <= noSpawnZoneSize.z / 2;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
        
        if (useNoSpawnZone)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(noSpawnZoneCenter, noSpawnZoneSize);
        }
    }

    void OnDestroy()
    {
        LevelPanel.OnStartGame -= SetUpQuantityHoes;
    }
}
