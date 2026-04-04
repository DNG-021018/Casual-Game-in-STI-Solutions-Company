using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CataFury
{
    public class CF_GameManager : Singleton<CF_GameManager>
    {
        public static event Action<GameState> OnGameStateChanged;

        private GameState _state = GameState.None;
        private GameState _prevState = GameState.None;

        [Header("Scene Detection")]
        [SerializeField] private bool autoInitOnSceneLoad = true;

        [Header("Clip")]
        [SerializeField] private AudioClip LoseClip;

        private bool _isGameFinished = false;
        private CF_UIManager uiManager;
        private CF_AudioManager audioManager;
        private CF_LoadingScreenManager LoadingSceneManager;
        private Coroutine _initCoroutine;
        private bool _sceneJustLoaded = false;

        protected override void Awake()
        {
            base.Awake();
            audioManager = ServiceLocator.Get<CF_AudioManager>();
            LoadingSceneManager = CF_LoadingScreenManager.Instance;
            uiManager = ServiceLocator.Get<CF_UIManager>();
        }

        void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _sceneJustLoaded = true;
            if (autoInitOnSceneLoad) InitializeAfterSceneLoad();
        }

        private void InitializeAfterSceneLoad()
        {
            if (_initCoroutine != null) StopCoroutine(_initCoroutine);
            _initCoroutine = StartCoroutine(InitializeGame());
        }

        void Start()
        {
            if (!_sceneJustLoaded)
                _initCoroutine = StartCoroutine(InitializeGame());
            _sceneJustLoaded = false;
        }

        private IEnumerator InitializeGame()
        {
            SetState(GameState.Initialize);
            _isGameFinished = false;

            yield return null;
            LoadingSceneManager.SetManualProgress(0.5f);
            yield return null;

            SetState(GameState.Ready);
        }

        public void SetState(GameState newState)
        {
            if (_state == newState) return;

            _prevState = _state;
            _state = newState;

            switch (_state)
            {
                case GameState.Initialize:
                case GameState.Ready:
                case GameState.Tutorial:
                    Time.timeScale = 1f;
                    audioManager?.SetBgmVolume(1f);
                    break;

                case GameState.Play:
                    Time.timeScale = 1f;
                    audioManager?.SetBgmVolume(1f);
                    // Bắt đầu spawn nếu là game mới (không phải resume từ Pause)
                    if (_prevState != GameState.Pause)
                        ServiceLocator.Get<CF_SpawnManager>()?.BeginSpawning();
                    else
                        // Resume từ Pause → chỉ resume spawn lại
                        ServiceLocator.Get<CF_SpawnManager>()?.BeginSpawning();
                    break;

                case GameState.Pause:
                    Time.timeScale = 0f;
                    ServiceLocator.Get<CF_SpawnManager>()?.PauseSpawning();
                    break;

                case GameState.Cleanup:
                    Time.timeScale = 1f;
                    audioManager?.SetBgmVolume(1f);
                    ServiceLocator.Get<CF_SpawnManager>()?.StopSpawning();
                    break;

                case GameState.Lose:
                    Time.timeScale = 1f;
                    ServiceLocator.Get<CF_SpawnManager>()?.PauseSpawning();
                    if (audioManager != null)
                    {
                        audioManager.SetBgmVolume(0.3f);
                        audioManager.PlaySfx(LoseClip, 1, () => audioManager.PlayBgm());
                    }
                    break;
            }

            OnGameStateChanged?.Invoke(_state);
            uiManager?.NotifyGameStateChanged(_state);
        }

        public void GameStart()
        {
            if (_state == GameState.Play || _state == GameState.Lose) return;
            _isGameFinished = false;
            bool tutorialDone = PlayerPrefs.GetInt(CF_SafetyKey.Data.KEY_TUTORIAL_DONE, 0) == 1;
            SetState(tutorialDone ? GameState.Play : GameState.Tutorial);
        }

        public void ResumeGame()
        {
            if (_state != GameState.Pause) return;
            SetState(_prevState != GameState.None ? _prevState : GameState.Play);
        }

        public void TutorialComplete()
        {
            PlayerPrefs.SetInt(CF_SafetyKey.Data.KEY_TUTORIAL_DONE, 1);
            PlayerPrefs.Save();
            SetState(GameState.Play);
        }

        public void ReplayGame()
        {
            if (_initCoroutine != null) { StopCoroutine(_initCoroutine); _initCoroutine = null; }
            StartCoroutine(ReplaySequence());
        }

        private IEnumerator ReplaySequence()
        {
            ServiceLocator.Get<CF_SpawnManager>()?.StopSpawning();
            ServiceLocator.Get<CF_PlayerController>()?.ResetPlayer();
            ServiceLocator.Get<CF_SpawnManager>()?.ResetSpawning();
            _isGameFinished = false;
            yield return null;
            SetState(GameState.Play);
        }

        public void RestartGame()
        {
            if (_initCoroutine != null) { StopCoroutine(_initCoroutine); _initCoroutine = null; }
            StartCoroutine(ReturnToMenuSequence());
        }

        private IEnumerator ReturnToMenuSequence()
        {
            LoadingSceneManager.SetManualProgress(0f);
            yield return StartCoroutine(LoadingSceneManager.ShowVisualAndWait());
            yield return new WaitForSeconds(0.3f);

            SetState(GameState.Cleanup);
            yield return null;

            ServiceLocator.Get<CF_SpawnManager>()?.ResetSpawning();
            LoadingSceneManager.SetManualProgress(0.4f);
            ServiceLocator.Get<CF_PlayerController>()?.ResetPlayer();
            LoadingSceneManager.SetManualProgress(1f);
            _isGameFinished = false;

            yield return new WaitForSeconds(0.3f);
            SetState(GameState.Ready);
            yield return StartCoroutine(LoadingSceneManager.HideVisualAndWait());
        }

        public void FinishGame(GameState finishState)
        {
            if (_isGameFinished || finishState != GameState.Lose) return;
            _isGameFinished = true;
            SetState(finishState);
        }

        public GameState GetState() => _state;
        public bool IsGameFinished() => _isGameFinished;
    }
}