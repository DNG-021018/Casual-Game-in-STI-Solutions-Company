using System;
using System.Collections;
using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_MirrorControlPanel : Wja8YNiR_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels BottomPanel;

        [Header("Button")]
        [SerializeField] Wja8YNiR_UIButton rotateButton;
        [SerializeField] Wja8YNiR_UIButton deleteButton;

        Vector2 _bottomStart;
        private Wja8YNiR_Mirror _selectedMirror;
        private Wja8YNiR_Tile _selectedTile;

        public static event Action<bool> OnReset = delegate { };

        bool _initializedPos;

        public override void Init(Wja8YNiR_BaseUI parent)
        {
            base.Init(parent);
            CacheStartPositions();
            Wja8YNiR_Mirror.OnMirrorSelected += HandleMirrorSelected;
            Wja8YNiR_Tile.OnTileSelected += HandleTileSelected;
        }

        void Start()
        {
            if (rotateButton != null)
            {
                rotateButton.Bind(() =>
                {
                    _selectedMirror?.Rotate45Degrees();
                });
            }

            if (deleteButton != null)
            {
                deleteButton.Bind(() =>
                {
                    _selectedTile?.ResetTile();
                    Destroy(_selectedMirror?.gameObject);
                    OnReset.Invoke(false);
                    Wja8YNiR_GameManager.Instance?.SetState(GameState.Playing);
                });
            }
        }

        void OnDestroy()
        {
            if (rotateButton != null)
            {
                rotateButton.UnBind();
            }

            if (deleteButton != null)
            {
                deleteButton.UnBind();
            }

            Wja8YNiR_Mirror.OnMirrorSelected -= HandleMirrorSelected;
            Wja8YNiR_Tile.OnTileSelected -= HandleTileSelected;
        }

        private void HandleTileSelected(Wja8YNiR_Tile tile)
        {
            _selectedTile = tile;
        }

        private void HandleMirrorSelected(Wja8YNiR_Mirror m)
        {
            _selectedMirror = m;
            _selectedMirror.HightLight();
        }

        protected override void CacheStartPositions()
        {
            if (_initializedPos) return;
            if (BottomPanel.panel) _bottomStart = BottomPanel.panel.anchoredPosition;
            _initializedPos = true;
        }

        public override IEnumerator Show(object ctx = null)
        {
            CacheStartPositions();
            Vector2 bFrom = GetOffscreenPos(BottomPanel.panel, BottomPanel.slideDir, _bottomStart, offscreenPadding);
            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (BottomPanel.panel, bFrom, _bottomStart)
            );
        }

        public override IEnumerator Hide()
        {
            CacheStartPositions();
            Vector2 bTo = GetOffscreenPos(BottomPanel.panel, BottomPanel.slideDir, _bottomStart, offscreenPadding);
            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (BottomPanel.panel, _bottomStart, bTo)
            );
        }
    }
}
