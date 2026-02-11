using System.Collections;
using UnityEngine;

namespace NightEscape
{
    public class NE_MainMenuPage : NE_UIPage
    {
        [SerializeField] Panels topPanel;
        [SerializeField] Panels bottomPanel;

        [Header("Bind buttons")]
        [SerializeField] NE_UIButton startBtn;
        [SerializeField] NE_UIButton tutorialBtn;
        [SerializeField] NE_UIButton settingsBtn;

        NE_BaseUI _parent;

        Vector2 _topStart;
        Vector2 _bottomStart;
        private bool _initializedPos;

        public override void Init(NE_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
            startBtn.Bind(() => _parent.Open(UIPageId.LevelSelect));
            tutorialBtn.Bind(() => _parent.Open(UIPageId.Tutorial));
            settingsBtn.Bind(() => _parent.Open(UIPageId.Settings));
        }

        private void OnDestroy()
        {
            startBtn.UnBind();
            tutorialBtn.UnBind();
            settingsBtn.UnBind();
        }
        protected override void CacheStartPositions()
        {
            if (_initializedPos) return;
            if (topPanel.panel) _topStart = topPanel.panel.anchoredPosition;
            if (bottomPanel.panel) _bottomStart = bottomPanel.panel.anchoredPosition;
            _initializedPos = true;
        }

        public override IEnumerator Show(object ctx = null)
        {
            CacheStartPositions();

            Vector2 tfrom = GetOffscreenPos(topPanel.panel, topPanel.slideDir, _topStart, offscreenPadding);
            Vector2 bfrom = GetOffscreenPos(bottomPanel.panel, bottomPanel.slideDir, _bottomStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (topPanel.panel, tfrom, _topStart),
                (bottomPanel.panel, bfrom, _bottomStart)
            );
        }

        public override IEnumerator Hide()
        {
            Vector2 tto = GetOffscreenPos(topPanel.panel, topPanel.slideDir, _topStart, offscreenPadding);
            Vector2 bto = GetOffscreenPos(bottomPanel.panel, bottomPanel.slideDir, _bottomStart, offscreenPadding);

            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (topPanel.panel, _topStart, tto),
                (bottomPanel.panel, _bottomStart, bto)
            );
        }
    }
}
