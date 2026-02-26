using System;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_ScoreManager : MonoBehaviour
    {
        private int _currentScore;
        private int _highScore;

        public int CurrentScore => _currentScore;
        public int HighScore => _highScore;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighScoreChanged;

        void Awake()
        {
            LoadHighScore();
        }

        private DZ_UIManager _uiManager;

        void Start()
        {
            _uiManager = ServiceLocator.Get<DZ_UIManager>();
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
            if (state == GameState.Play)
                ResetCurrentScore();
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
            PlayerPrefs.SetInt(DZ_SafetyKey.DAILY_HIGH_SCORE_SAVE_KEY, _highScore);
            PlayerPrefs.Save();
        }

        private void LoadHighScore()
        {
            _highScore = PlayerPrefs.GetInt(DZ_SafetyKey.DAILY_HIGH_SCORE_SAVE_KEY, 0);
        }
    }
}