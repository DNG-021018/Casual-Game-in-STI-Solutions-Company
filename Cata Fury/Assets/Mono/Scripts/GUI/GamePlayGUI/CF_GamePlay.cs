using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CataFury
{
    public class CF_GamePlay : CF_UIPage
    {
        [Header("Score UI")]
        [SerializeField] TextMeshProUGUI scoreText;

        [Header("Slider")]
        [SerializeField] Slider powerUpSlider;

        [Header("Buttons")]
        [SerializeField] CF_UIButton pauseButton;

        [Header("Booster Button")]
        [SerializeField] CF_UIButton boosterButton;
        [SerializeField]
        Image boosterCooldownFill;
        [SerializeField] TextMeshProUGUI boosterLabel;

        CF_BaseUI parent;
        CF_ScoreManager _scoreManager;
        CF_PlayerController _playerController;

        void Awake()
        {
            _scoreManager = ServiceLocator.Get<CF_ScoreManager>();
            _playerController = ServiceLocator.Get<CF_PlayerController>();
        }

        public override void Init(CF_BaseUI parent)
        {
            base.Init(parent);
            this.parent = parent;
        }

        void OnEnable()
        {
            if (_scoreManager != null)
                _scoreManager.OnScoreChanged += UpdateScoreUI;

            CF_PlayerController.OnKillStreakChanged += UpdatePowerUpSlider;
            CF_PlayerController.OnComboStateChanged += OnComboStateChanged;
            CF_PlayerController.OnBoosterStateChanged += OnBoosterStateChanged;
            CF_PlayerController.OnBoosterCooldownTick += OnBoosterCooldownTick;

            if (pauseButton != null)
                pauseButton.Bind(() => CF_GameManager.Instance.SetState(GameState.Pause));

            if (boosterButton != null)
                boosterButton.Bind(OnBoosterClicked);

            RefreshScoreUI();
            ResetPowerUpSlider();
            RefreshBoosterUI();
        }

        void OnDisable()
        {
            if (_scoreManager != null)
                _scoreManager.OnScoreChanged -= UpdateScoreUI;

            CF_PlayerController.OnKillStreakChanged -= UpdatePowerUpSlider;
            CF_PlayerController.OnComboStateChanged -= OnComboStateChanged;
            CF_PlayerController.OnBoosterStateChanged -= OnBoosterStateChanged;
            CF_PlayerController.OnBoosterCooldownTick -= OnBoosterCooldownTick;

            if (pauseButton != null) pauseButton.UnBind();
            if (boosterButton != null) boosterButton.UnBind();
        }


        private void RefreshScoreUI()
        {
            if (_scoreManager == null) return;
            UpdateScoreUI(_scoreManager.CurrentScore);
        }

        private void UpdateScoreUI(int score)
        {
            if (scoreText != null) scoreText.text = score.ToString();
        }


        private void UpdatePowerUpSlider(int currentKills, int requiredKills)
        {
            if (powerUpSlider == null) return;
            float targetValue = (float)currentKills / requiredKills;
            powerUpSlider.DOValue(targetValue, 0.15f).SetEase(Ease.OutQuad);
        }

        private void OnComboStateChanged(bool isActive)
        {
            if (isActive)
            {
                powerUpSlider.DOKill();
                powerUpSlider.value = 1f;
                float duration = _playerController != null ? _playerController.ComboDuration : 5f;
                powerUpSlider.DOValue(0f, duration).SetEase(Ease.Linear);
            }
            else
            {
                ResetPowerUpSlider();
            }
        }

        private void ResetPowerUpSlider()
        {
            if (powerUpSlider == null) return;
            powerUpSlider.DOKill();
            powerUpSlider.value = 0f;
        }


        private void OnBoosterClicked()
        {
            _playerController?.ActivateBooster();
        }

        private void OnBoosterStateChanged(bool isActive)
        {
            if (boosterButton == null) return;

            if (isActive)
            {
                boosterButton.SetInteractable(false);
                if (boosterLabel != null) boosterLabel.text = "BOOSTING";
                if (boosterCooldownFill != null) boosterCooldownFill.fillAmount = 1f;
            }
            else
            {
                boosterButton.SetInteractable(false);
                if (boosterLabel != null) boosterLabel.text = "COOLDOWN";
            }
        }

        private void OnBoosterCooldownTick(float remaining, float total)
        {
            if (boosterCooldownFill != null)
                boosterCooldownFill.fillAmount = total > 0f ? remaining / total : 0f;

            if (remaining <= 0f)
            {
                if (boosterButton != null) boosterButton.SetInteractable(true);
                if (boosterLabel != null) boosterLabel.text = "BOOST";
                if (boosterCooldownFill != null) boosterCooldownFill.fillAmount = 0f;
            }
        }

        private void RefreshBoosterUI()
        {
            if (_playerController == null || boosterButton == null) return;

            bool ready = _playerController.BoosterReady;
            boosterButton.SetInteractable(ready);

            if (boosterLabel != null)
                boosterLabel.text = ready ? "BOOST" : (_playerController.IsBoosted ? "BOOSTING" : "COOLDOWN");

            if (boosterCooldownFill != null)
                boosterCooldownFill.fillAmount = 0f;
        }


        public override IEnumerator Show()
        {
            canvasGroup.alpha = 1f;
            RefreshScoreUI();
            ResetPowerUpSlider();
            RefreshBoosterUI();
            yield return base.Show();
        }

        public override IEnumerator Hide()
        {
            canvasGroup.alpha = 0f;
            yield return base.Hide();
        }
    }
}