using System.Collections;
using UnityEngine;
using TMPro;
using System;

namespace CataFury
{
    public class CF_LoseGameMenu : CF_UIPage
    {
        [Header("Tween")]
        [SerializeField] Panels loseMenu;
        private Vector2 _originalPanelPos;

        [Header("Buttons")]
        [SerializeField] private CF_UIButton RestartButton;
        [SerializeField] private CF_UIButton ReturnToMainMenuButton;

        [Header("Score UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI currentCoinText;

        private CF_BaseUI parent;
        private CF_GameManager _gameManager;
        private CF_ScoreManager _scoreManager;
        private CF_CurrencyManager _currencyManager;
        private CF_EnvironmentManager _environmentManager;

        private void Awake()
        {
            _gameManager = CF_GameManager.Instance;
            _scoreManager = ServiceLocator.Get<CF_ScoreManager>();
            _environmentManager = ServiceLocator.Get<CF_EnvironmentManager>();
            _currencyManager = ServiceLocator.Get<CF_CurrencyManager>();
        }

        public override void Init(CF_BaseUI parent)
        {
            base.Init(parent);
            this.parent = parent;
            CacheStartPositions();
        }

        void OnEnable()
        {
            RestartButton?.Bind(OnRestartClicked);
            ReturnToMainMenuButton?.Bind(OnReturnToMainMenuClicked);
        }

        void OnDisable()
        {
            RestartButton?.UnBind();
            ReturnToMainMenuButton?.UnBind();
        }

        protected override void CacheStartPositions()
        {
            if (loseMenu.panel != null)
                _originalPanelPos = loseMenu.panel.anchoredPosition;
        }

        private void OnRestartClicked()
        {
            _gameManager.ReplayGame();
            _environmentManager?.ResetEnvironment();
        }

        private void OnReturnToMainMenuClicked()
        {
            _gameManager.RestartGame();
            _environmentManager?.ResetEnvironment();
        }

        private void RefreshScoreUI()
        {
            if (_scoreManager == null || _currencyManager == null) return;
            if (scoreText != null) scoreText.text = _scoreManager.CurrentScore.ToString();
            if (highScoreText != null) highScoreText.text = _scoreManager.HighScore.ToString();
            if (currentCoinText != null) currentCoinText.text = _currencyManager.GetCoins().ToString();
        }

        public override IEnumerator Show()
        {
            RefreshScoreUI();

            Vector2 offscreen = GetOffscreenPos(loseMenu.panel, SlideDir.Up, _originalPanelPos, loseMenu.offscreenPadding);

            yield return ShowMovePanels(
                loseMenu.duration, loseMenu.showEase,
                0f, 1f,
                (loseMenu.panel, offscreen, _originalPanelPos)
            );
        }

        public override IEnumerator Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.gameObject.SetActive(false);

            if (loseMenu.panel != null)
                loseMenu.panel.anchoredPosition = _originalPanelPos;

            yield break;
        }
    }
}