using System;
using UnityEngine;

public enum GameState
{
    Initialize,
    Playing,
    Paused,
    Setup,
    Win,
    Lose,
    Shooting
}

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_GameManager : MonoBehaviour
    {
        public static Wja8YNiR_GameManager Instance { get; private set; }

        public int currentLevel { get; set; }
        GameState _state = GameState.Initialize;

        public event Action<GameState> OnGameStateChanged;

        // Key để lưu level cao nhất đã mở
        private const string MAX_UNLOCKED_LEVEL_KEY = "MaxUnlockedLevel";

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
        }

        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            SetState(GameState.Initialize);

            // Đảm bảo level 1 luôn được mở
            if (GetMaxUnlockedLevel() < 1)
            {
                SetMaxUnlockedLevel(1);
            }
        }

        public void SetState(GameState s)
        {
            if (_state == s) return;
            _state = s;
            switch (_state)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.Win:
                    UnlockNextLevel(currentLevel);
                    break;
                default:
                    break;
            }
            OnGameStateChanged?.Invoke(_state);
        }

        public GameState GetState() => _state;

        public int GetMaxUnlockedLevel()
        {
            return PlayerPrefs.GetInt(MAX_UNLOCKED_LEVEL_KEY, 1);
        }

        private void SetMaxUnlockedLevel(int level)
        {
            int current = GetMaxUnlockedLevel();
            if (level > current)
            {
                PlayerPrefs.SetInt(MAX_UNLOCKED_LEVEL_KEY, level);
                PlayerPrefs.Save();
            }
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
            PlayerPrefs.DeleteKey(MAX_UNLOCKED_LEVEL_KEY);
            PlayerPrefs.Save();
            SetMaxUnlockedLevel(1);
        }
    }
}