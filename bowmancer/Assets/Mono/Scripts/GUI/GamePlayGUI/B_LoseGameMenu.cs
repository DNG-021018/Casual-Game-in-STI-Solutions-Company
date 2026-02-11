using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

namespace Bowmancer
{
    public class B_LoseGameMenu : B_UIPage
    {
        [Header("Buttons")]
        [SerializeField] private B_UIButton RestartButton;

        [Header("Coin Text")]
        [SerializeField] private TextMeshProUGUI text_Coins;

        [Header("Lose Menu UI Elements")]
        [SerializeField] private RectTransform screenDim;
        [SerializeField] private RectTransform popup;
        [SerializeField] private RectTransform backGlow;
        [SerializeField] private RectTransform deco_Leaf;
        [SerializeField] private RectTransform deco_Arrow;
        [SerializeField] private RectTransform[] deco_Arrows;
        [SerializeField] private RectTransform deco_Skull;
        [SerializeField] private RectTransform frame;
        [SerializeField] private RectTransform ribbon;
        [SerializeField] private RectTransform text_Title;
        [SerializeField] private RectTransform textFrame;
        [SerializeField] private RectTransform button_Continue;

        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private float delayBetweenElements = 0.1f;
        [SerializeField] private Ease easeType = Ease.OutBack;
        [SerializeField] private bool playAnimationOnShow = true;

        [Header("Optional - For Fade")]
        [SerializeField] private CanvasGroup popupCanvasGroup;
        [SerializeField] private Image screenDimImage;

        private Sequence mainSequence;

        private Vector3 decoLeafOriginalPos;
        private Vector3 decoArrowOriginalPos;
        private Vector3[] decoArrowsOriginalPos;
        private Vector3 decoSkullOriginalScale;
        private Vector3 frameOriginalPos;
        private Vector3 ribbonOriginalScale;
        private Vector3 textTitleOriginalScale;
        private Vector3 textFrameOriginalScale;
        private Vector3 buttonContinueOriginalScale;

        private B_BaseUI parent;
        private B_GameManager _gameManager;
        private B_CurrencyManager _currencyManager;

        private void Awake()
        {
            _gameManager = B_GameManager.Instance;
            _currencyManager = B_CurrencyManager.Instance;
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

        void OnEnable()
        {
            if (RestartButton != null)
            {
                RestartButton.Bind(OnRestartClicked);
            }
        }

        void OnDisable()
        {
            if (RestartButton != null)
            {
                RestartButton.UnBind();
            }
        }

        private void OnRestartClicked()
        {
            _gameManager.RestartLevel();
            parent.CloseAll();
        }

        private void CacheElementPositions()
        {
            if (deco_Leaf != null) decoLeafOriginalPos = deco_Leaf.localPosition;
            if (deco_Arrow != null) decoArrowOriginalPos = deco_Arrow.localPosition;
            if (deco_Skull != null) decoSkullOriginalScale = deco_Skull.localScale;
            if (frame != null) frameOriginalPos = frame.localPosition;
            if (ribbon != null) ribbonOriginalScale = ribbon.localScale;
            if (text_Title != null) textTitleOriginalScale = text_Title.localScale;
            if (textFrame != null) textFrameOriginalScale = textFrame.localScale;
            if (button_Continue != null) buttonContinueOriginalScale = button_Continue.localScale;

            if (deco_Arrows != null && deco_Arrows.Length > 0)
            {
                decoArrowsOriginalPos = new Vector3[deco_Arrows.Length];
                for (int i = 0; i < deco_Arrows.Length; i++)
                {
                    if (deco_Arrows[i] != null)
                    {
                        decoArrowsOriginalPos[i] = deco_Arrows[i].localPosition;
                    }
                }
            }
        }

        public override IEnumerator Show()
        {
            canvasGroup.alpha = 1f;

            if (playAnimationOnShow)
            {
                PlayLoseAnimation();
            }
            _currencyManager.AddCoins(1);
            text_Coins.text = "+1";
            yield return base.Show();
        }

        public override IEnumerator Hide()
        {
            HideLoseMenu(() =>
            {
                canvasGroup.alpha = 0f;
            });

            yield return new WaitForSeconds(0.3f);

            yield return base.Hide();
        }

        public void PlayLoseAnimation()
        {
            mainSequence?.Kill();

            ResetElements();

            mainSequence = DOTween.Sequence();

            if (screenDimImage != null)
            {
                Color c = screenDimImage.color;
                c.a = 0f;
                screenDimImage.color = c;
                mainSequence.Append(screenDimImage.DOFade(0.85f, 0.4f));
            }

            if (popup != null && popupCanvasGroup != null)
            {
                popup.localScale = Vector3.zero;
                popupCanvasGroup.alpha = 0f;

                mainSequence.Append(popup.DOScale(1f, animationDuration * 1.2f).SetEase(Ease.OutQuad));
                mainSequence.Join(popupCanvasGroup.DOFade(1f, animationDuration * 1.2f));
            }

            float delay = 0.3f;

            if (backGlow != null)
            {
                backGlow.localScale = Vector3.zero;
                mainSequence.Insert(delay, backGlow.DOScale(1f, animationDuration * 1.3f).SetEase(Ease.OutQuad));
                delay += delayBetweenElements;
            }

            if (deco_Leaf != null)
            {
                deco_Leaf.localPosition = decoLeafOriginalPos + Vector3.up * 300f;
                deco_Leaf.localRotation = Quaternion.Euler(0, 0, Random.Range(-30f, 30f));
                mainSequence.Insert(delay, deco_Leaf.DOLocalMove(decoLeafOriginalPos, animationDuration * 1.5f).SetEase(Ease.OutQuad));
                mainSequence.Insert(delay, deco_Leaf.DOLocalRotate(Vector3.zero, animationDuration * 1.5f).SetEase(Ease.OutQuad));
                delay += delayBetweenElements;
            }

            if (deco_Arrows != null && deco_Arrows.Length > 0)
            {
                for (int i = 0; i < deco_Arrows.Length; i++)
                {
                    if (deco_Arrows[i] != null)
                    {
                        Vector3 offset = new Vector3(
                            Random.Range(-150f, 150f),
                            300f,
                            0
                        );
                        deco_Arrows[i].localPosition = decoArrowsOriginalPos[i] + offset;
                        deco_Arrows[i].localScale = Vector3.zero;
                        deco_Arrows[i].localRotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));

                        mainSequence.Insert(delay + (i * 0.05f), deco_Arrows[i].DOLocalMove(decoArrowsOriginalPos[i], animationDuration * 1.2f).SetEase(Ease.OutQuad));
                        mainSequence.Insert(delay + (i * 0.05f), deco_Arrows[i].DOScale(1f, animationDuration * 1.2f).SetEase(Ease.OutQuad));
                        mainSequence.Insert(delay + (i * 0.05f), deco_Arrows[i].DOLocalRotate(Vector3.zero, animationDuration * 1.2f).SetEase(Ease.OutQuad));
                    }
                }
                delay += delayBetweenElements * 1.5f;
            }
            else if (deco_Arrow != null)
            {
                deco_Arrow.localPosition = decoArrowOriginalPos + Vector3.up * 300f;
                deco_Arrow.localRotation = Quaternion.Euler(0, 0, Random.Range(-45f, 45f));
                mainSequence.Insert(delay, deco_Arrow.DOLocalMove(decoArrowOriginalPos, animationDuration * 1.2f).SetEase(Ease.OutQuad));
                mainSequence.Insert(delay, deco_Arrow.DOLocalRotate(Vector3.zero, animationDuration * 1.2f).SetEase(Ease.OutQuad));
                delay += delayBetweenElements;
            }

            if (deco_Skull != null)
            {
                deco_Skull.localScale = Vector3.zero;
                mainSequence.Insert(delay, deco_Skull.DOScale(decoSkullOriginalScale, animationDuration).SetEase(Ease.OutBack));
                mainSequence.Insert(delay + animationDuration, deco_Skull.DOShakePosition(0.3f, strength: 10f, vibrato: 10));
                delay += delayBetweenElements;
            }

            if (ribbon != null)
            {
                ribbon.localScale = new Vector3(0f, ribbonOriginalScale.y, ribbonOriginalScale.z);
                mainSequence.Insert(delay, ribbon.DOScaleX(ribbonOriginalScale.x, animationDuration * 1.3f).SetEase(Ease.OutQuad));
                delay += delayBetweenElements;
            }

            if (text_Title != null)
            {
                text_Title.localScale = Vector3.zero;
                mainSequence.Insert(delay, text_Title.DOScale(textTitleOriginalScale, animationDuration * 1.2f).SetEase(Ease.OutQuad));
                delay += delayBetweenElements;
            }

            if (frame != null)
            {
                frame.localPosition = frameOriginalPos + Vector3.down * 300f;
                mainSequence.Insert(delay, frame.DOLocalMove(frameOriginalPos, animationDuration * 1.3f).SetEase(Ease.OutQuad));
                delay += delayBetweenElements;
            }

            if (textFrame != null)
            {
                textFrame.localScale = Vector3.zero;
                mainSequence.Insert(delay, textFrame.DOScale(textFrameOriginalScale, animationDuration * 1.2f).SetEase(Ease.OutQuad));
            }

            if (button_Continue != null)
            {
                button_Continue.localScale = Vector3.zero;

                mainSequence.OnComplete(() =>
                {
                    if (button_Continue != null)
                    {
                        DOVirtual.DelayedCall(0.5f, () =>
                        {
                            if (button_Continue != null)
                            {
                                button_Continue.DOScale(buttonContinueOriginalScale, 0.4f)
                                    .SetEase(Ease.OutQuad)
                                    .OnComplete(() =>
                                    {
                                        if (button_Continue != null)
                                        {
                                            button_Continue.DOScale(buttonContinueOriginalScale * 1.05f, 0.8f)
                                                .SetLoops(-1, LoopType.Yoyo)
                                                .SetEase(Ease.InOutSine);
                                        }
                                    });
                            }
                        });
                    }
                });
            }

            mainSequence.Play();
        }

        public void HideLoseMenu(System.Action onComplete = null)
        {
            mainSequence?.Kill();
            mainSequence = DOTween.Sequence();

            if (popupCanvasGroup != null)
            {
                mainSequence.Append(popupCanvasGroup.DOFade(0f, 0.3f));
            }

            if (popup != null)
            {
                mainSequence.Join(popup.DOScale(0.8f, 0.3f).SetEase(Ease.InBack));
            }

            if (screenDimImage != null)
            {
                mainSequence.Join(screenDimImage.DOFade(0f, 0.3f));
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

            if (popupCanvasGroup != null)
            {
                popupCanvasGroup.alpha = 0f;
            }

            if (popup != null)
            {
                popup.localScale = Vector3.zero;
            }
        }

        private void OnDestroy()
        {
            mainSequence?.Kill();
            if (button_Continue != null) button_Continue.DOKill();
        }
    }
}