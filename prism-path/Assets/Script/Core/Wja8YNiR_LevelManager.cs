using System;
using System.Collections;
using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    [Serializable]
    public struct LevelHUDSnapshot
    {
        public int levelId;
        public string levelName;

        public float timeRemain;
        public float timeNorm;

        public int mirrorRemain;
        public int mirrorUsed;
        public int mirrorLimit;

        public int StarReceive;
    }

    public class Wja8YNiR_LevelManager : MonoBehaviour
    {
        public static Wja8YNiR_LevelManager Instance { get; private set; }

        [Header("Level Data")]
        [SerializeField] private Wja8YNiR_LevelData levelData;

        [Header("Spawn")]
        [SerializeField] private Transform levelSpawnRoot;
        [SerializeField] private bool autoStartWhenReady = true;
        [SerializeField] private bool useUnscaledTime = false;

        private bool IsGameFinish;
        public bool isGameFinish => IsGameFinish;

        public int CurrentLevelId { get; private set; }
        public GameObject SpawnedLevel { get; private set; }

        public float TimeLimit { get; private set; }
        public float TimeRemain { get; private set; }

        public int MirrorLimit { get; private set; }
        public int MirrorRemain { get; private set; }
        public int MirrorUsed => Mathf.Max(0, MirrorLimit - MirrorRemain);

        public int FirstStarRequire { get; private set; }
        public int SecondStarRequire { get; private set; }
        public int ThirdStarRequire { get; private set; }

        private bool _timerPaused;

        public event Action<LevelHUDSnapshot> OnLevelInitialized;
        public event Action<LevelHUDSnapshot> OnHUDChanged;
        public event Action OnTimeExpired;

        private Wja8YNiR_GameManager _GameManager;
        private Coroutine _timerCo;
        private bool _initialized;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            _GameManager = Wja8YNiR_GameManager.Instance;

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

            IsGameFinish = false;
            _initialized = true;
            if (autoStartWhenReady) GameStart();
        }

        void OnDisable() => StopTimer();

        public bool TryUseMirror()
        {
            if (MirrorRemain > MirrorLimit || MirrorRemain <= 0)
            {
                return false;
            }
            MirrorRemain -= 1;
            ChangeHUDValue();
            return true;
        }

        public void RefundMirror()
        {
            if (MirrorRemain > MirrorLimit) return;
            MirrorRemain += 1;
            ChangeHUDValue();
        }

        public LevelHUDSnapshot GetHUDValue()
        {
            float norm = TimeLimit <= 0.0001f ? 1f : Mathf.Clamp01(TimeRemain / TimeLimit);
            return new LevelHUDSnapshot
            {
                levelId = CurrentLevelId,
                levelName = $"Level {CurrentLevelId}",
                timeRemain = Mathf.Max(0f, TimeRemain),
                timeNorm = norm,
                mirrorRemain = MirrorRemain,
                mirrorUsed = MirrorUsed,
                mirrorLimit = MirrorLimit,
                StarReceive = CalculateReceiveStar()
            };
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
            _GameManager.SetState(GameState.Playing);
            StartTimer();
            ChangeHUDValue();
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
            if (lv.levelPrefabs != null)
            {
                var parent = levelSpawnRoot ? levelSpawnRoot : null;

                SpawnedLevel = Instantiate(
                lv.levelPrefabs,
                parent ? parent.position : Vector3.zero,
                parent ? parent.rotation : Quaternion.identity,
                parent
                );
            }

            TimeLimit = lv.timeLimit;
            TimeRemain = TimeLimit;

            MirrorLimit = lv.mirrorLimit;
            MirrorRemain = MirrorLimit;

            FirstStarRequire = lv.FirstStarRequire;
            SecondStarRequire = lv.SecondStarRequire;
            ThirdStarRequire = lv.ThirdStarRequire;

            LevelHUDSnapshot snap = GetHUDValue();
            OnLevelInitialized?.Invoke(snap);
            OnHUDChanged?.Invoke(snap);
        }

        void StartTimer()
        {
            StopTimer();
            _timerCo = StartCoroutine(CoTimer());
        }

        void StopTimer()
        {
            if (_timerCo != null)
            {
                StopCoroutine(_timerCo);
                _timerCo = null;
            }
        }

        public void PauseCountdown()
        {
            _timerPaused = true;
        }

        public void ContinueCountdown()
        {
            _timerPaused = false;
        }

        IEnumerator CoTimer()
        {
            while (TimeRemain > 0f && !isGameFinish)
            {
                if (!_timerPaused &&
                    (_GameManager.GetState() == GameState.Playing ||
                     _GameManager.GetState() == GameState.Setup ||
                     _GameManager.GetState() == GameState.Shooting))
                {
                    float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                    TimeRemain -= dt;
                    if (TimeRemain < 0f) TimeRemain = 0f;

                    ChangeHUDValue();
                }

                yield return null;
            }

            if (TimeRemain <= 0f && FinishGame())
            {
                OnTimeExpired?.Invoke();
                GetHUDValue();
                _GameManager.SetState(GameState.Lose);
            }
        }

        void ChangeHUDValue()
        {
            OnHUDChanged?.Invoke(GetHUDValue());
        }

        private int CalculateReceiveStar()
        {
            if (MirrorUsed <= ThirdStarRequire)
            {
                return 3;
            }

            if (MirrorUsed <= SecondStarRequire)
            {
                return 2;
            }

            if (MirrorUsed <= FirstStarRequire)
            {
                return 1;
            }

            return 0;
        }
    }
}
