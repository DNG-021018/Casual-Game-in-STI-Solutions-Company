using System;
using UnityEngine;

namespace VertiblockPass
{
    public enum GameState
    {
        Initialize,
        InitializeLevel,
        Play,
        Paused,
        Win,
        Lose,
    }

    [DefaultExecutionOrder(-1000)]
    public class VP_GameManager : MonoBehaviour
    {
        public static VP_GameManager Instance { get; private set; }

        public int currentLevel { get; set; }
        GameState _state = GameState.Initialize;

        public event Action<GameState> OnGameStateChanged;

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
            }

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 240;
        }

        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            SetState(GameState.Initialize);
        }

        public void SetState(GameState s)
        {
            if (_state == s) return;
            _state = s;
            switch (_state)
            {
                case GameState.Initialize:
                case GameState.InitializeLevel:
                case GameState.Play:
                case GameState.Lose:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.Win:
                    Time.timeScale = 1f;
                    UnlockNextLevel(currentLevel);
                    break;
            }

            OnGameStateChanged?.Invoke(_state);
        }

        public GameState GetState() => _state;

        private void SetMaxUnlockedLevel(int level)
        {
            int cappedLevel = Mathf.Min(level, 12);
            int current = GetMaxUnlockedLevel();
            if (cappedLevel > current)
            {
                PlayerPrefs.SetInt(VP_SafetyKey.MAX_UNLOCKED_LEVEL_KEY, cappedLevel);
                PlayerPrefs.Save();
            }
        }

        public int GetMaxUnlockedLevel()
        {
            int maxLevel = PlayerPrefs.GetInt(VP_SafetyKey.MAX_UNLOCKED_LEVEL_KEY, 1);
            return Mathf.Min(maxLevel, 12);
        }

        public void UnlockNextLevel(int completedLevel)
        {
            int nextLevel = completedLevel + 1;
            SetMaxUnlockedLevel(nextLevel);
        }

        public bool IsLevelUnlocked(int level)
        {
            return level <= GetMaxUnlockedLevel();
        }
    }
}
