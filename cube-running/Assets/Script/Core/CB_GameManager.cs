using System;
using UnityEngine;

namespace CB_CubeRunner
{
    public enum GameState
    {
        Initialize,
        Play,
        Paused,
        FinishGame,
    }

    [DefaultExecutionOrder(-100)]
    public class CB_GameManager : MonoBehaviour
    {
        public static CB_GameManager Instance { get; private set; }

        GameState _state = GameState.Initialize;

        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnPlayerPoint;

        public int CurrentPoint { get; private set; }

        public event Action<int> OnSkinChanged;
        public int CurrentSkinId
        {
            get; private set;
        }

        const string KEY_HIGH_SCORE = "HIGH_SCORE";
        const string KEY_COIN = "COIN";

        public int TotalCoin { get; private set; }

        public const int MAX_COIN = 100;

        public event Action<int> OnCoinChanged;

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

            TotalCoin = PlayerPrefs.GetInt(KEY_COIN, 0);
        }

        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            CurrentPoint = 0;
            OnPlayerPoint?.Invoke(CurrentPoint);
            SetState(GameState.Initialize);
        }

        public void SetState(GameState s)
        {
            if (_state == s)
            {
                return;
            }

            var prev = _state;
            _state = s;

            switch (_state)
            {
                case GameState.Initialize:
                case GameState.Play:
                case GameState.FinishGame:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
            }

            if (_state == GameState.FinishGame)
            {
                SaveHighScore();
            }

            if (_state == GameState.Initialize)
            {
                CurrentPoint = 0;
                OnPlayerPoint?.Invoke(CurrentPoint);
            }

            OnGameStateChanged?.Invoke(_state);
        }

        public GameState GetState() => _state;

        public void AddPoint(int amount = 1)
        {
            CurrentPoint += amount;
            OnPlayerPoint?.Invoke(CurrentPoint);
        }

        public void AddCoin(int amount = 1)
        {
            TotalCoin = Mathf.Clamp(TotalCoin + amount, 0, MAX_COIN);
            PlayerPrefs.SetInt(KEY_COIN, TotalCoin);
            PlayerPrefs.Save();
            OnCoinChanged?.Invoke(TotalCoin);
        }

        public void SelectSkin(int id)
        {
            CurrentSkinId = id;
            OnSkinChanged?.Invoke(id);
        }

        public int GetHighScore()
        {
            return PlayerPrefs.GetInt(KEY_HIGH_SCORE, 0);
        }

        private void SaveHighScore()
        {
            int currentHigh = PlayerPrefs.GetInt(KEY_HIGH_SCORE, 0);
            if (CurrentPoint > currentHigh)
            {
                PlayerPrefs.SetInt(KEY_HIGH_SCORE, CurrentPoint);
                PlayerPrefs.Save();
            }
        }

        [ContextMenu("Set coin to 100")]
        public void setCoin()
        {
            PlayerPrefs.SetInt(KEY_COIN, 100);
            PlayerPrefs.Save();
        }
    }
}