using System;
using Unity.Mathematics;
using UnityEngine;

namespace VoltaTwins
{
    [DefaultExecutionOrder(-90)]
    public class VT_LevelManager : MonoBehaviour
    {
        public static VT_LevelManager Instance { get; private set; }

        [Header("Level Data")]
        [SerializeField] private VT_LevelData levelData;

        [Header("Spawn")]
        [SerializeField] private Transform levelSpawnRoot;

        [Header("Energy Ball")]
        [SerializeField] private VT_EnergyCore energyBallPrefabs;
        private VT_EnergyCore energyBall;

        [Header("Starting Player")]
        [SerializeField] private PlayerType startingPlayer = PlayerType.Blue;

        [Header("Goals")]
        [SerializeField] private int requiredGoals = 2;

        private int _currentPressedGoals = 0;
        private bool IsGameFinish;
        public bool isGameFinish => IsGameFinish;

        public int CurrentLevelId { get; private set; }
        public GameObject SpawnedLevel { get; private set; }

        private VT_GameManager _GameManager;
        private bool _initialized;

        private VT_PlayerController bluePlayer;
        private VT_PlayerController redPlayer;

        private VT_LevelRoot _currentLevelRoot;

        public Action OnShoot;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            IsGameFinish = false;
            _currentPressedGoals = 0;

            _GameManager = VT_GameManager.Instance;

            if (_GameManager == null)
            {
                return;
            }

            int targetLevel = _GameManager.currentLevel;

            var found = FindLevelById(targetLevel, out var lv);
            if (!found)
            {
                if (!checkNextLevelInvalid())
                {
                    found = FindLevelById(1, out lv);
                    if (!found)
                    {
                        return;
                    }
                }
            }

            CurrentLevelId = lv.levelID;

            InitializeLevel(lv);
            InitializeGame();

            _initialized = true;

            GameStart();
        }

        private void InitializeGame()
        {
            VT_PlayerController starter = startingPlayer == PlayerType.Blue ? bluePlayer : redPlayer;
            VT_EnergyCore go = Instantiate(energyBallPrefabs, Vector3.zero, quaternion.identity);
            energyBall = go.GetComponent<VT_EnergyCore>();
            energyBall.SetInitialOwner(starter);
        }

        public void OnGoalButtonStateChanged(bool isPressed)
        {
            if (isPressed)
            {
                _currentPressedGoals++;
            }
            else
            {
                _currentPressedGoals = Mathf.Max(0, _currentPressedGoals - 1);
            }

            if (!IsGameFinish && _currentPressedGoals >= requiredGoals)
            {
                if (FinishGame())
                {
                    if (_GameManager != null)
                    {
                        _GameManager.SetState(GameState.Win);
                    }
                }
            }
        }

        public bool checkNextLevelInvalid()
        {
            if (CurrentLevelId >= levelData.levels.Length)
            {
                return true;
            }
            return false;
        }

        public bool FinishGame()
        {
            if (IsGameFinish) return false;
            IsGameFinish = true;
            return true;
        }

        public void GameStart()
        {
            if (!_initialized) return;
            _GameManager.SetState(GameState.Play);
        }

        bool FindLevelById(int id, out Level lv)
        {
            lv = default;

            if (levelData == null)
            {
                return false;
            }

            if (levelData.levels == null || levelData.levels.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < levelData.levels.Length; i++)
            {
                var a = levelData.levels[i];
                if (a == null) continue;

                if (a.levels.levelID == id)
                {
                    lv = a.levels;
                    return true;
                }
            }

            return false;
        }

        void InitializeLevel(Level lv)
        {
            if (lv.levelPrefabs == null) return;

            var parent = levelSpawnRoot ? levelSpawnRoot : null;

            SpawnedLevel = Instantiate(
                lv.levelPrefabs,
                parent ? parent.position : Vector3.zero,
                parent ? parent.rotation : Quaternion.identity,
                parent
            );

            _currentLevelRoot = SpawnedLevel.GetComponent<VT_LevelRoot>();
            if (_currentLevelRoot == null)
            {
                Debug.LogError("Spawned level prefab is missing VT_LevelRoot component!");
                return;
            }

            if (_currentLevelRoot.bluePlayer != null)
                bluePlayer = _currentLevelRoot.bluePlayer;

            if (_currentLevelRoot.redPlayer != null)
                redPlayer = _currentLevelRoot.redPlayer;

            if (_currentLevelRoot.goals != null && _currentLevelRoot.goals.Length > 0)
            {
                requiredGoals = _currentLevelRoot.goals.Length;
            }

            _currentPressedGoals = 0;
            IsGameFinish = false;
        }
    }
}
