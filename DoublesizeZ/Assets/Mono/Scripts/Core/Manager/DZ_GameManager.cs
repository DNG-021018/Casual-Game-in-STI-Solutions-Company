using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DoublesideZ
{
    public class DZ_GameManager : Singleton<DZ_GameManager>
    {
        private GameState _state = GameState.None;

        [Header("Auto Start Settings")]
        [SerializeField] private bool autoStartGame = true;
        [SerializeField] private float startDelay = 0.5f;

        [Header("Scene Detection")]
        [SerializeField] private bool autoInitOnSceneLoad = true;

        [Header("Clip")]
        [SerializeField] private AudioClip LoseClip;

        private bool _isGameFinished = false;
        private DZ_UIManager uiManager;
        private DZ_AudioManager audioManager;
        private DZ_LoadingScreenManager LoadingSceneManager;
        private Coroutine _initCoroutine;
        private bool _sceneJustLoaded = false;

        protected override void Awake()
        {
            base.Awake();
            audioManager = ServiceLocator.Get<DZ_AudioManager>();
            LoadingSceneManager = DZ_LoadingScreenManager.Instance;

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
                StartCoroutine(InitializeAfterSceneLoad());
        }

        private IEnumerator InitializeAfterSceneLoad()
        {
            yield return new WaitForEndOfFrame();

            if (_initCoroutine != null)
                StopCoroutine(_initCoroutine);

            _initCoroutine = StartCoroutine(InitializeGame());
        }

        void Start()
        {
            uiManager = ServiceLocator.Get<DZ_UIManager>();

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

            yield return new WaitForSeconds(1f);

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

            _state = newState;

            switch (_state)
            {
                case GameState.Initialize:
                case GameState.Ready:
                case GameState.Play:
                case GameState.Cleanup:
                    Time.timeScale = 1f;
                    audioManager?.SetBgmVolume(1f);
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;

                case GameState.Lose:
                    Time.timeScale = 1f;
                    if (audioManager != null)
                    {
                        audioManager.SetBgmVolume(0.3f);
                        audioManager.PlaySfx(LoseClip, 1, () => audioManager.PlayBgm());
                    }
                    break;
            }

            uiManager?.NotifyGameStateChanged(_state);
        }


        public void GameStart()
        {
            if (_state == GameState.Play || _state == GameState.Lose) return;

            _isGameFinished = false;
            SetState(GameState.Play);
            ServiceLocator.Get<DZ_SpawnManager>()?.BeginSpawning();
        }

        public void RestartGame()
        {
            if (_initCoroutine != null)
            {
                StopCoroutine(_initCoroutine);
                _initCoroutine = null;
            }

            StartCoroutine(ReturnToMenuSequence());
        }

        private IEnumerator ReturnToMenuSequence()
        {
            LoadingSceneManager.SetManualProgress(0f);
            yield return StartCoroutine(LoadingSceneManager.ShowVisualAndWait());
            yield return new WaitForSeconds(0.3f);

            SetState(GameState.Cleanup);
            yield return null;

            ServiceLocator.Get<DZ_SpawnManager>()?.ResetSpawning();
            LoadingSceneManager.SetManualProgress(0.4f);

            ServiceLocator.Get<DZ_PlayerController>()?.ResetPlayer();
            LoadingSceneManager.SetManualProgress(0.7f);

            ServiceLocator.Get<DZ_CameraManager>()?.SwitchToMenuCamera();
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
            ServiceLocator.Get<DZ_SpawnManager>()?.StopSpawning();
            SetState(finishState);
        }

        public GameState GetState() => _state;
        public bool IsGameFinished() => _isGameFinished;
    }
}
