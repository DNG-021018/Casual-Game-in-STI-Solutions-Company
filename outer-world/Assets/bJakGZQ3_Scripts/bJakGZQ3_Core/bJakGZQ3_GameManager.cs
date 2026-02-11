using System;
using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public enum GameState
    {
        Initialize,
        LevelSetup,
        Play,
        Paused,
        FinishGame,
    }
    [DefaultExecutionOrder(-100)]
    public class bJakGZQ3_GameManager : MonoBehaviour
    {
        public static bJakGZQ3_GameManager Instance { get; private set; }

        GameState _state = GameState.Initialize;

        public event Action<GameState> OnGameStateChanged;

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
        }

        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            SetState(GameState.Initialize);
        }

        public void SetState(GameState s)
        {
            if (_state == s) return;
            _state = s;
            switch (_state)
            {
                case GameState.Initialize:
                case GameState.LevelSetup:
                case GameState.Play:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.FinishGame:
                    Time.timeScale = 1f;
                    bJakGZQ3_LevelManager.Instance?.FinishGame();
                    break;
            }
            OnGameStateChanged?.Invoke(_state);
        }

        public GameState GetState() => _state;
    }
}