using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace bJakGZQ3_Outer_World
{
    [DefaultExecutionOrder(-10)]
    public class bJakGZQ3_LevelManager : MonoBehaviour
    {
        public static bJakGZQ3_LevelManager Instance { get; private set; }

        [SerializeField] private bJakGZQ3_GridSystem _gridSystem;
        [SerializeField] private bJakGZQ3_Timer _timer;
        [SerializeField] private bJakGZQ3_Player playerRef;
        [SerializeField] private bJakGZQ3_SpawnManager _spawnManager;
        private bool _initialized;
        private bool _isGameFinish = false;

        private int _stepsCount = 0;
        private int _roundCount = 1;

        private string _timeStr = "00:00";

        public string TimeStr => _timeStr;
        public int StepsCount => _stepsCount;
        public int RoundCount => _roundCount;

        public event Action<string> OnTimeChanged;
        public event Action<int> OnStepsChanged;
        public event Action<int> OnRoundChanged;
        public event Action OnPlayerStep;

        public event Action OnPlayerMoveStart;
        public static event Action OnEnemyMoveFinish;

        bJakGZQ3_GameManager _gameManager;

        private HashSet<bJakGZQ3_Enemy> _activeEnemies = new HashSet<bJakGZQ3_Enemy>();
        private int _enemiesWaitingToMove = 0;

        [Header("Intro")]
        [SerializeField] private float playerSpawnHeight = 6f;
        [SerializeField] private float playerIntroDuration = 1.2f;
        [SerializeField] private Ease playerIntroEase = Ease.OutCubic;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            _gameManager = bJakGZQ3_GameManager.Instance;
            if (_gameManager)
            {
                _gameManager.SetState(GameState.LevelSetup);
            }

            if (_gridSystem == null)
            {
                _gridSystem = FindFirstObjectByType<bJakGZQ3_GridSystem>();
            }

            if (_timer == null)
            {
                _timer = GetComponent<bJakGZQ3_Timer>();
            }

            if (playerRef == null)
            {
                playerRef = FindFirstObjectByType<bJakGZQ3_Player>();
            }

            if (_timer != null)
            {
                _timer.OnTimerUpdated += HandleTimerUpdated;
            }

            _initialized = true;
            StartGame();
        }

        public void StartGame()
        {
            if (!_initialized) return;

            _isGameFinish = false;

            _stepsCount = 0;
            _roundCount = 1;
            _timeStr = "00:00";

            if (_gridSystem != null)
            {
                _gridSystem.Inititalize(() =>
                {
                    StartCoroutine(PlayerIntroCoroutine());
                });
            }
            else
            {
                if (playerRef != null)
                    StartCoroutine(PlayerIntroCoroutine());
                else
                {
                    if (_gameManager) _gameManager.SetState(GameState.Play);
                    if (_timer) _timer.StartRecordTime();
                    PushHUDInitial();
                    bJakGZQ3_DataManager mm = bJakGZQ3_DataManager.Instance;
                    if (mm != null)
                    {
                        mm.GenerateNewRoundMissions();
                    }
                }
            }
        }

        private IEnumerator PlayerIntroCoroutine()
        {
            if (playerRef == null)
            {
                playerRef = FindFirstObjectByType<bJakGZQ3_Player>();
                if (playerRef == null)
                {
                    if (_gameManager) _gameManager.SetState(GameState.Play);
                    if (_timer) _timer.StartRecordTime();
                    PushHUDInitial();

                    bJakGZQ3_DataManager mm = bJakGZQ3_DataManager.Instance;
                    if (mm != null) mm.GenerateNewRoundMissions();

                    if (_spawnManager == null)
                        _spawnManager = FindFirstObjectByType<bJakGZQ3_SpawnManager>();
                    _spawnManager?.BeginSpawning();

                    yield break;
                }
            }

            Vector3 target = new Vector3(0f, 1f, 0f);
            Vector3 startPos = target + Vector3.up * playerSpawnHeight;

            var gridMove = playerRef.GetComponent<bJakGZQ3_GridMovement>();
            var charSM = playerRef.GetComponent<bJakGZQ3_CharacterStateMachine>();
            gridMove?.DisableMovement();
            if (charSM != null) charSM.SwitchState(EntityState.Idle);

            playerRef.transform.position = startPos;

            bool landed = false;
            playerRef.transform.DOMove(target, playerIntroDuration)
                .SetEase(playerIntroEase)
                .OnComplete(() => landed = true);

            while (!landed) yield return null;
            yield return new WaitForSeconds(0.05f);

            gridMove?.EnableMovement();
            if (_gameManager) _gameManager.SetState(GameState.Play);
            if (_timer) _timer.StartRecordTime();
            PushHUDInitial();

            bJakGZQ3_DataManager mm2 = bJakGZQ3_DataManager.Instance;
            if (mm2 != null) mm2.GenerateNewRoundMissions();

            if (_spawnManager == null)
                _spawnManager = FindFirstObjectByType<bJakGZQ3_SpawnManager>();
            _spawnManager?.BeginSpawning();

            RegisterAllEnemies();
        }

        void PushHUDInitial()
        {
            OnStepsChanged?.Invoke(_stepsCount);
            OnRoundChanged?.Invoke(_roundCount);
            OnTimeChanged?.Invoke(_timeStr);
        }

        public bool FinishGame()
        {
            if (_isGameFinish) return false;
            _isGameFinish = true;

            OnStepsChanged?.Invoke(_stepsCount);
            OnRoundChanged?.Invoke(_roundCount);
            OnTimeChanged?.Invoke(_timeStr);
            return true;
        }

        void HandleTimerUpdated(float t)
        {
            if (_isGameFinish) return;

            _timeStr = FormatTime(t);
            OnTimeChanged?.Invoke(_timeStr);
        }

        string FormatTime(float t)
        {
            if (t < 0f) t = 0f;
            int m = Mathf.FloorToInt(t / 60f);
            int s = Mathf.FloorToInt(t % 60f);
            return $"{m:00}:{s:00}";
        }

        public void AddStep()
        {
            if (_isGameFinish) return;
            _stepsCount++;
            OnStepsChanged?.Invoke(_stepsCount);
        }

        public void NextRound()
        {
            if (_isGameFinish) return;

            _roundCount++;
            OnRoundChanged?.Invoke(_roundCount);
            if (CheckRoundStage())
            {
                _gridSystem.AddSize();
            }
        }

        public void RegisterEnemy(bJakGZQ3_Enemy enemy)
        {
            if (enemy != null && !_activeEnemies.Contains(enemy))
            {
                _activeEnemies.Add(enemy);
            }
        }

        public void UnregisterEnemy(bJakGZQ3_Enemy enemy)
        {
            if (enemy != null)
            {
                _activeEnemies.Remove(enemy);
            }
        }

        private void RegisterAllEnemies()
        {
            _activeEnemies.Clear();
            var enemies = FindObjectsByType<bJakGZQ3_Enemy>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                RegisterEnemy(enemy);
            }
        }

        public void NotifyPlayerMoveStart()
        {
            OnPlayerStep?.Invoke();

            _activeEnemies.RemoveWhere(e => e == null);
            _enemiesWaitingToMove = _activeEnemies.Count;

            if (_enemiesWaitingToMove > 0)
            {
                OnPlayerMoveStart?.Invoke();
            }
            else
            {
                OnEnemyMoveFinish?.Invoke();
            }
        }


        public void NotifyEnemyMoveFinish()
        {
            _enemiesWaitingToMove--;

            if (_enemiesWaitingToMove <= 0)
            {
                _enemiesWaitingToMove = 0;
                OnEnemyMoveFinish?.Invoke();
            }
        }

        private bool CheckRoundStage() => _roundCount % 5 == 1;
        public string GetFinalTime() => _timeStr;
        public string GetFinalSteps() => _stepsCount.ToString();
        public string GetFinalRound() => _roundCount.ToString();
    }
}