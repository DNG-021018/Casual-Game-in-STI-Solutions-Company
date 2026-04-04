using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace CataFury
{
    public class CF_MainMenu : CF_UIPage
    {
        [Header("References")]
        [SerializeField] private RectTransform coinNotifyPop;

        [Header("Tween Targets")]
        [SerializeField] private RectTransform logoRect;
        [SerializeField] private RectTransform buttonGroupRect;
        [SerializeField] private CanvasGroup settingsGroup;

        [Header("Tween Settings")]
        [SerializeField] private float tweenDuration = 0.4f;
        [SerializeField] private float logoOffsetY = 60f;
        [SerializeField] private float btnOffsetY = 50f;

        [Header("Buttons")]
        [SerializeField] CF_UIButton playButton;
        [SerializeField] CF_UIButton shopButton;
        [SerializeField] CF_UIButton dailyRewardButton;
        [SerializeField] CF_UIButton settingsButton;

        private CF_DailyRewardManager _dailyRewardManager;
        private CF_BaseUI _parent;

        private Vector2 _logoOrigin;
        private Vector2 _btnGroupOrigin;

        void Awake()
        {
            _dailyRewardManager = ServiceLocator.Get<CF_DailyRewardManager>();
        }

        public override void Init(CF_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;

            if (logoRect != null) _logoOrigin = logoRect.anchoredPosition;
            if (buttonGroupRect != null) _btnGroupOrigin = buttonGroupRect.anchoredPosition;
        }

        void OnEnable()
        {
            playButton.Bind(OnPlayClicked);
            shopButton.Bind(OnShopClicked);
            dailyRewardButton.Bind(OnDailyRewardClicked);
            settingsButton.Bind(OnSettingsClicked);
        }

        void OnDisable()
        {
            playButton.UnBind();
            shopButton.UnBind();
            dailyRewardButton.UnBind();
            settingsButton.UnBind();
        }


        void OnPlayClicked() => CF_GameManager.Instance.GameStart();
        void OnShopClicked() { StartCoroutine(Hide()); _parent.Open(UIPageId.ShopMenu); }
        void OnDailyRewardClicked() { _parent.Open(UIPageId.DailyReward); canvasGroup.alpha = 1f; }
        void OnSettingsClicked() { StartCoroutine(Hide()); _parent.Open(UIPageId.SettingsMenu); }


        public override IEnumerator Show()
        {
            UpdateNotifyDisplay();

            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.blocksRaycasts = false;

            if (logoRect != null)
                logoRect.anchoredPosition = _logoOrigin + Vector2.up * logoOffsetY;

            if (buttonGroupRect != null)
                buttonGroupRect.anchoredPosition = _btnGroupOrigin + Vector2.down * btnOffsetY;

            if (settingsGroup != null)
                settingsGroup.alpha = 0f;

            Sequence seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            seq.Join(canvasGroup.DOFade(1f, tweenDuration).SetEase(Ease.OutQuad));

            if (logoRect != null)
                seq.Join(logoRect
                    .DOAnchorPos(_logoOrigin, tweenDuration)
                    .SetEase(Ease.OutCubic));

            if (buttonGroupRect != null)
                seq.Join(buttonGroupRect
                    .DOAnchorPos(_btnGroupOrigin, tweenDuration)
                    .SetEase(Ease.OutCubic));

            if (settingsGroup != null)
                seq.Join(settingsGroup
                    .DOFade(1f, tweenDuration * 0.8f)
                    .SetEase(Ease.OutQuad)
                    .SetDelay(tweenDuration * 0.2f));

            bool done = false;
            seq.OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = true;
                done = true;
            });

            while (!done) yield return null;

            yield return base.Show();
        }


        public override IEnumerator Hide()
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.gameObject.SetActive(false);

            if (logoRect != null) logoRect.anchoredPosition = _logoOrigin;
            if (buttonGroupRect != null) buttonGroupRect.anchoredPosition = _btnGroupOrigin;
            if (settingsGroup != null) settingsGroup.alpha = 1f;

            yield return base.Hide();
        }


        private void UpdateNotifyDisplay()
        {
            if (coinNotifyPop != null)
                coinNotifyPop.gameObject.SetActive(_dailyRewardManager.HasRewardToday());
        }
    }
}