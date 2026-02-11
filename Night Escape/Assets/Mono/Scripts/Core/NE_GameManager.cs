using System;
using UnityEngine;

namespace NightEscape
{
    public enum GameState
    {
        Initialize,
        InitializeLevel,
        Play,
        Paused,
        Win,
        Lose
    }

    [DefaultExecutionOrder(-200)]
    public class NE_GameManager : MonoBehaviour
    {
        public static NE_GameManager Instance { get; private set; }

        public int CurrentLevel { get; set; }
        GameState _state = GameState.Initialize;

        [Header("Levels Settings")]
        [SerializeField] private int levelsCount = 18;

        [Header("Cooldown Settings")]
        [SerializeField] private float cooldownDuration = 90f;
        private float _cooldownTimer;
        private bool _isCooldownActive;

        [Header("Clip")]
        [SerializeField] AudioClip winClip;
        [SerializeField] AudioClip LoseClip;

        private bool _isGameFinished = false;

        public event Action<GameState> OnGameStateChanged;
        public event Action<float> OnCooldownTick;
        public event Action OnCooldownFinished;

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

        void Update()
        {
            if (_isCooldownActive)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0)
                {
                    _cooldownTimer = 0;
                    _isCooldownActive = false;
                    OnCooldownFinished?.Invoke();
                    if (!_isGameFinished)
                    {
                        FinishGame(GameState.Lose);
                    }
                }
                OnCooldownTick?.Invoke(_cooldownTimer);
            }
        }

        public void Initialize()
        {
            SetState(GameState.Initialize);
        }

        public void FinishGame(GameState finishState)
        {
            if (_isGameFinished)
            {
                return;
            }

            if (finishState != GameState.Win && finishState != GameState.Lose)
            {
                return;
            }

            OnCooldownFinished = null;
            _isGameFinished = true;
            SetState(finishState);
        }

        public bool IsGameFinished()
        {
            return _isGameFinished;
        }

        public void SetState(GameState s)
        {
            if (_state == s) return;
            _state = s;
            NE_AudioManager.Instance.SetBgmVolume(1f);
            switch (_state)
            {
                case GameState.Initialize:
                case GameState.Play:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.Win:
                    StopCooldown();
                    NE_AudioManager.Instance.SetBgmVolume();
                    NE_AudioManager.Instance.PlaySfx(winClip, 1, () => NE_AudioManager.Instance.PlayBgm());
                    Time.timeScale = 1f;
                    UnlockNextLevel(CurrentLevel);
                    break;
                case GameState.Lose:
                    StopCooldown();
                    NE_AudioManager.Instance.SetBgmVolume();
                    NE_AudioManager.Instance.PlaySfx(LoseClip, 1, () => NE_AudioManager.Instance.PlayBgm());
                    Time.timeScale = 1f;
                    break;
                default:
                    Time.timeScale = 1f;
                    break;
            }

            OnGameStateChanged?.Invoke(_state);
        }

        public GameState GetState() => _state;

        public void GameStart()
        {
            _isGameFinished = false;
            SetState(GameState.Play);
            StartCooldown();
        }

        public void StartCooldown()
        {
            _cooldownTimer = cooldownDuration;
            _isCooldownActive = true;
        }

        public void StopCooldown()
        {
            _isCooldownActive = false;
        }

        public bool IsCooldownActive()
        {
            return _isCooldownActive;
        }

        public float GetCooldownRemaining()
        {
            return _isCooldownActive ? _cooldownTimer : 0f;
        }

        public float GetCooldownDuration()
        {
            return cooldownDuration;
        }

        public void SetCooldownDuration(float duration)
        {
            cooldownDuration = duration;
        }

        private void SetMaxUnlockedLevel(int level)
        {
            int cappedLevel = Mathf.Min(level, levelsCount);
            int current = GetMaxUnlockedLevel();
            if (cappedLevel > current)
            {
                PlayerPrefs.SetInt(NE_SafetyKey.MAX_UNLOCKED_LEVEL_KEY, cappedLevel);
                PlayerPrefs.Save();
            }
        }

        public int GetMaxUnlockedLevel()
        {
            int maxLevel = PlayerPrefs.GetInt(NE_SafetyKey.MAX_UNLOCKED_LEVEL_KEY, 1);
            return Mathf.Min(maxLevel, levelsCount);
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

        public string GetLevelSceneName(int level)
        {
            if (level < 1 || level > levelsCount)
            {
                return null;
            }
            return $"Level_{level}";
        }

        public void LoadLevelScene(int level)
        {
            if (level < 1 || level > levelsCount)
            {
                return;
            }

            if (!IsLevelUnlocked(level))
            {
                return;
            }

            CurrentLevel = level;

            string sceneName = GetLevelSceneName(level);
            NE_LoadingScreenRoot.Instance.LoadScene(sceneName);
        }

        public bool CheckNextLevelInvalid()
        {
            int nextLevel = CurrentLevel + 1;
            return nextLevel > levelsCount;
        }
    }
}
