using System.Collections;
using TMPro;
using UnityEngine;

namespace HexaStack
{
    public class HS_GamePlayPage : HS_UIPage
    {
        [Header("Panel")]
        [SerializeField] Panels gameplayPanel;

        [SerializeField] TextMeshProUGUI scoreText;

        private bool _initializedPos;
        private Vector2 _panelStart;

        public override void Init(HS_BaseUI parent)
        {
            base.Init(parent);

            if (HS_GameManager.Instance != null)
            {
                HS_GameManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            }

            CacheStartPositions();
        }

        void OnDestroy()
        {
            if (HS_GameManager.Instance != null)
            {
                HS_GameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
            }
        }

        protected override void CacheStartPositions()
        {
            if (_initializedPos) return;
            _initializedPos = true;

            if (gameplayPanel.panel != null)
            {
                _panelStart = gameplayPanel.panel.anchoredPosition;
            }
        }

        public void UpdateScoreDisplay(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }
        }

        public override IEnumerator Show(object ctx = null)
        {
            yield return base.Show(ctx);

            Vector2 from = GetOffscreenPos(gameplayPanel.panel, gameplayPanel.slideDir, _panelStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (gameplayPanel.panel, from, _panelStart)
            );
        }

        public override IEnumerator Hide()
        {
            Vector2 to = GetOffscreenPos(gameplayPanel.panel, gameplayPanel.slideDir, _panelStart, offscreenPadding);

            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (gameplayPanel.panel, _panelStart, to)
            );
        }
    }
}
