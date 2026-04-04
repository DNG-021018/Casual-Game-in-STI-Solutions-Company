using System;
using UnityEngine;

namespace CataFury
{
    public class CF_ScoreManager : MonoBehaviour
    {
        private int _currentScore;
        private int _highScore;

        public int CurrentScore => _currentScore;
        public int HighScore => _highScore;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighScoreChanged;

        private CF_UIManager _uiManager;
        private GameState _prevState = GameState.None;

        void Awake()
        {
            LoadHighScore();
        }

        void Start()
        {
            _uiManager = ServiceLocator.Get<CF_UIManager>();
            if (_uiManager != null)
                _uiManager.OnGameStateChanged += HandleGameState;

            LoadHighScore();
        }

        void OnDestroy()
        {
            if (_uiManager != null)
                _uiManager.OnGameStateChanged -= HandleGameState;
        }

        private void HandleGameState(GameState state)
        {
            if (state == GameState.Play && _prevState != GameState.Pause)
                ResetCurrentScore();

            _prevState = state;
        }

        public void AddScore(int amount = 1)
        {
            _currentScore += amount;
            OnScoreChanged?.Invoke(_currentScore);

            if (_currentScore > _highScore)
            {
                _highScore = _currentScore;
                SaveHighScore();
                OnHighScoreChanged?.Invoke(_highScore);
            }
        }

        public void ResetCurrentScore()
        {
            _currentScore = 0;
            OnScoreChanged?.Invoke(_currentScore);
        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt(CF_SafetyKey.Data.DAILY_HIGH_SCORE_SAVE_KEY, _highScore);
            PlayerPrefs.Save();
        }

        private void LoadHighScore()
        {
            _highScore = PlayerPrefs.GetInt(CF_SafetyKey.Data.DAILY_HIGH_SCORE_SAVE_KEY, 0);
        }
    }
}