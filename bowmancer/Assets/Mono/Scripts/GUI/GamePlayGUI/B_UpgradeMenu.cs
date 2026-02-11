using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Bowmancer
{
    public class B_UpgradeMenu : B_UIPage
    {
        [Header("Audio Clips")]
        [SerializeField] private AudioClip OpenUpgradeClip;

        [Header("Upgrade Items")]
        [SerializeField] private B_UpgradeItem[] upgradeItems;

        [Header("Upgrade Menu UI Elements")]
        [SerializeField] private RectTransform screenDim;
        [SerializeField] private RectTransform menu;
        [SerializeField] private RectTransform toPause;
        [SerializeField] private RectTransform upgradeButtonGroup;
        [SerializeField] private RectTransform[] upgradeItemsRect;
        [SerializeField] private bool autoDetectUpgradeItems = true;

        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.4f;
        [SerializeField] private float delayBetweenItems = 0.1f;
        [SerializeField] private Ease easeType = Ease.OutBack;
        [SerializeField] private bool playAnimationOnShow = true;
        [SerializeField] private float itemStaggerDelay = 0.08f;

        [Header("Fade")]
        [SerializeField] private CanvasGroup menuCanvasGroup;
        [SerializeField] private Image screenDimImage;

        private Sequence mainSequence;

        private Vector3 menuOriginalScale;
        private Vector3 toPauseOriginalPos;
        private Vector3 upgradeButtonGroupOriginalPos;
        private Vector3[] upgradeItemsOriginalPos;
        private Vector3[] upgradeItemsOriginalScale;

        private List<B_BaseUpgrade> randomUpgrades = new();
        private B_UpgradeManager _upgradeManager;
        private B_AudioManager audioManager;
        B_BaseUI parent;

        void Awake()
        {
            _upgradeManager = B_UpgradeManager.Instance;
            audioManager = B_AudioManager.Instance;
        }

        public override void Init(B_BaseUI parent)
        {
            base.Init(parent);
            this.parent = parent;
        }

        void Start()
        {
            CacheElementPositions();
        }

        private void CacheElementPositions()
        {
            if (menu != null) menuOriginalScale = menu.localScale;

            if (toPause != null) toPauseOriginalPos = toPause.localPosition;

            if (upgradeButtonGroup != null) upgradeButtonGroupOriginalPos = upgradeButtonGroup.localPosition;

            if (upgradeItemsRect != null && upgradeItemsRect.Length > 0)
            {
                upgradeItemsOriginalPos = new Vector3[upgradeItemsRect.Length];
                upgradeItemsOriginalScale = new Vector3[upgradeItemsRect.Length];

                for (int i = 0; i < upgradeItemsRect.Length; i++)
                {
                    if (upgradeItemsRect[i] != null)
                    {
                        upgradeItemsOriginalPos[i] = upgradeItemsRect[i].localPosition;
                        upgradeItemsOriginalScale[i] = upgradeItemsRect[i].localScale;
                    }
                }
            }
        }

        public override IEnumerator Show()
        {
            canvasGroup.alpha = 1f;
            audioManager.PlaySfx(OpenUpgradeClip);
            randomUpgrades = _upgradeManager.GetRandomUpgradeOptions();
            DisplayUpgradeOptions();

            if (playAnimationOnShow)
            {
                PlayUpgradeAnimation();
            }

            yield return base.Show();
        }

        private void DisplayUpgradeOptions()
        {
            if (upgradeItems == null || upgradeItems.Length == 0)
            {
                return;
            }

            for (int i = 0; i < upgradeItems.Length; i++)
            {
                if (i < randomUpgrades.Count)
                {
                    upgradeItems[i].SetUpgradeInfo(randomUpgrades[i], parent);
                }
                else
                {
                    upgradeItems[i].SetUpgradeInfo(null, parent);
                }
            }
        }

        public override IEnumerator Hide()
        {
            HideUpgradeMenu(() =>
            {
                canvasGroup.alpha = 0f;
            });
            yield return base.Hide();
        }

        public void PlayUpgradeAnimation()
        {
            mainSequence?.Kill();

            ResetElements();

            mainSequence = DOTween.Sequence();

            float currentDelay = 0f;

            if (screenDimImage != null)
            {
                Color c = screenDimImage.color;
                c.a = 0f;
                screenDimImage.color = c;
                mainSequence.Append(screenDimImage.DOFade(0.7f, 0.25f));
                currentDelay = 0.25f;
            }

            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = 0f;
                mainSequence.Insert(currentDelay, menuCanvasGroup.DOFade(1f, 0.3f));
            }

            if (toPause != null)
            {
                toPause.localPosition = toPauseOriginalPos + Vector3.up * 100f;
                mainSequence.Insert(currentDelay, toPause.DOLocalMove(toPauseOriginalPos, animationDuration).SetEase(Ease.OutQuad));
                currentDelay += delayBetweenItems;
            }

            if (upgradeButtonGroup != null)
            {
                upgradeButtonGroup.localPosition = upgradeButtonGroupOriginalPos + Vector3.down * 150f;
                upgradeButtonGroup.localScale = new Vector3(0.8f, 0.8f, 0.8f);

                mainSequence.Insert(currentDelay, upgradeButtonGroup.DOLocalMove(upgradeButtonGroupOriginalPos, animationDuration).SetEase(easeType));
                mainSequence.Insert(currentDelay, upgradeButtonGroup.DOScale(1f, animationDuration).SetEase(easeType));
                currentDelay += delayBetweenItems * 1.5f;
            }

            if (upgradeItemsRect != null && upgradeItemsRect.Length > 0)
            {
                for (int i = 0; i < upgradeItemsRect.Length; i++)
                {
                    if (upgradeItemsRect[i] != null)
                    {
                        upgradeItemsRect[i].localScale = Vector3.zero;
                        upgradeItemsRect[i].localPosition = upgradeItemsOriginalPos[i] + Vector3.down * 50f;

                        float itemDelay = currentDelay + (i * itemStaggerDelay);

                        mainSequence.Insert(itemDelay,
                            upgradeItemsRect[i].DOScale(upgradeItemsOriginalScale[i], animationDuration)
                            .SetEase(Ease.OutQuad));

                        mainSequence.Insert(itemDelay,
                            upgradeItemsRect[i].DOLocalMove(upgradeItemsOriginalPos[i], animationDuration)
                            .SetEase(Ease.OutQuad));
                    }
                }
            }

            mainSequence.OnComplete(() =>
            {
                SetupUpgradeItemsHoverEffect();
            });

            mainSequence.Play();
        }

        private void SetupUpgradeItemsHoverEffect()
        {
            if (upgradeItemsRect != null && upgradeItemsRect.Length > 0)
            {
                for (int i = 0; i < upgradeItemsRect.Length; i++)
                {
                    if (upgradeItemsRect[i] != null)
                    {
                        float delay = i * 0.2f;

                        upgradeItemsRect[i].DOScale(upgradeItemsOriginalScale[i] * 1.03f, 1f)
                            .SetDelay(delay)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine);
                    }
                }
            }
        }

        public void HideUpgradeMenu(System.Action onComplete = null)
        {
            mainSequence?.Kill();

            if (upgradeItemsRect != null && upgradeItemsRect.Length > 0)
            {
                foreach (var item in upgradeItemsRect)
                {
                    if (item != null) item.DOKill();
                }
            }

            mainSequence = DOTween.Sequence();

            if (menuCanvasGroup != null)
            {
                mainSequence.Append(menuCanvasGroup.DOFade(0f, 0.25f));
            }

            if (menu != null)
            {
                mainSequence.Join(menu.DOScale(0.9f, 0.25f).SetEase(Ease.InQuad));
            }

            if (screenDimImage != null)
            {
                mainSequence.Join(screenDimImage.DOFade(0f, 0.25f));
            }

            mainSequence.OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }

        private void ResetElements()
        {
            if (screenDimImage != null)
            {
                Color c = screenDimImage.color;
                c.a = 0f;
                screenDimImage.color = c;
            }

            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = 0f;
            }

            if (upgradeItemsRect != null && upgradeItemsRect.Length > 0)
            {
                foreach (var item in upgradeItemsRect)
                {
                    if (item != null)
                    {
                        item.DOKill();
                    }
                }
            }
        }

        private void OnDestroy()
        {
            mainSequence?.Kill();

            if (upgradeItemsRect != null && upgradeItemsRect.Length > 0)
            {
                foreach (var item in upgradeItemsRect)
                {
                    if (item != null) item.DOKill();
                }
            }
        }
    }
}