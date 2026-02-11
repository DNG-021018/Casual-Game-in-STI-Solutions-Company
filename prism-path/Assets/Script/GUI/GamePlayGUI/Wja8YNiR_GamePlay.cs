using System;
using System.Collections;
using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_GamePlay : Wja8YNiR_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels RightPanel;
        // [SerializeField] Panels LeftPanel;

        [Header("Button")]
        [SerializeField] Wja8YNiR_UIButton shootButton;

        Vector2 _rightStart;
        // Vector2 _leftStart;

        bool _initializedPos;

        public static event Action Shoot = delegate { };

        public override void Init(Wja8YNiR_BaseUI parent)
        {
            base.Init(parent);
            CacheStartPositions();
        }

        void Start()
        {
            if (shootButton != null)
            {
                shootButton.Bind(() =>
                {
                    if (Wja8YNiR_LevelManager.Instance.isGameFinish) return;
                    Wja8YNiR_GameManager.Instance.SetState(GameState.Shooting);
                    Shoot.Invoke();
                    StartCoroutine(Hide());
                });
            }
        }

        void OnDestroy()
        {
            if (shootButton != null)
            {
                shootButton.UnBind();
            }
        }

        protected override void CacheStartPositions()
        {
            if (_initializedPos) return;
            if (RightPanel.panel) _rightStart = RightPanel.panel.anchoredPosition;
            // if (LeftPanel.panel) _leftStart = LeftPanel.panel.anchoredPosition;
            _initializedPos = true;
        }

        public override IEnumerator Show(object ctx = null)
        {
            CacheStartPositions();

            Vector2 rFrom = GetOffscreenPos(RightPanel.panel, RightPanel.slideDir, _rightStart, offscreenPadding);
            // Vector2 lFrom = GetOffscreenPos(LeftPanel.panel, LeftPanel.slideDir, _leftStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (RightPanel.panel, rFrom, _rightStart)
            // (LeftPanel.panel, lFrom, _leftStart)
            );
        }

        public override IEnumerator Hide()
        {
            CacheStartPositions();

            Vector2 rTo = GetOffscreenPos(RightPanel.panel, RightPanel.slideDir, _rightStart, offscreenPadding);
            // Vector2 lTo = GetOffscreenPos(LeftPanel.panel, LeftPanel.slideDir, _leftStart, offscreenPadding);

            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (RightPanel.panel, _rightStart, rTo)
            // (LeftPanel.panel, _leftStart, lTo)
            );
        }
    }
}
