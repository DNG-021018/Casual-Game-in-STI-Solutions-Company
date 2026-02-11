using System.Collections;
using UnityEngine;

namespace CubeSokoban
{
    public class CS_LevelSelectPage : CS_UIPage
    {
        [Header("Panel")]
        [SerializeField] RectTransform _levelsContainer;
        [SerializeField] Panels _LevelSelectPanel;

        [Header("Buttons")]
        [SerializeField] CS_UIButton _exitBtn;

        CS_UIButton[] _levelButtons;
        CS_BaseUI _parent;
        Vector2 _menuStart;

        public override void Init(CS_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
        }

        void Start()
        {
            if (_levelsContainer != null)
            {
                _levelButtons = _levelsContainer.GetComponentsInChildren<CS_UIButton>();

                for (int i = 0; i < _levelButtons.Length; i++)
                {
                    CS_UIButton btn = _levelButtons[i];

                    int level = i + 1;
                    btn.gameObject.name = $"Level {level}";
                    btn.SetText(level.ToString());

                    bool isUnlocked = CS_GameManager.Instance.IsLevelUnlocked(level);
                    btn.SetInteractable(isUnlocked);

                    if (isUnlocked)
                    {
                        btn.Bind(() =>
                        {
                            CS_GameManager.Instance.currentLevel = level;
                            CS_LoadingScreenRoot.Instance.LoadScene("GamePlay");
                        });
                    }
                }
            }

            if (_exitBtn != null) _exitBtn.Bind(() => _parent.Back());
        }

        void OnDestroy()
        {
            if (_exitBtn != null) _exitBtn.UnBind();
        }

        public override IEnumerator Show(object ctx = null)
        {
            RefreshLevelLockStates();

            Vector2 from = GetOffscreenPos(_LevelSelectPanel.panel, _LevelSelectPanel.slideDir, _menuStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (_LevelSelectPanel.panel, from, _menuStart)
            );
        }

        public override IEnumerator Hide()
        {
            Vector2 to = GetOffscreenPos(_LevelSelectPanel.panel, _LevelSelectPanel.slideDir, _menuStart, offscreenPadding);

            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (_LevelSelectPanel.panel, _menuStart, to)
            );
        }

        public override void ApplyContext(object ctx)
        {
        }

        private void RefreshLevelLockStates()
        {
            if (_levelButtons == null) return;

            for (int i = 0; i < _levelButtons.Length; i++)
            {
                int level = i + 1;
                bool isUnlocked = CS_GameManager.Instance.IsLevelUnlocked(level);
                _levelButtons[i].SetInteractable(isUnlocked);
            }
        }
    }
}
