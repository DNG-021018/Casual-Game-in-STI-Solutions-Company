using System.Collections;
using UnityEngine;

namespace CubeSokoban
{
    public class CS_MainMenuPage : CS_UIPage
    {
        [SerializeField] Panels topPanel;
        [SerializeField] Panels bottomPanel;

        [Header("Bind buttons")]
        [SerializeField] CS_UIButton startBtn;
        [SerializeField] CS_UIButton tutorialBtn;
        [SerializeField] CS_UIButton settingsBtn;

        CS_BaseUI _parent;

        Vector2 _topStart;
        Vector2 _bottomStart;
        private bool _initializedPos;
        private bool _initFistime = false;

        public override void Init(CS_BaseUI parent)
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
            if (!_initFistime)
            {
                CacheStartPositions();

                Vector2 tfrom = GetOffscreenPos(topPanel.panel, topPanel.slideDir, _topStart, offscreenPadding);
                Vector2 bfrom = GetOffscreenPos(bottomPanel.panel, bottomPanel.slideDir, _bottomStart, offscreenPadding);

                yield return ShowMovePanels(
                    duration, showEase, 0f, 1f,
                    (topPanel.panel, tfrom, _topStart),
                    (bottomPanel.panel, bfrom, _bottomStart)
                );
                _initFistime = true;
            }
            else
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
                yield break;
            }
        }

        public override IEnumerator Hide()
        {
            canvasGroup.blocksRaycasts = false;
            yield break;
            // canvasGroup.alpha = 0f;
        }
    }
}
