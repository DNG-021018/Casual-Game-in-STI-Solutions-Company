using System;
using UnityEngine;

public class GamePlayController : MonoBehaviour
{
    private static GamePlayController _instance;
    public static GamePlayController Instance => _instance;

    [Header("References")]
    [SerializeField] private EggSpawner _eggSpawner;
    private Character _character;

    [Header("Values")]
    private int _score;
    private int _targetScore;
    private int _limitTime;
    private bool _isLevelCompleted = false;

    public event Action<int> OnScoreChanged;
    public event Action<bool> OnLevelCompleted;
    public bool IsLevelCompleted => _isLevelCompleted;
    private InGame inGameUI;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void Start()
    {
        _score = 0;
        //LevelPanel.OnStartGame += SetUpEgg;
        inGameUI = FindFirstObjectByType<InGame>();
        _eggSpawner.OnEggPickupScore += OnGetScore;
        _eggSpawner.OnEggTargetChange += OnEggTargetChanged;
        _character = FindAnyObjectByType<Character>();
        _character.OnGetCatch += OnCharacterGetCatch;
        SetUpEgg();
        // Subscribe to time up event from InGame UI
        InGame.OnTimeUp += OnTimeUpHandler;
    }

    private void SetUpEgg()
    {
        _targetScore = inGameUI.currentLevelGame.RequiredScore;
        _limitTime = inGameUI.currentLevelGame.TimeLimit;
    }

    private void OnCharacterGetCatch()
    {
        if (!_isLevelCompleted)
        {
            _isLevelCompleted = true;
            OnLevelCompleted?.Invoke(false);
        }
    }

    private void OnTimeUpHandler()
    {
        if (!_isLevelCompleted)
        {
            Debug.Log("Time limit reached! Game Over.");
            _isLevelCompleted = true;
            OnLevelCompleted?.Invoke(false);
        }
    }

    private void OnEggTargetChanged(Egg egg)
    {
        _character.ChangeEggTarget(egg);
    }

    public void OnGetScore()
    {
        if (_isLevelCompleted)
        {
            Debug.LogWarning("Level is already completed. Cannot score more.");
            return;
        }
        _score += 1;
        Debug.Log($"Score Updated: {_score}");
        OnScoreChanged?.Invoke(_score);

        // int _nextSpawnThresholdIndex = 1;
        // int threshold = (_targetScore * _nextSpawnThresholdIndex) / 2;
        // if (_score >= threshold && _nextSpawnThresholdIndex <= 2)
        // {
        //     ChickenSpawnManager.Instance?.SpawnChicken();
        //     _nextSpawnThresholdIndex++;
        // }

        if (_score == _targetScore)
        {
            Debug.Log("Level Completed!");
            _isLevelCompleted = true;
            OnLevelCompleted?.Invoke(true);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
        // Unsubscribe from events
        InGame.OnTimeUp -= OnTimeUpHandler;
        OnScoreChanged = null;
        OnLevelCompleted = null;
    }
}