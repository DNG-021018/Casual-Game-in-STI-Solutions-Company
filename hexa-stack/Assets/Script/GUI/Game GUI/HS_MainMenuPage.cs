using System.Collections;
using UnityEngine;

namespace HexaStack
{
    public class HS_MainMenuPage : HS_UIPage
    {
        [Header("Bind buttons")]
        [SerializeField] HS_UIButton startBtn;

        [Header("Panel")]
        [SerializeField] Panels menuPanel;

        private bool _initializedPos;
        private Vector2 _menuStart;

        public override void Init(HS_BaseUI parent)
        {
            base.Init(parent);
            startBtn.Bind(() => HS_GameManager.Instance.StartGame());
            CacheStartPositions();
        }

        private void OnDestroy()
        {
            startBtn.UnBind();
        }

        protected override void CacheStartPositions()
        {
            if (_initializedPos) return;
            _initializedPos = true;

            if (menuPanel.panel != null)
            {
                _menuStart = menuPanel.panel.anchoredPosition;
            }
        }

        public override IEnumerator Show(object ctx = null)
        {
            yield return base.Show(ctx);

            Vector2 from = GetOffscreenPos(menuPanel.panel, menuPanel.slideDir, _menuStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (menuPanel.panel, from, _menuStart)
            );
        }

        public override IEnumerator Hide()
        {
            Vector2 to = GetOffscreenPos(menuPanel.panel, menuPanel.slideDir, _menuStart, offscreenPadding);

            yield return HideMovePanels(
                0.1f, hideEase, 1f, 0f,
                (menuPanel.panel, _menuStart, to)
            );
        }
    }
}
