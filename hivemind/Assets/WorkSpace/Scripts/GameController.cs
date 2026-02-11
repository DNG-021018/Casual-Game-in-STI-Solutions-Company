using UnityEngine;

public class GameController : MonoBehaviour
{
    LevelController levelController;
    TileMapController tileMapController;
    public PlayerManager playerManager;
    InGame inGame;

    void Start()
    {
        levelController = LevelController.Instance;
        levelController.OnSpawnEnd += OnMapLoaded;
        playerManager.OnGameOver += OnEndGame;
        if (tileMapController == null) tileMapController = FindFirstObjectByType<TileMapController>();
        inGame = FindFirstObjectByType<InGame>();
    }

    private void OnDestroy()
    {
        if (levelController != null)
            levelController.OnSpawnEnd -= OnMapLoaded;
    }

    private void OnMapLoaded(LevelDetail levelDetail)
    {
        InitializeGameLevel(levelDetail);
    }

    [ContextMenu("Initialize Game Level")]
    private void InitializeGameLevel(LevelDetail levelDetail)
    {
        if (tileMapController != null && levelController.tileMapBaseOnMapData != null)
        {
            tileMapController.InitializeTileMapGround(levelController.tileMapBaseOnMapData);
            tileMapController.InitializeSpawnPoints(levelDetail.spawnPoint1, levelDetail.spawnPoint2);
        }
        if (playerManager != null)
        {
            playerManager.InitializePlayer();
        }
    }

    public void OnEndGame(bool isWin)
    {
        if (isWin)
        {
            inGame.OnLevelCompleted(true);
        }
        else
        {
            inGame.OnLevelCompleted(false);
        }
    }
}