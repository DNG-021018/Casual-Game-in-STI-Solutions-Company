using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace NightEscape
{
    public class NE_GamePlay : NE_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels TopPanel;

        [Header("Button")]
        [SerializeField] NE_UIButton pauseButton;

        [Header("Top Panel")]
        [SerializeField] private Image Sprite;
        [SerializeField] private Sprite[] levelInfo;

        [Header("Cooldown Timer")]
        [SerializeField] private TextMeshProUGUI cooldownTimerText;
        [SerializeField] private Color warningColor = Color.red;

        [Header("Clip")]
        [SerializeField] AudioClip timerCountdownClip;
        [SerializeField] float scaleDownBGMVolume = 0.4f;

        Vector2 _TopStart;

        bool _initializedPos;
        private Color _originalColor;
        private Sequence _warningSequence;
        private bool _hasPlayedCountdownAudio;

        public static event Action Shoot = delegate { };

        public override void Init(NE_BaseUI parent)
        {
            base.Init(parent);
            CacheStartPositions();
            Sprite.sprite = levelInfo[NE_GameManager.Instance.CurrentLevel - 1];
        }

        void Start()
        {
            if (pauseButton != null)
            {
                pauseButton.Bind(() => OnPauseButtonClicked());
            }

            if (NE_GameManager.Instance != null)
            {
                NE_GameManager.Instance.OnCooldownTick += UpdateCooldownDisplay;
                NE_GameManager.Instance.OnCooldownFinished += OnCooldownFinished;
            }

            if (cooldownTimerText != null)
            {
                _originalColor = cooldownTimerText.color;
            }

            _hasPlayedCountdownAudio = false;
        }

        void OnDestroy()
        {
            if (pauseButton != null)
            {
                pauseButton.UnBind();
            }

            if (NE_GameManager.Instance != null)
            {
                NE_GameManager.Instance.OnCooldownTick -= UpdateCooldownDisplay;
                NE_GameManager.Instance.OnCooldownFinished -= OnCooldownFinished;
            }

            _warningSequence?.Kill();
        }

        private void OnPauseButtonClicked()
        {
            if (NE_GameManager.Instance != null)
            {
                GameState currentState = NE_GameManager.Instance.GetState();

                // Không cho pause nếu game đã kết thúc
                if (currentState == GameState.Win || currentState == GameState.Lose)
                {
                    return;
                }

                // Chỉ pause khi đang Play
                if (currentState == GameState.Play)
                {
                    NE_GameManager.Instance.SetState(GameState.Paused);
                }
            }
        }

        private void UpdateCooldownDisplay(float remainingTime)
        {
            if (cooldownTimerText != null)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                cooldownTimerText.text = $"{minutes:D2}:{seconds:D2}";

                if (remainingTime <= 6f && remainingTime > 0f)
                {
                    if (_warningSequence == null || !_warningSequence.IsActive())
                    {
                        StartWarningAnimation();
                    }
                }
            }
        }

        private void StartWarningAnimation()
        {
            _warningSequence?.Kill();
            _warningSequence = DOTween.Sequence();

            _warningSequence.SetLoops(-1, LoopType.Restart);

            _warningSequence.Append(
                cooldownTimerText.transform.DOScale(1.2f, 0.3f)
                    .SetEase(Ease.InOutQuad)
            );
            _warningSequence.Append(
                cooldownTimerText.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.InOutQuad)
            );

            _warningSequence.Join(
                cooldownTimerText.DOColor(warningColor, 0.3f)
                    .SetEase(Ease.InOutQuad)
            );
            _warningSequence.Append(
                cooldownTimerText.DOColor(_originalColor, 0.3f)
                    .SetEase(Ease.InOutQuad)
            );

            if (!_hasPlayedCountdownAudio && timerCountdownClip != null)
            {
                _hasPlayedCountdownAudio = true;

                if (NE_AudioManager.Instance != null)
                {
                    NE_AudioManager.Instance.SetBgmVolume(scaleDownBGMVolume);
                    NE_AudioManager.Instance.PlaySfxWithDuration(timerCountdownClip, 6f);
                }
            }
        }

        private void OnCooldownFinished()
        {
            _warningSequence?.Kill();

            if (cooldownTimerText != null)
            {
                cooldownTimerText.color = _originalColor;
                cooldownTimerText.transform.localScale = Vector3.one;
            }

            _hasPlayedCountdownAudio = false;
        }

        protected override void CacheStartPositions()
        {
            if (_initializedPos) return;
            if (TopPanel.panel) _TopStart = TopPanel.panel.anchoredPosition;
            _initializedPos = true;
        }

        public override IEnumerator Show(object ctx = null)
        {
            CacheStartPositions();

            Vector2 rFrom = GetOffscreenPos(TopPanel.panel, TopPanel.slideDir, _TopStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (TopPanel.panel, rFrom, _TopStart)
            );
        }

        public override IEnumerator Hide()
        {
            CacheStartPositions();

            Vector2 rTo = GetOffscreenPos(TopPanel.panel, TopPanel.slideDir, _TopStart, offscreenPadding);

            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (TopPanel.panel, _TopStart, rTo)
            );
        }
    }
}