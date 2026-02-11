using System;
using UnityEngine;

namespace VoltaTwins
{
    public enum GameState
    {
        Initialize,
        Play,
        Paused,
        Win,
        Lose,
    }

    [DefaultExecutionOrder(-100)]
    public class VT_GameManager : MonoBehaviour
    {
        public static VT_GameManager Instance { get; private set; }

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
            Application.targetFrameRate = 120;
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
                default:
                    Time.timeScale = 1f;
                    break;
            }

            OnGameStateChanged?.Invoke(_state);
        }

        public GameState GetState() => _state;

        private void SetMaxUnlockedLevel(int level)
        {
            // Giới hạn level tối đa là 12
            int cappedLevel = Mathf.Min(level, 12);
            int current = GetMaxUnlockedLevel();
            if (cappedLevel > current)
            {
                PlayerPrefs.SetInt(VT_SafetyKey.MAX_UNLOCKED_LEVEL_KEY, cappedLevel);
                PlayerPrefs.Save();
            }
        }

        public int GetMaxUnlockedLevel()
        {
            int maxLevel = PlayerPrefs.GetInt(VT_SafetyKey.MAX_UNLOCKED_LEVEL_KEY, 1);
            // Đảm bảo không vượt quá 12
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

        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(VT_SafetyKey.MAX_UNLOCKED_LEVEL_KEY);
            PlayerPrefs.Save();
            SetMaxUnlockedLevel(1);
        }
    }
}