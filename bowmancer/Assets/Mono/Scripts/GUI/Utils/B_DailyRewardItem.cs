using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bowmancer
{
    public class B_DailyRewardItem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool isDailyDayFinal = false;
        [SerializeField] private B_UIButton button;
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private Transform coinSpawnPoint;
        [SerializeField] private AudioClip claimRewardClip;
        private Transform coinTargetPoint;

        [Header("UI Components")]
        [SerializeField] private Image backgroundRewardImage;
        [SerializeField] private Image titleDayImage;
        [SerializeField] private Image IconRewardImage;
        [SerializeField] private GameObject claimedOverlay;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI rewardAmountText;

        [Header("Active Image Group")]
        [SerializeField] private Sprite backgroundActiveRewardSprite;
        [SerializeField] private Sprite titleActive;
        [SerializeField] private Color TextActiveColor;
        [SerializeField] private GameObject activeEffectObject;

        [Header("Inactive Image Group")]
        [SerializeField] private Sprite backgroundInactiveRewardSprite;
        [SerializeField] private Sprite titleInactive;
        [SerializeField] private Color TextInactiveColor;

        [Header("Claim Animation Settings")]
        [SerializeField] private float claimScaleDuration = 0.3f;
        [SerializeField] private float claimScaleAmount = 1.2f;
        [SerializeField] private Ease claimScaleEase = Ease.OutBack;
        [SerializeField] private float overlayFadeDuration = 0.4f;
        [SerializeField] private float overlayScaleFrom = 0.5f;
        [SerializeField] private Ease overlayEase = Ease.OutBack;

        [Header("Coin Animation Settings")]
        [SerializeField] private float popScale = 1.25f;
        [SerializeField] private int coinCount = 10;
        [SerializeField] private float coinSpreadDuration = 0.25f;
        [SerializeField] private float coinDelayBetween = 0.05f;
        [SerializeField] private float coinSpreadRadius = 80f;
        [SerializeField] private float coinFlyDuration = 0.5f;
        [SerializeField] private AudioClip collectClip;

        private CanvasGroup _overlayCanvasGroup;
        private RectTransform _overlayRect;
        private Sequence _claimSequence;
        private Vector3 _targetBaseScale;

        private B_AudioManager _audioManager;

        void Awake()
        {
            _audioManager = B_AudioManager.Instance;
            if (button == null) button = GetComponent<B_UIButton>();

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
                    SpawnAndFlyCoins();
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

        public void SetInfo(int amount, string day, Sprite icon, Transform coinTarget)
        {
            rewardAmountText.text = amount.ToString();
            dayText.text = "Day " + day;
            IconRewardImage.sprite = icon;
            coinTargetPoint = coinTarget;

            if (coinTargetPoint != null)
            {
                _targetBaseScale = coinTargetPoint.localScale;
            }
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
            if (!isDailyDayFinal)
            {
                backgroundRewardImage.sprite = backgroundActiveRewardSprite;
                titleDayImage.sprite = titleActive;
                dayText.color = TextActiveColor;
            }
            button.SetInteractable(true);
            activeEffectObject.SetActive(true);
            claimedOverlay.SetActive(false);
        }

        private void SetInactiveState()
        {
            if (!isDailyDayFinal)
            {
                backgroundRewardImage.sprite = backgroundInactiveRewardSprite;
                titleDayImage.sprite = titleInactive;
                dayText.color = TextInactiveColor;
            }
            button.SetInteractable(false);
            activeEffectObject.SetActive(isDailyDayFinal);
            claimedOverlay.SetActive(false);
        }

        private void SetClaimedState()
        {
            if (!isDailyDayFinal)
            {
                backgroundRewardImage.sprite = backgroundInactiveRewardSprite;
                titleDayImage.sprite = titleInactive;
                dayText.color = TextInactiveColor;
            }
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

        private void SpawnAndFlyCoins()
        {
            if (coinPrefab == null || coinSpawnPoint == null || coinTargetPoint == null)
            {
                Debug.LogWarning("Missing coin setup!");
                return;
            }

            for (int i = 0; i < coinCount; i++)
            {
                int index = i;
                GameObject coin = Instantiate(coinPrefab, coinSpawnPoint);
                RectTransform rect = coin.GetComponent<RectTransform>();

                if (rect == null)
                {
                    Destroy(coin);
                    continue;
                }

                rect.anchoredPosition = Vector2.zero;

                Vector2 spreadPos = UnityEngine.Random.insideUnitCircle * coinSpreadRadius;

                Sequence coinSeq = DOTween.Sequence();

                coinSeq.Append(
                    rect.DOAnchorPos(spreadPos, coinSpreadDuration)
                        .SetEase(Ease.OutBack)
                );

                float delayBeforeFly = index * coinDelayBetween;
                coinSeq.AppendInterval(delayBeforeFly);

                coinSeq.Append(
                    rect.DOMove(coinTargetPoint.position, coinFlyDuration)
                        .SetEase(Ease.InQuad)
                );

                coinSeq.OnComplete(() =>
                {
                    PlayCoinArriveSound();

                    PlayCoinArriveFeedback();

                    Destroy(coin);
                });

                coinSeq.SetUpdate(true);
            }
        }

        private void PlayCoinArriveSound()
        {
            if (collectClip != null && _audioManager != null)
            {
                _audioManager.PlaySfx(collectClip, volumeScale: 1f);
            }
        }

        private void PlayCoinArriveFeedback()
        {
            if (coinTargetPoint == null) return;

            coinTargetPoint.DOKill();

            Sequence feedbackSeq = DOTween.Sequence();

            feedbackSeq.Append(
                coinTargetPoint.DOScale(_targetBaseScale * popScale, 0.08f)
                    .SetEase(Ease.OutQuad)
            );

            feedbackSeq.Append(
                coinTargetPoint.DOScale(_targetBaseScale, 0.12f)
                    .SetEase(Ease.OutBack)
            );

            feedbackSeq.SetUpdate(true);
        }

        public void OnRewardClaimed()
        {
            PlayClaimAnimation();
        }
    }
}