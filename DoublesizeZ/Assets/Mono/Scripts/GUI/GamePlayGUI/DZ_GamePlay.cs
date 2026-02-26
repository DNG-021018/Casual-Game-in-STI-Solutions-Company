using System.Collections;
using TMPro;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_GamePlay : DZ_UIPage
    {
        [Header("Buttons")]
        [SerializeField] DZ_UIButton pauseButton;

        [Header("Score UI")]
        [SerializeField] TextMeshProUGUI scoreText;

        DZ_BaseUI parent;
        DZ_ScoreManager _scoreManager;

        void Awake()
        {
            _scoreManager = ServiceLocator.Get<DZ_ScoreManager>();
        }

        public override void Init(DZ_BaseUI parent)
        {
            base.Init(parent);
            this.parent = parent;
        }

        void OnEnable()
        {
            pauseButton.Bind(() =>
            {
                parent.Open(UIPageId.PauseMenu);
            });

            if (_scoreManager != null)
            {
                _scoreManager.OnScoreChanged += UpdateScoreUI;
            }

            RefreshScoreUI();
        }

        void OnDisable()
        {
            pauseButton.UnBind();

            if (_scoreManager != null)
            {
                _scoreManager.OnScoreChanged -= UpdateScoreUI;
            }
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

        public override IEnumerator Show()
        {
            canvasGroup.alpha = 1f;
            RefreshScoreUI();
            yield return base.Show();
        }

        public override IEnumerator Hide()
        {
            canvasGroup.alpha = 0f;
            yield return base.Hide();
        }
    }
}