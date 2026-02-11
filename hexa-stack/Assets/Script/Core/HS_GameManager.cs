using System;
using UnityEngine;

namespace HexaStack
{
    public enum GameState
    {
        Initialize,
        Play,
        Lose,
    }

    [DefaultExecutionOrder(-100)]
    public class HS_GameManager : MonoBehaviour
    {
        public static HS_GameManager Instance { get; private set; }

        [Header("Managers")]
        [SerializeField] private HS_StackSpawner stackSpawner;
        [SerializeField] private HS_MergeManager mergeManager;

        GameState _state = GameState.Initialize;
        int _currentScore = 0;
        int _highestScore = 0;

        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighScoreChanged;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;

            LoadHighScore();
        }

        void Start()
        {
            if (stackSpawner == null)
            {
                stackSpawner = FindFirstObjectByType<HS_StackSpawner>();
            }

            if (mergeManager == null)
            {
                mergeManager = FindFirstObjectByType<HS_MergeManager>();
            }

            SetState(GameState.Initialize);
        }

        public void StartGame()
        {
            SetState(GameState.Play);
            if (stackSpawner != null)
            {
                stackSpawner.ClearAllStacks();
                stackSpawner.SpawnInitialStacks();
            }

            if (mergeManager != null)
            {
                mergeManager.ResetMerge();
            }

            _currentScore = 0;
            OnScoreChanged?.Invoke(_currentScore);

        }

        public void ResetGame()
        {
            if (stackSpawner != null)
            {
                stackSpawner.ClearAllStacks();
            }

            if (mergeManager != null)
            {
                mergeManager.ResetMerge();
            }

            _currentScore = 0;
            OnScoreChanged?.Invoke(_currentScore);

            SetState(GameState.Initialize);
        }


        public void SetState(GameState s)
        {
            if (_state == s) return;
            _state = s;

            switch (_state)
            {
                case GameState.Initialize:
                    break;
                case GameState.Play:
                    break;
                case GameState.Lose:
                    SaveHighScore();
                    break;
            }

            OnGameStateChanged?.Invoke(_state);
        }

        public GameState GetState() => _state;

        public void AddScore(int points)
        {
            _currentScore += points;
            OnScoreChanged?.Invoke(_currentScore);
        }
        public int GetScore() => _currentScore;
        public int GetHighestScore() => _highestScore;

        public void SaveHighScore()
        {
            if (_currentScore > _highestScore)
            {
                _highestScore = _currentScore;
                PlayerPrefs.SetInt(HS_SafetyKey.HIGH_SCORE_KEY, _highestScore);
                PlayerPrefs.Save();
                OnHighScoreChanged?.Invoke(_highestScore);
            }
        }

        void LoadHighScore()
        {
            _highestScore = PlayerPrefs.GetInt(HS_SafetyKey.HIGH_SCORE_KEY, 0);
            OnHighScoreChanged?.Invoke(_highestScore);
        }
    }
}
