using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoublesideZ
{
    public class DZ_DailyRewardItem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool isDailyDayFinal = false;
        [SerializeField] private DZ_UIButton button;
        [SerializeField] private AudioClip claimRewardClip;

        [Header("UI Components")]
        [SerializeField] private Image backgroundRewardImage;
        [SerializeField] private Image titleDayImage;
        [SerializeField] private Image IconRewardImage;
        [SerializeField] private GameObject claimedOverlay;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI rewardAmountText;
        [SerializeField] private GameObject activeEffectObject;

        private CanvasGroup _overlayCanvasGroup;
        private RectTransform _overlayRect;
        private Sequence _claimSequence;

        private DZ_AudioManager _audioManager;

        void Awake()
        {
            _audioManager = ServiceLocator.Get<DZ_AudioManager>();
            if (button == null) button = GetComponent<DZ_UIButton>();

            if (claimedOverlay != null)
            {
                _overlayCanvasGroup = claimedOverlay.GetComponent<CanvasGroup>();
                if (_overlayCanvasGroup == null)
                {
                    _overlayCanvasGroup = claimedOverlay.AddComponent<CanvasGroup>();
                }

                _overlayRect = claimedOverlay.GetComponent<RectTransform>();
                if (_overlayRect == null)
                {
                    _overlayRect = claimedOverlay.AddComponent<RectTransform>();
                }
            }
        }

        void OnDestroy()
        {
            KillClaimAnimation();
        }

        public void Bind(Action onClick)
        {
            if (button != null)
            {
                button.Bind(() =>
                {
                    onClick?.Invoke();
                    _audioManager.PlaySfx(claimRewardClip);
                });
            }
        }

        public void UnBind()
        {
            if (button != null)
            {
                button.UnBind();
            }
        }

        public void SetInfo(int amount, string day, Sprite icon)
        {
            rewardAmountText.text = amount.ToString();
            dayText.text = "Day " + day;
            IconRewardImage.sprite = icon;
        }

        public void SetRewardState(StateDailyReward state)
        {
            switch (state)
            {
                case StateDailyReward.Active:
                    SetActiveState();
                    break;
                case StateDailyReward.Inactive:
                    SetInactiveState();
                    break;
                case StateDailyReward.AlreadyClaimed:
                    SetClaimedState();
                    break;
            }
        }

        private void SetActiveState()
        {
            button.SetInteractable(true);
            activeEffectObject.SetActive(true);
            claimedOverlay.SetActive(false);
        }

        private void SetInactiveState()
        {
            button.SetInteractable(false);
            activeEffectObject.SetActive(isDailyDayFinal);
            claimedOverlay.SetActive(false);
        }

        private void SetClaimedState()
        {
            button.SetInteractable(false);
            activeEffectObject.SetActive(isDailyDayFinal);
        }

        public void PlayClaimAnimation()
        {
            KillClaimAnimation();

            claimedOverlay.SetActive(true);

            if (_overlayCanvasGroup != null)
                _overlayCanvasGroup.alpha = 0f;

            if (_overlayRect != null)
                _overlayRect.localScale = Vector3.one * 0.3f;

            transform.localScale = Vector3.one;

            _claimSequence = DOTween.Sequence();

            _claimSequence.Insert(0.12f,
                _overlayCanvasGroup.DOFade(1f, 0.12f)
                    .SetEase(Ease.OutQuad)
            );

            _claimSequence.Insert(0.12f,
                _overlayRect.DOScale(1f, 0.15f)
                    .From(0.3f)
                    .SetEase(Ease.OutBack, 4f)
            );

            _claimSequence.SetUpdate(true);
        }

        private void KillClaimAnimation()
        {
            if (_claimSequence != null && _claimSequence.IsActive())
            {
                _claimSequence.Kill();
                _claimSequence = null;
            }

            transform.localScale = Vector3.one;
            if (IconRewardImage != null)
            {
                IconRewardImage.transform.localScale = Vector3.one;
            }
        }

        public void OnRewardClaimed()
        {
            PlayClaimAnimation();
        }
    }
}