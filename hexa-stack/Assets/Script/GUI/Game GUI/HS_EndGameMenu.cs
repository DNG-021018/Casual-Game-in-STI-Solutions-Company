using System.Collections;
using UnityEngine;
using TMPro;

namespace HexaStack
{
    public class HS_EndGameMenu : HS_UIPage
    {
        [Header("Panel Root")]
        [SerializeField] Panels Panel;

        [Header("Score")]
        [SerializeField] TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI highScoreText;

        [Header("Button")]
        [SerializeField] HS_UIButton replayBtn;
        [SerializeField] HS_UIButton returnMainMenu;

        [Header("Clip")]
        [SerializeField] AudioClip AwakeClip;

        private Vector2 _panelStartPos;

        public override void Init(HS_BaseUI parent)
        {
            base.Init(parent);
            CacheStartPositions();
        }

        protected override void CacheStartPositions()
        {
            if (Panel.panel != null)
            {
                _panelStartPos = Panel.panel.anchoredPosition;
            }
        }

        void Start()
        {
            if (replayBtn != null)
                replayBtn.Bind(() =>
                {
                    HS_GameManager.Instance.SetState(GameState.Initialize);
                    HS_GameManager.Instance.StartGame();
                });

            if (returnMainMenu != null)
                returnMainMenu.Bind(() =>
                {
                    HS_GameManager.Instance.SetState(GameState.Initialize);
                    HS_GameManager.Instance.ResetGame();
                });

            if (HS_GameManager.Instance != null)
            {
                HS_GameManager.Instance.OnScoreChanged += UpdateScoreDisplay;
                HS_GameManager.Instance.OnHighScoreChanged += UpdateHighScoreDisplay;
            }
        }

        void OnDestroy()
        {
            if (replayBtn != null) replayBtn.UnBind();
            if (returnMainMenu != null) returnMainMenu.UnBind();

            if (HS_GameManager.Instance != null)
            {
                HS_GameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
                HS_GameManager.Instance.OnHighScoreChanged -= UpdateHighScoreDisplay;
            }
        }

        private void UpdateScoreDisplay(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"{score}";
            }
        }

        private void UpdateHighScoreDisplay(int highScore)
        {
            if (highScoreText != null)
            {
                highScoreText.text = $"{highScore}";
            }
        }

        public override IEnumerator Show(object ctx = null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            if (Panel.panel != null)
            {
                Panel.panel.anchoredPosition = _panelStartPos;
            }

            if (HS_AudioManager.Instance && AwakeClip)
            {
                HS_AudioManager.Instance.PlaySfx(AwakeClip);
            }

            if (HS_GameManager.Instance != null)
            {
                UpdateScoreDisplay(HS_GameManager.Instance.GetScore());
                UpdateHighScoreDisplay(HS_GameManager.Instance.GetHighestScore());
            }

            yield return ShowScalePanels(
                duration, hideEase, 0f, 1f,
                (Panel.panel, Vector3.zero, Vector3.one)
            );
        }

        public override IEnumerator Hide()
        {
            Vector2 to = GetOffscreenPos(Panel.panel, Panel.slideDir, _panelStartPos, offscreenPadding);

            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (Panel.panel, _panelStartPos, to)
            );
        }
    }
}
