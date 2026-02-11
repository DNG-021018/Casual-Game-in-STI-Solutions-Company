using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CubeSokoban
{
    public class CS_GamePlay : CS_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels TopPanel;

        [Header("Button")]
        [SerializeField] CS_UIButton pauseButton;

        [Header("Top Panel")]
        [SerializeField] private Image Sprite;
        [SerializeField] private Sprite[] levelInfo;

        Vector2 _rightStart;

        bool _initializedPos;

        public static event Action Shoot = delegate { };

        public override void Init(CS_BaseUI parent)
        {
            base.Init(parent);
            CacheStartPositions();
            Sprite.sprite = levelInfo[CS_GameManager.Instance.currentLevel - 1];
        }

        void Start()
        {
            if (pauseButton != null)
            {
                pauseButton.Bind(() => CS_GameManager.Instance?.SetState(GameState.Paused));
            }
        }

        void OnDestroy()
        {
            if (pauseButton != null)
            {
                pauseButton.UnBind();
            }
        }

        protected override void CacheStartPositions()
        {
            if (_initializedPos) return;
            if (TopPanel.panel) _rightStart = TopPanel.panel.anchoredPosition;
            _initializedPos = true;
        }

        public override IEnumerator Show(object ctx = null)
        {
            CacheStartPositions();

            Vector2 rFrom = GetOffscreenPos(TopPanel.panel, TopPanel.slideDir, _rightStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (TopPanel.panel, rFrom, _rightStart)
            );
        }

        public override IEnumerator Hide()
        {
            base.Hide();
            yield break;
        }
    }
}
