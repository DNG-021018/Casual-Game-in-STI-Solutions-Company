using System.Collections;
using TMPro;
using UnityEngine;

namespace CB_CubeRunner
{
    public class CB_FinishGamePage : CB_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels LogoPanel;
        [SerializeField] Panels ScorePanel;
        [SerializeField] Panels HighestScorePanel;
        [SerializeField] Panels ButtonPanel;

        [Header("Text Mesh Pro")]
        [SerializeField] TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI highScoreText;

        [Header("Buttons")]
        [SerializeField] CB_UIButton homeButton;

        [Header("Audio")]
        [SerializeField] AudioClip audioClip;

        CB_BaseUI _parent;

        Vector2 _logoStart;
        Vector2 _scoreStart;
        Vector2 _highestScoreStart;
        Vector2 _buttonStart;

        CB_AudioManager audioManager;

        public override void Init(CB_BaseUI parent)
        {
            base.Init(parent);
            CacheStartPositions();
            _parent = parent;

            homeButton.Bind(() =>
            {
                CB_GameManager.Instance?.SetState(GameState.Initialize);
            });

            base.Init(parent);
        }

        void Start()
        {
            audioManager = CB_AudioManager.Instance;
        }

        void OnDestroy()
        {
            homeButton.UnBind();
        }

        override protected void CacheStartPositions()
        {
            if (LogoPanel.panel != null)
            {
                _logoStart = LogoPanel.panel.anchoredPosition;
            }

            if (ScorePanel.panel != null)
            {
                _scoreStart = ScorePanel.panel.anchoredPosition;
            }

            if (HighestScorePanel.panel != null)
            {
                _highestScoreStart = HighestScorePanel.panel.anchoredPosition;
            }

            if (ButtonPanel.panel != null)
            {
                _buttonStart = ButtonPanel.panel.anchoredPosition;
            }
        }

        public override IEnumerator Show(object ctx = null)
        {

            if (CB_GameManager.Instance != null)
            {
                if (scoreText) scoreText.text = CB_GameManager.Instance.CurrentPoint.ToString();
                if (highScoreText) highScoreText.text = CB_GameManager.Instance.GetHighScore().ToString();
            }

            if (audioManager) audioManager.PlaySfx(audioClip);

            Vector2 lfrom = GetOffscreenPos(LogoPanel.panel, LogoPanel.slideDir, _logoStart, offscreenPadding);
            Vector2 sfrom = GetOffscreenPos(ScorePanel.panel, ScorePanel.slideDir, _scoreStart, offscreenPadding);
            Vector2 hfrom = GetOffscreenPos(HighestScorePanel.panel, HighestScorePanel.slideDir, _highestScoreStart, offscreenPadding);
            Vector2 bfrom = GetOffscreenPos(ButtonPanel.panel, ButtonPanel.slideDir, _buttonStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (LogoPanel.panel, lfrom, _logoStart),
                (ScorePanel.panel, sfrom, _scoreStart),
                (HighestScorePanel.panel, hfrom, _highestScoreStart),
                (ButtonPanel.panel, bfrom, _buttonStart)
            );
        }

        public override IEnumerator Hide()
        {
            Vector2 lTo = GetOffscreenPos(LogoPanel.panel, LogoPanel.slideDir, _logoStart, offscreenPadding);
            Vector2 sTo = GetOffscreenPos(ScorePanel.panel, ScorePanel.slideDir, _scoreStart, offscreenPadding);
            Vector2 hTo = GetOffscreenPos(HighestScorePanel.panel, HighestScorePanel.slideDir, _highestScoreStart, offscreenPadding);
            Vector2 bTo = GetOffscreenPos(ButtonPanel.panel, ButtonPanel.slideDir, _buttonStart, offscreenPadding);

            yield return HideMovePanels(
                duration, showEase, 1f, 0f,
                (LogoPanel.panel, _logoStart, lTo),
                (ScorePanel.panel, _scoreStart, sTo),
                (HighestScorePanel.panel, _highestScoreStart, hTo),
                (ButtonPanel.panel, _buttonStart, bTo)
            );
        }
    }
}