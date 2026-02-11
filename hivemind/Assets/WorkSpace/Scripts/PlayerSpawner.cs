using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform playerParent;
    public PlayerController PlayerAPrefab;
    public PlayerController PlayerBPrefab;

    public void Initialize(PlayerManager playerManager)
    {
        SpawnPlayer(PlayerAPrefab, playerManager.tileMapController.spawnPoint1.position, playerManager);
        SpawnPlayer(PlayerBPrefab, playerManager.tileMapController.spawnPoint2.position, playerManager);
    }

    private void SpawnPlayer(PlayerController prefab, Vector3 spawnPosition, PlayerManager playerManager)
    {
        PlayerController playerController = Instantiate(prefab, spawnPosition, Quaternion.identity, playerParent);
        playerController.Initialize(playerManager.tileMapService, playerManager.SetAllPlayersIdle, playerManager.CheckPlayerReachedGate, playerManager.OnHandleFalling);
        playerManager.AddPlayer(playerController);
    }
}
