using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bowmancer
{
    public enum GameState
    {
        None,
        Initialize,
        Ready,
        Play,
        Paused,
        PickupUpgrade,
        Win,
        Lose,
        Cleanup
    }

    public class B_GameManager : Singleton<B_GameManager>
    {
        public int CurrentLevel { get; set; }
        private GameState _state = GameState.None;

        [Header("Levels Settings")]
        [SerializeField] private int levelsCount = 10;

        [Header("Auto Start Settings")]
        [SerializeField] private bool autoStartGame = true;
        [SerializeField] private float startDelay = 0.5f;

        [Header("Scene Detection")]
        [SerializeField] private bool autoInitOnSceneLoad = true;

        [Header("Clip")]
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip LoseClip;

        private bool _isGameFinished = false;
        private B_UIManager uiManager;
        private Coroutine _initCoroutine;
        private bool _sceneJustLoaded = false;

        protected override void Awake()
        {
            base.Awake();

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Physics.reuseCollisionCallbacks = true;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _sceneJustLoaded = true;

            if (autoInitOnSceneLoad && scene.name.Contains("Level_"))
            {
                string levelName = scene.name.Replace("Level_", "");
                if (int.TryParse(levelName, out int levelNumber))
                {
                    CurrentLevel = levelNumber;
                }

                StartCoroutine(InitializeAfterSceneLoad());
            }
        }

        private IEnumerator InitializeAfterSceneLoad()
        {
            yield return new WaitForEndOfFrame();

            if (_initCoroutine != null)
            {
                StopCoroutine(_initCoroutine);
            }
            _initCoroutine = StartCoroutine(InitializeGame());
        }

        void Start()
        {
            uiManager = B_UIManager.Instance;

            if (!_sceneJustLoaded)
            {
                _initCoroutine = StartCoroutine(InitializeGame());
            }

            _sceneJustLoaded = false;
        }

        private IEnumerator InitializeGame()
        {
            SetState(GameState.Initialize);

            _isGameFinished = false;

            yield return null;

            var player = FindObjectOfType<B_PlayerController>();
            if (player != null)
            {
                var permanentUpgradeManager = B_PermanentUpgradeManager.Instance;
                if (permanentUpgradeManager != null)
                {
                    permanentUpgradeManager.Init(player);
                    permanentUpgradeManager.ApplyAllUpgrades();
                }
            }

            yield return new WaitForSeconds(0.1f);

            SetState(GameState.Ready);

            if (autoStartGame)
            {
                yield return new WaitForSeconds(startDelay);
                GameStart();
            }
        }

        public void SetState(GameState newState)
        {
            if (_state == newState) return;

            GameState oldState = _state;
            _state = newState;

            switch (_state)
            {
                case GameState.Ready:
                case GameState.Play:
                case GameState.PickupUpgrade:
                    Time.timeScale = 1f;
                    B_AudioManager.Instance?.SetBgmVolume(1f);
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;

                case GameState.Win:
                    Time.timeScale = 1f;
                    if (B_AudioManager.Instance != null)
                    {
                        B_AudioManager.Instance.SetBgmVolume(0.3f);
                        B_AudioManager.Instance.PlaySfx(winClip, 1, () => B_AudioManager.Instance.PlayBgm());
                    }
                    B_UpgradeManager.Instance?.ClearAllUpgrades();
                    UnlockNextLevel(CurrentLevel);
                    break;

                case GameState.Lose:
                    Time.timeScale = 1f;
                    if (B_AudioManager.Instance != null)
                    {
                        B_AudioManager.Instance.SetBgmVolume(0.3f);
                        B_AudioManager.Instance.PlaySfx(LoseClip, 1, () => B_AudioManager.Instance.PlayBgm());
                    }
                    B_UpgradeManager.Instance?.ClearAllUpgrades();
                    break;
            }

            if (uiManager != null)
            {
                uiManager.NotifyGameStateChanged(_state);
            }
        }

        public void GameStart()
        {
            if (_state == GameState.Play)
            {
                return;
            }

            if (_state == GameState.Win || _state == GameState.Lose)
            {
                return;
            }

            _isGameFinished = false;
            SetState(GameState.Play);
        }

        public void RestartLevel()
        {
            if (_initCoroutine != null)
            {
                StopCoroutine(_initCoroutine);
                _initCoroutine = null;
            }

            StartCoroutine(RestartLevelRoutine());
        }

        private IEnumerator RestartLevelRoutine()
        {
            SetState(GameState.Cleanup);

            B_UpgradeManager.Instance?.ClearAllUpgrades();

            Time.timeScale = 1f;

            _isGameFinished = false;

            yield return null;

            B_LoadingScreenRoot.Instance.LoadSceneWithName(
                GetLevelSceneName(CurrentLevel)
            );
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

            _isGameFinished = true;
            SetState(finishState);
        }

        #region Level Management

        private void SetMaxUnlockedLevel(int level)
        {
            int cappedLevel = Mathf.Min(level, levelsCount);
            int current = GetMaxUnlockedLevel();

            if (cappedLevel > current)
            {
                PlayerPrefs.SetInt(B_SafetyKey.KEY_PLAYPREF_MAX_UNLOCKED_LEVEL, cappedLevel);
                PlayerPrefs.Save();
            }
        }

        public int GetMaxUnlockedLevel()
        {
            int maxLevel = PlayerPrefs.GetInt(B_SafetyKey.KEY_PLAYPREF_MAX_UNLOCKED_LEVEL, 1);
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
                MB_PopupManager.Instance?.ShowTopNotification($"Level {level} is locked!", Color.red);
                return;
            }

            if (_initCoroutine != null)
            {
                StopCoroutine(_initCoroutine);
                _initCoroutine = null;
            }

            StartCoroutine(LoadLevelRoutine(level));
        }

        private IEnumerator LoadLevelRoutine(int level)
        {
            SetState(GameState.Cleanup);
            B_UpgradeManager.Instance?.ClearAllUpgrades();
            Time.timeScale = 1f;
            _isGameFinished = false;

            CurrentLevel = level;

            yield return null;

            string sceneName = GetLevelSceneName(level);
            B_LoadingScreenRoot.Instance.LoadSceneWithName(sceneName);
        }

        public void LoadNextLevel()
        {
            int nextLevel = CurrentLevel + 1;

            if (CheckNextLevelInvalid())
            {
                B_LoadingScreenRoot.Instance.LoadSceneWithName(GetLevelSceneName(GetMaxUnlockedLevel()));
                return;
            }

            LoadLevelScene(nextLevel);
        }

        public bool CheckNextLevelInvalid()
        {
            int nextLevel = CurrentLevel + 1;
            return nextLevel > levelsCount;
        }

        #endregion

        #region Public Accessors

        public GameState GetState() => _state;
        public bool IsGameFinished() => _isGameFinished;

        public void ForceReinitialize()
        {
            if (_initCoroutine != null)
            {
                StopCoroutine(_initCoroutine);
            }
            _initCoroutine = StartCoroutine(InitializeGame());
        }

        #endregion
    }
}
