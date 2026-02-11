using UnityEngine;

namespace CubeSokoban
{
    [DefaultExecutionOrder(-50)]
    public class CS_LevelManager : MonoBehaviour
    {
        public static CS_LevelManager Instance { get; private set; }

        [Header("Level Data")]
        [SerializeField] private CS_LevelData levelData;

        [Header("Spawn")]
        [SerializeField] private Transform levelSpawnRoot;

        private int _currentPressedGoals = 0;
        private bool IsGameFinish;
        public bool isGameFinish => IsGameFinish;

        private int requiredGoals;

        public int CurrentLevelId { get; private set; }
        public GameObject SpawnedLevel { get; private set; }

        private CS_GameManager _GameManager;

        private bool _initialized;

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

            _GameManager = CS_GameManager.Instance;

            if (_GameManager == null)
            {
                return;
            }

            int targetLevel = _GameManager.currentLevel;

            var found = FindLevelById(targetLevel, out var lv);
            if (!found)
            {
                if (!CheckNextLevelInvalid())
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

            _initialized = true;
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

        public bool CheckNextLevelInvalid()
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

                if (a.level.levelID == id)
                {
                    lv = a.level;
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

            requiredGoals = lv.requiredGoals;

            _currentPressedGoals = 0;
            IsGameFinish = false;
        }
    }
}
