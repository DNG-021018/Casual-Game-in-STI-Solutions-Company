using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private enum CollisionResult { None, Obstacle, Gate, Teleport }

    [Header("Settings")]
    public float moveSpeed = 3f;
    public float gravity = 9.81f;
    [SerializeField] private float fallSoundY = -0.1f; // Gọi sound khi rớt tới Y này
    [SerializeField] private float fallDeathY = -5f; // Chết khi rớt tới Y này
    public PlayerAnimator animator;

    private bool isMoving, isFalling, isTeleporting;
    private float currentPosY;
    private float verticalVelocity;
    private Vector2 lastInput;

    private PlayerCollisionChecker collisionChecker;
    private ITileMapService tileMapService;
    public Action OnReachedGate;
    public Action OnIdle;
    public Action OnGameOver;
    public bool isReachedGate;
    private bool isDead = false;
    private bool hasPlayedFallSound = false;
    private bool justSpawned = true; 



    [Header("Sound Effects")]
    [SerializeField] private AudioClip teleportClip;
    [SerializeField] private AudioClip slideClip;
    [SerializeField] private AudioClip fallClip;


    void Start()
    {
        isReachedGate = false;
        currentPosY = 1.175f;
        collisionChecker = new PlayerCollisionChecker(transform, currentPosY);
    }


    public void Initialize(ITileMapService service, Action onIdle, Action onReachedGate, Action onGameOver)
    {
        tileMapService = service;
        OnReachedGate = onReachedGate;
        OnIdle = onIdle;
        OnGameOver = onGameOver;

        animator.SetState(PlayerStateAnimator.Idle);
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isMoving && !isTeleporting)
            HandleFalling();
    }

    public void ManualMove(Vector2 direction)
    {
        if (PlayerState() || direction == Vector2.zero) return;
        StartCoroutine(MoveOnce(direction));
    }

    public bool PlayerState() => isMoving || isFalling || isTeleporting;

    public Vector3Int GetNewTilePos(Vector2 input)
    {
        Vector3Int currentTilePos = tileMapService.WorldToCell(transform.position);
        Vector3Int direction = new Vector3Int(Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y));
        return currentTilePos + direction;
    }


    private void HandleFalling()
    {
        collisionChecker.UpdateCurrentPosY(currentPosY);

        bool hasGroundBelow = collisionChecker.IsGroundBelow() || collisionChecker.IsIceGroundBelow();

        if (!hasGroundBelow)
        {
            if (!isFalling)
            {
                isFalling = true;
                verticalVelocity = 0f;

                if (!justSpawned && !hasPlayedFallSound && !isDead && fallClip != null && SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(fallClip);
                    hasPlayedFallSound = true;
                }
            }

            verticalVelocity -= gravity * Time.deltaTime;
            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;

            if (transform.position.y < fallDeathY)
            {
                isDead = true;
                OnGameOver?.Invoke();
                return;
            }
        }
        else
        {
            if (transform.position.y < currentPosY)
            {
                transform.position = new Vector3(transform.position.x, currentPosY, transform.position.z);
                verticalVelocity = 0f;
            }

            isFalling = false;
            hasPlayedFallSound = false;

            if (justSpawned)
            {
                justSpawned = false;
            }
        }
    }


    private void RotateTowards(Vector2 input)
    {
        Vector3 direction = new Vector3(input.x, 0, input.y);
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.DORotateQuaternion(rotation, 0.2f);
    }

    private CollisionResult CheckCollision(Vector2 input)
    {
        if (collisionChecker.IsWallAheadBox(input)) return CollisionResult.Obstacle;
        if (collisionChecker.IsGateAheadBox(input)) return CollisionResult.Gate;
        if (collisionChecker.IsTeleportAheadBox(input)) return CollisionResult.Teleport;

        Debug.DrawRay(transform.position, new Vector3(input.x, 0, input.y), Color.green, 0.3f);
        return CollisionResult.None;
    }

    private Vector3 GetTileCenterWorldAtHeight(Vector3Int tilePos, float yHeight)
    {
        Vector3 center = tileMapService.GetCellCenterWorld(tilePos);
        return new Vector3(center.x, yHeight, center.z);
    }

    private IEnumerator MoveOnce(Vector2 input)
    {
        isMoving = true;
        lastInput = input;

        RotateTowards(input);

        CollisionResult result = CheckCollision(input);
        switch (result)
        {
            case CollisionResult.Obstacle:
                break;

            case CollisionResult.Gate:
                isReachedGate = true;
                OnReachedGate?.Invoke();
                break;

            case CollisionResult.Teleport:
            case CollisionResult.None:
                Vector3Int newTilePos = GetNewTilePos(input);
                yield return MoveToTile(newTilePos);

                Vector3Int finalPos = tileMapService.WorldToCell(transform.position);
                if (tileMapService.HasTeleportAt(finalPos))
                    yield return HandleTeleport(Vector2.zero);
                break;
        }

        isMoving = false;
        OnIdle?.Invoke();
    }

    private IEnumerator MoveToTile(Vector3Int tilePos)
    {
        Vector3 destination = GetTileCenterWorldAtHeight(tilePos, currentPosY);
        float duration = Vector3.Distance(transform.position, destination) / moveSpeed;

        yield return transform.DOMove(destination, duration).SetEase(Ease.Linear).WaitForCompletion();

        if (collisionChecker.IsIceGroundBelow())
        {
            CollisionResult iceSlideResult = CheckCollision(lastInput);
            if (iceSlideResult == CollisionResult.None || iceSlideResult == CollisionResult.Teleport)
            {
                yield return MoveSlide(lastInput);
            }
            else if (iceSlideResult == CollisionResult.Gate)
            {
                isReachedGate = true;
                OnReachedGate?.Invoke();
            }
        }
    }

    private IEnumerator MoveSlide(Vector2 input)
    {
        do
        {
            Vector3Int newTilePos = GetNewTilePos(input);
            if (slideClip != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(slideClip);
            }
            yield return MoveToTile(newTilePos);
        }
        while (collisionChecker.IsIceGroundBelow() &&
               CheckCollision(input) is CollisionResult.None or CollisionResult.Teleport);

        isMoving = false;
        OnIdle?.Invoke();
    }


    private IEnumerator HandleTeleport(Vector2 input)
    {
        isTeleporting = true;

        Vector3Int teleportTilePos = GetNewTilePos(input);
        if (tileMapService.HasTeleportAt(teleportTilePos))
        {
            TileMapPointTeleport teleportPoint = tileMapService.GetTeleportAt(teleportTilePos);
            Vector3Int destinationTilePos = tileMapService.WorldToCell(teleportPoint.toObject.position);
            Vector3 destination = GetTileCenterWorldAtHeight(destinationTilePos, currentPosY);

            if (teleportClip != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(teleportClip);
            }

            yield return transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).WaitForCompletion();
            transform.position = destination;
            yield return transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).WaitForCompletion();
        }
        else
        {
            Debug.LogWarning($"No teleport destination found for position {teleportTilePos}");
        }

        isTeleporting = false;
    }

}
