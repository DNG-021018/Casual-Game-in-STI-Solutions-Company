using System.Collections;
using DG.Tweening;
using UnityEngine;
using TMPro;

namespace DoublesideZ
{
    public class DZ_LoseGameMenu : DZ_UIPage
    {
        [Header("Buttons")]
        [SerializeField] private DZ_UIButton RestartButton;

        [Header("Score UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        [Header("Tween Elements")]
        [SerializeField] private RectTransform popup;
        [SerializeField] private RectTransform wowImage;
        [SerializeField] private CanvasGroup midPanel;
        [SerializeField] private RectTransform continueButton;

        [Header("Tween Settings")]
        [SerializeField] private float popupDuration = 0.45f;
        [SerializeField] private float wowOffsetY = 60f;

        private Vector2 _wowStartPos;
        private Vector2 _continueStartPos;
        private bool _cached;
        private Sequence _showSeq;

        private DZ_BaseUI parent;
        private DZ_GameManager _gameManager;
        private DZ_ScoreManager _scoreManager;

        private void Awake()
        {
            _gameManager = DZ_GameManager.Instance;
            _scoreManager = ServiceLocator.Get<DZ_ScoreManager>();
        }

        public override void Init(DZ_BaseUI parent)
        {
            base.Init(parent);
            this.parent = parent;
        }

        void Start() => CachePositions();

        void OnEnable() { if (RestartButton != null) RestartButton.Bind(OnRestartClicked); }
        void OnDisable() { if (RestartButton != null) RestartButton.UnBind(); }

        private void OnRestartClicked() => _gameManager.RestartGame();

        private void CachePositions()
        {
            if (_cached) return;
            _cached = true;
            if (wowImage != null) _wowStartPos = wowImage.anchoredPosition;
            if (continueButton != null) _continueStartPos = continueButton.anchoredPosition;
        }

        private void RefreshScoreUI()
        {
            if (_scoreManager == null) return;
            if (scoreText != null) scoreText.text = _scoreManager.CurrentScore.ToString();
            if (highScoreText != null) highScoreText.text = _scoreManager.HighScore.ToString();
        }

        public override IEnumerator Show()
        {
            CachePositions();
            RefreshScoreUI();

            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            if (popup != null) popup.localScale = Vector3.zero;
            if (wowImage != null)
            {
                wowImage.anchoredPosition = _wowStartPos + Vector2.up * wowOffsetY;
                wowImage.localScale = Vector3.one * 0.5f;
            }
            if (midPanel != null) midPanel.alpha = 0f;
            if (continueButton != null) continueButton.localScale = Vector3.zero;

            _showSeq?.Kill();
            _showSeq = DOTween.Sequence().SetLink(gameObject);
            if (popup != null)
                _showSeq.Insert(0f, popup.DOScale(1f, popupDuration)
                    .SetEase(Ease.OutBack, 1.2f));
            if (wowImage != null)
            {
                _showSeq.Insert(0.20f, wowImage.DOAnchorPos(_wowStartPos, popupDuration)
                    .SetEase(Ease.OutBounce));
                _showSeq.Insert(0.20f, wowImage.DOScale(1f, popupDuration * 0.6f)
                    .SetEase(Ease.OutBack));
            }
            if (midPanel != null)
                _showSeq.Insert(0.30f, midPanel.DOFade(1f, popupDuration * 0.7f)
                    .SetEase(Ease.OutQuad));
            if (continueButton != null)
                _showSeq.Insert(0.50f, continueButton.DOScale(1f, popupDuration * 0.7f)
                    .SetEase(Ease.OutBack, 2f));

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
            _showSeq?.Kill();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            if (popup != null)
            {
                bool done = false;
                popup.DOScale(0f, 0.2f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => done = true);
                while (!done) yield return null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);
            if (popup != null) popup.localScale = Vector3.one;
            if (wowImage != null) wowImage.localScale = Vector3.one;
            if (continueButton != null) continueButton.localScale = Vector3.one;
            if (midPanel != null) midPanel.alpha = 1f;

            yield return base.Hide();
        }
    }
}
