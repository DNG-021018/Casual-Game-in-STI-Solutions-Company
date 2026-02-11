using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

namespace Bowmancer
{
    public class B_WinGameMenu : B_UIPage
    {
        [Header("Panel Root")]
        [SerializeField] Panels Panel;

        [Header("Buttons")]
        [SerializeField] private B_UIButton ContinueButton;
        [SerializeField]
        private B_UIButton RestartButton;

        [Header("Coin Text")]
        [SerializeField] private TextMeshProUGUI text_Coins;

        [Header("Victory Menu UI Elements")]
        [SerializeField] private RectTransform screenDim;
        [SerializeField] private RectTransform popup;
        [SerializeField] private RectTransform imageEffect;
        [SerializeField] private RectTransform backGlow;
        [SerializeField] private RectTransform deco_Leaf;
        [SerializeField] private RectTransform deco_Trumpet;
        [SerializeField] private RectTransform[] deco_Trumpets;
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

        private Vector3 imageEffectOriginalPos;
        private Vector3 backGlowOriginalPos;
        private Vector3 decoLeafOriginalPos;
        private Vector3 decoTrumpetOriginalPos;
        private Vector3[] decoTrumpetsOriginalPos;
        private Vector3 frameOriginalPos;
        private Vector3 ribbonOriginalScale;
        private Vector3 textTitleOriginalScale;
        private Vector3 decoSkullOriginalScale;
        private Vector3 textFrameOriginalScale;
        private Vector3 buttonContinueOriginalScale;

        private B_UIManager _UIManager;
        private B_GameManager _gameManager;
        private B_CurrencyManager _currencyManager;
        private B_BaseUI parent;

        private void Awake()
        {
            _UIManager = B_UIManager.Instance;
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
            _UIManager.OnCoinChanged += SetCoinText;

            if (ContinueButton != null)
            {
                ContinueButton.Bind(OnContinueClicked);
            }

            if (RestartButton != null)
            {
                RestartButton.Bind(OnRestartClicked);
            }
        }

        void OnDisable()
        {
            _UIManager.OnCoinChanged -= SetCoinText;

            if (ContinueButton != null)
            {
                ContinueButton.UnBind();
            }

            if (RestartButton != null)
            {
                RestartButton.UnBind();
            }
        }

        private void OnDestroy()
        {
            mainSequence?.Kill();
            if (backGlow != null) backGlow.DOKill();
            if (button_Continue != null) button_Continue.DOKill();
        }

        private void OnContinueClicked()
        {
            if (_gameManager.CheckNextLevelInvalid())
            {
                B_LoadingScreenRoot.Instance.LoadSceneWithName(_gameManager.GetLevelSceneName(_gameManager.CurrentLevel));
            }
            else
            {
                _gameManager.LoadNextLevel();
            }

            parent.CloseAll();
        }

        private void OnRestartClicked()
        {
            _gameManager.RestartLevel();
            parent.CloseAll();
        }

        public void SetCoinText(int coin)
        {
            if (text_Coins != null)
            {
                text_Coins.text = coin.ToString();
            }
        }

        private void CacheElementPositions()
        {
            if (imageEffect != null) imageEffectOriginalPos = imageEffect.localPosition;
            if (backGlow != null) backGlowOriginalPos = backGlow.localPosition;
            if (deco_Leaf != null) decoLeafOriginalPos = deco_Leaf.localPosition;
            if (deco_Trumpet != null) decoTrumpetOriginalPos = deco_Trumpet.localPosition;
            if (frame != null) frameOriginalPos = frame.localPosition;
            if (ribbon != null) ribbonOriginalScale = ribbon.localScale;
            if (text_Title != null) textTitleOriginalScale = text_Title.localScale;
            if (deco_Skull != null) decoSkullOriginalScale = deco_Skull.localScale;
            if (textFrame != null) textFrameOriginalScale = textFrame.localScale;
            if (button_Continue != null) buttonContinueOriginalScale = button_Continue.localScale;
            if (deco_Trumpets != null && deco_Trumpets.Length > 0)
            {
                decoTrumpetsOriginalPos = new Vector3[deco_Trumpets.Length];
                for (int i = 0; i < deco_Trumpets.Length; i++)
                {
                    if (deco_Trumpets[i] != null)
                    {
                        decoTrumpetsOriginalPos[i] = deco_Trumpets[i].localPosition;
                    }
                }
            }
        }

        public override IEnumerator Show()
        {
            canvasGroup.alpha = 1f;

            if (playAnimationOnShow)
            {
                PlayVictoryAnimation();
            }
            _currencyManager.AddCoins(15);
            text_Coins.text = "+15";
            yield return base.Show();
        }

        public override IEnumerator Hide()
        {
            HideVictoryMenu(() =>
            {
                canvasGroup.alpha = 0f;
            });

            yield return new WaitForSeconds(0.3f);

            yield return base.Hide();
        }

        public void PlayVictoryAnimation()
        {
            mainSequence?.Kill();

            ResetElements();

            mainSequence = DOTween.Sequence();

            if (screenDimImage != null)
            {
                Color c = screenDimImage.color;
                c.a = 0f;
                screenDimImage.color = c;
                mainSequence.Append(screenDimImage.DOFade(0.8f, 0.3f));
            }

            if (popup != null && popupCanvasGroup != null)
            {
                popup.localScale = Vector3.zero;
                popupCanvasGroup.alpha = 0f;

                mainSequence.Append(popup.DOScale(1f, animationDuration).SetEase(Ease.OutBack));
                mainSequence.Join(popupCanvasGroup.DOFade(1f, animationDuration));
            }

            float delay = 0.2f;

            if (imageEffect != null)
            {
                imageEffect.localPosition = imageEffectOriginalPos + Vector3.up * 200f;
                mainSequence.Insert(delay, imageEffect.DOLocalMove(imageEffectOriginalPos, animationDuration).SetEase(easeType));
                delay += delayBetweenElements;
            }

            if (backGlow != null)
            {
                backGlow.localScale = Vector3.zero;
                mainSequence.Insert(delay, backGlow.DOScale(1f, animationDuration).SetEase(Ease.OutElastic));
                backGlow.DORotate(new Vector3(0, 0, 360), 20f, RotateMode.FastBeyond360)
                    .SetLoops(-1, LoopType.Restart)
                    .SetEase(Ease.Linear);
                delay += delayBetweenElements;
            }

            if (deco_Leaf != null)
            {
                deco_Leaf.localPosition = decoLeafOriginalPos + Vector3.left * 300f;
                mainSequence.Insert(delay, deco_Leaf.DOLocalMove(decoLeafOriginalPos, animationDuration).SetEase(easeType));
                delay += delayBetweenElements;
            }

            if (deco_Trumpets != null && deco_Trumpets.Length > 0)
            {
                for (int i = 0; i < deco_Trumpets.Length; i++)
                {
                    if (deco_Trumpets[i] != null)
                    {
                        Vector3 offset = new Vector3(
                            Random.Range(-300f, 300f),
                            Random.Range(-300f, 300f),
                            0
                        );
                        deco_Trumpets[i].localPosition = decoTrumpetsOriginalPos[i] + offset;
                        deco_Trumpets[i].localScale = Vector3.zero;

                        mainSequence.Insert(delay, deco_Trumpets[i].DOLocalMove(decoTrumpetsOriginalPos[i], animationDuration).SetEase(easeType));
                        mainSequence.Insert(delay, deco_Trumpets[i].DOScale(1f, animationDuration).SetEase(easeType));
                    }
                }
                delay += delayBetweenElements;
            }
            else if (deco_Trumpet != null)
            {
                deco_Trumpet.localPosition = decoTrumpetOriginalPos + Vector3.right * 300f;
                mainSequence.Insert(delay, deco_Trumpet.DOLocalMove(decoTrumpetOriginalPos, animationDuration).SetEase(easeType));
                delay += delayBetweenElements;
            }

            if (ribbon != null)
            {
                ribbon.localScale = new Vector3(0f, ribbonOriginalScale.y, ribbonOriginalScale.z);
                mainSequence.Insert(delay, ribbon.DOScaleX(ribbonOriginalScale.x, animationDuration).SetEase(Ease.OutBack));
                delay += delayBetweenElements;
            }

            if (text_Title != null)
            {
                text_Title.localScale = Vector3.zero;
                mainSequence.Insert(delay, text_Title.DOScale(textTitleOriginalScale, animationDuration).SetEase(Ease.OutBounce));
                delay += delayBetweenElements;
            }

            if (frame != null)
            {
                frame.localPosition = frameOriginalPos + Vector3.down * 300f;
                mainSequence.Insert(delay, frame.DOLocalMove(frameOriginalPos, animationDuration).SetEase(easeType));

                mainSequence.Append(frame.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.5f));
            }

            if (deco_Skull != null)
            {
                deco_Skull.localScale = Vector3.zero;
                mainSequence.Insert(delay + 0.1f, deco_Skull.DOScale(decoSkullOriginalScale, animationDuration).SetEase(Ease.OutBack));
            }

            if (textFrame != null)
            {
                textFrame.localScale = Vector3.zero;
                mainSequence.Insert(delay + 0.2f, textFrame.DOScale(textFrameOriginalScale, animationDuration).SetEase(Ease.OutBack));
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

        public void HideVictoryMenu(System.Action onComplete = null)
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
    }
}