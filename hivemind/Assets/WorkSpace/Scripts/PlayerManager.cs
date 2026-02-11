using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("References")]
    public TileMapController tileMapController;
    public PlayerSpawner playerSpawner;

    [Header("Sound Effects")]
    public AudioClip swipeClip;


    [Header("Players")]
    public List<PlayerController> playerControllers = new();
    public ITileMapService tileMapService;

    private SwipeInput swipeInput;
    public enum GameState { Playing, CheckWinningCondition, Win, Lose }
    public GameState currentGameState = GameState.Playing;
    public bool IsGameOver => currentGameState != GameState.Playing;
    private int totalReachedGate = 0;

    public event Action<bool> OnGameOver;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        swipeInput = new SwipeInput();
    }

    void Update()
    {
        swipeInput.HandleSwipeInput();
    }

    public void InitializePlayer()
    {
        tileMapController ??= FindFirstObjectByType<TileMapController>();
        tileMapService ??= FindFirstObjectByType<TileMapService>();

        if (tileMapService == null)
            Debug.LogError("TileMapService is not assigned or doesn't implement ITileMapService");

        SpawnPlayer();
    }

    public void SpawnPlayer() => playerSpawner.Initialize(this);

    public void AddPlayer(PlayerController playerController)
    {
        playerControllers.Add(playerController);
    }

    public void MoveAllPlayers(Vector2 direction)
    {
        if (IsGameOver || direction == Vector2.zero)
            return;
        foreach (PlayerController playerInfo in playerControllers)
            playerInfo.animator.Moving();

        foreach (PlayerController playerInfo in playerControllers)
            playerInfo.ManualMove(direction);
    }

    public bool AreAllPlayersIdle()
    {
        foreach (PlayerController playerInfo in playerControllers)
        {
            if (playerInfo.PlayerState()) return false; // Nếu còn player đang đi hoặc trượt
        }
        return true;
    }

    public void SetAllPlayersIdle()
    {
        foreach (PlayerController playerInfo in playerControllers)
        {
            playerInfo.animator.Idle();
        }
    }

    public void CheckPlayerReachedGate()
    {
        totalReachedGate++;
        StartCoroutine(DelayCheckPlayerReachedGate());
    }

    public IEnumerator DelayCheckPlayerReachedGate()
    {
        // ⏳ Chờ cho tất cả player dừng di chuyển (bao gồm cả trượt băng)
        yield return new WaitUntil(() => AreAllPlayersIdle());
        foreach (PlayerController playerInfo in playerControllers)
        {
            Debug.Log(playerInfo.PlayerState()); // Đảm bảo tất cả player đã dừng)
        }

        // ✅ Sau khi tất cả đã đứng yên, mới check gate
        CheckPlayerGate();
        OnPlayerAnimationEnd();
    }

    public void OnHandleFalling()
    {
        currentGameState = GameState.Lose;
        OnPlayerAnimationEnd();
    }

    public void CheckPlayerGate()
    {
        if (currentGameState != GameState.Playing) return;

        currentGameState = GameState.CheckWinningCondition;
        foreach (PlayerController playerInfo in playerControllers)
        {
            if (playerInfo.isReachedGate == false)
            {
                currentGameState = GameState.Lose;
                break;
            }
            else
            {
                currentGameState = GameState.Win;
            }
        }
    }

    public void OnPlayerAnimationEnd()
    {
        if (currentGameState == GameState.Win)
        {
            AnimationWin();
            Debug.Log("All players reached the gate. Game Over: Win");
            OnGameOver?.Invoke(true);
        }
        else
        {
            AnimationLose();
            Debug.Log("Not all players reached the gate. Game Over: Lose");
            OnGameOver?.Invoke(false);
        }
    }
    public void AnimationWin()
    {
        foreach (PlayerController playerInfo in playerControllers)
        {
            playerInfo.animator.Win();
        }
    }
    public void AnimationLose()
    {
        foreach (PlayerController playerInfo in playerControllers)
        {
            playerInfo.animator.Die();
        }
    }
}