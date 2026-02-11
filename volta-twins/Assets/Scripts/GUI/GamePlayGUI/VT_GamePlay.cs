using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VoltaTwins
{
    public class VT_GamePlay : VT_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels RightPanel;
        // [SerializeField] Panels LeftPanel;

        [Header("Button")]
        [SerializeField] VT_UIButton shootButton;
        [SerializeField] VT_UIButton pauseButton;
        [SerializeField] VT_UIButton shootBtn;

        [Header("Level Image Display")]
        [SerializeField] Image levelImage;
        [SerializeField] List<Sprite> levelSprites;

        Vector2 _rightStart;
        // Vector2 _leftStart;

        bool _initializedPos;
        VT_LevelManager levelmanager;

        public static event Action Shoot = delegate { };

        public override void Init(VT_BaseUI parent)
        {
            base.Init(parent);
            CacheStartPositions();
            levelmanager = VT_LevelManager.Instance;
        }

        void Start()
        {
            if (shootButton != null)
            {
                shootButton.Bind(() =>
                {
                    if (levelmanager.isGameFinish) return;

                    Shoot.Invoke();
                    StartCoroutine(Hide());
                });
            }

            if (pauseButton != null)
            {
                pauseButton.Bind(() => VT_GameManager.Instance?.SetState(GameState.Paused));
            }

            if (shootBtn != null)
            {
                shootBtn.Bind(() =>
                {
                    levelmanager.OnShoot.Invoke();
                });
            }

            if (levelImage != null)
            {
                if (VT_GameManager.Instance != null) levelImage.sprite = levelSprites[VT_GameManager.Instance.currentLevel - 1];
            }
        }

        void OnDestroy()
        {
            if (shootButton != null)
            {
                shootButton.UnBind();
            }

            if (pauseButton != null)
            {
                pauseButton.UnBind();
            }

            if (shootBtn != null)
            {
                shootBtn.UnBind();
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
