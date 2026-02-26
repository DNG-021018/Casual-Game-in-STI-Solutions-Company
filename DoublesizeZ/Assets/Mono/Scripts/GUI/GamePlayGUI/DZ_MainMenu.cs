using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_MainMenu : DZ_UIPage
    {
        [Header("References")]
        [SerializeField] private RectTransform coinNotifyPop;

        [Header("Buttons")]
        [SerializeField] DZ_UIButton playButton;
        [SerializeField] DZ_UIButton shopButton;
        [SerializeField] DZ_UIButton dailyRewardButton;
        [SerializeField] DZ_UIButton settingsButton;

        [Header("Tween Elements")]
        [SerializeField] private RectTransform logo;
        [SerializeField] private RectTransform btnPlay;
        [SerializeField] private RectTransform btnShop;
        [SerializeField] private RectTransform btnSettings;
        [SerializeField] private RectTransform btnDailyReward;

        [Header("Tween Settings")]
        [SerializeField] private float popDuration = 0.4f;
        [SerializeField] private float slideDuration = 0.45f;
        [SerializeField] private float popOffsetY = 60f;
        [SerializeField] private float slideOffsetX = 300f;

        private Vector2 _logoPos;
        private Vector2 _btnPlayPos;
        private Vector2 _btnShopPos;
        private Vector2 _btnSettingsPos;
        private Vector2 _btnDailyRewardPos;
        private bool _cached;

        private Sequence _showSeq;
        private Sequence _hideSeq;

        private DZ_CameraManager _cameraManager;
        private DZ_DailyRewardManager _dailyRewardManager;
        private DZ_BaseUI _parent;


        void Awake()
        {
            _cameraManager = ServiceLocator.Get<DZ_CameraManager>();
            _dailyRewardManager = ServiceLocator.Get<DZ_DailyRewardManager>();
        }

        public override void Init(DZ_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
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

        void OnPlayClicked()
        {
            _cameraManager.SwitchToGameplayCamera(() =>
            {
                DZ_GameManager.Instance.GameStart();
            });
        }

        void OnShopClicked()
        {
            _cameraManager.SwitchToShopCamera();
            StartCoroutine(Hide());
            _parent.Open(UIPageId.ShopMenu);
        }

        private void OnDailyRewardClicked()
        {
            _parent.Open(UIPageId.DailyReward);
            canvasGroup.alpha = 1f;
        }

        void OnSettingsClicked()
        {
            StartCoroutine(Hide());
            _parent.Open(UIPageId.SettingsMenu);
        }

        private void CachePositions()
        {
            if (_cached) return;
            _cached = true;

            if (logo != null) _logoPos = logo.anchoredPosition;
            if (btnPlay != null) _btnPlayPos = btnPlay.anchoredPosition;
            if (btnShop != null) _btnShopPos = btnShop.anchoredPosition;
            if (btnSettings != null) _btnSettingsPos = btnSettings.anchoredPosition;
            if (btnDailyReward != null) _btnDailyRewardPos = btnDailyReward.anchoredPosition;
        }

        public override IEnumerator Show()
        {
            CachePositions();
            UpdateNotifyDisplay();
            _cameraManager.SwitchToMenuCamera();

            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            KillTweens();

            if (logo != null)
            {
                logo.anchoredPosition = _logoPos + Vector2.down * popOffsetY;
                logo.localScale = Vector3.one * 0.75f;
            }

            if (btnPlay != null)
            {
                btnPlay.anchoredPosition = _btnPlayPos + Vector2.down * popOffsetY;
                btnPlay.localScale = Vector3.one * 0.75f;
            }

            if (btnShop != null)
            {
                btnShop.anchoredPosition = _btnShopPos + Vector2.down * popOffsetY;
                btnShop.localScale = Vector3.one * 0.75f;
            }

            if (btnSettings != null)
            {
                btnSettings.anchoredPosition = _btnSettingsPos + Vector2.down * popOffsetY;
                btnSettings.localScale = Vector3.one * 0.75f;
            }

            if (btnDailyReward != null)
            {
                btnDailyReward.anchoredPosition = _btnDailyRewardPos + Vector2.left * slideOffsetX;
                btnDailyReward.localScale = Vector3.one;
            }

            _showSeq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            if (logo != null)
            {
                _showSeq.Insert(0f, logo.DOAnchorPos(_logoPos, popDuration)
                    .SetEase(Ease.OutBack, 1.8f));
                _showSeq.Insert(0f, logo.DOScale(1f, popDuration)
                    .SetEase(Ease.OutBack, 1.8f));
            }

            if (btnPlay != null)
            {
                _showSeq.Insert(0.18f, btnPlay.DOAnchorPos(_btnPlayPos, popDuration)
                    .SetEase(Ease.OutBack, 1.8f));
                _showSeq.Insert(0.18f, btnPlay.DOScale(1f, popDuration)
                    .SetEase(Ease.OutBack, 1.8f));
            }

            if (btnDailyReward != null)
            {
                _showSeq.Insert(0.18f, btnDailyReward.DOAnchorPos(_btnDailyRewardPos, slideDuration)
                    .SetEase(Ease.OutCubic));
            }

            if (btnShop != null)
            {
                _showSeq.Insert(0.34f, btnShop.DOAnchorPos(_btnShopPos, popDuration)
                    .SetEase(Ease.OutBack, 1.8f));
                _showSeq.Insert(0.34f, btnShop.DOScale(1f, popDuration)
                    .SetEase(Ease.OutBack, 1.8f));
            }

            if (btnSettings != null)
            {
                _showSeq.Insert(0.34f, btnSettings.DOAnchorPos(_btnSettingsPos, popDuration)
                    .SetEase(Ease.OutBack, 1.8f));
                _showSeq.Insert(0.34f, btnSettings.DOScale(1f, popDuration)
                    .SetEase(Ease.OutBack, 1.8f));
            }

            bool done = false;
            _showSeq.OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                done = true;
            });

            while (!done) yield return null;
        }


        public override IEnumerator Hide()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            KillTweens();
            _hideSeq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            const float hd = 0.2f;

            _hideSeq.Join(canvasGroup.DOFade(0f, hd).SetEase(Ease.InQuad));

            RectTransform[] all = { logo, btnPlay, btnShop, btnSettings, btnDailyReward };
            foreach (var rt in all)
                if (rt != null)
                    _hideSeq.Join(rt.DOScale(0.85f, hd).SetEase(Ease.InQuad));

            bool done = false;
            _hideSeq.OnComplete(() =>
            {
                canvasGroup.alpha = 0f;
                canvasGroup.gameObject.SetActive(false);

                foreach (var rt in all)
                    if (rt != null) rt.localScale = Vector3.one;

                done = true;
            });

            while (!done) yield return null;
            yield return base.Hide();
        }


        private void KillTweens()
        {
            _showSeq?.Kill();
            _hideSeq?.Kill();
        }

        private void UpdateNotifyDisplay()
        {
            if (coinNotifyPop != null)
                coinNotifyPop.gameObject.SetActive(_dailyRewardManager.HasRewardToday());
        }
    }
}
