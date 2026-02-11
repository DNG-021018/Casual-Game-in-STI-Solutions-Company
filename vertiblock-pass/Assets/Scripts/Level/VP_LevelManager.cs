using UnityEngine;

namespace VertiblockPass
{
    [DefaultExecutionOrder(-100)]
    public class VP_LevelManager : MonoBehaviour
    {
        public static VP_LevelManager Instance { get; private set; }

        [Header("Level Data")]
        [SerializeField] private VP_LevelData levelData;

        [Header("Spawn")]
        [SerializeField] private Transform levelSpawnRoot;

        private bool IsGameFinish;
        public bool isGameFinish => IsGameFinish;

        public int CurrentLevelId { get; private set; }
        public GameObject SpawnedLevel { get; private set; }

        private VP_GameManager _GameManager;

        private bool _initialized;
        private int step;

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
            step = 0;
            _GameManager = VP_GameManager.Instance;

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
            _GameManager?.SetState(GameState.Play);
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

            IsGameFinish = false;
        }

        public void PlayerStepCount()
        {
            if (!IsGameFinish)
            {
                step++;
            }
        }

        public int GetFinalStepCount()
        {
            return step;
        }
    }
}
