using System.Collections;
using UnityEngine;

namespace VoltaTwins
{
    public class VT_LevelSelectPage : VT_UIPage
    {
        [Header("Panel")]
        [SerializeField] RectTransform _levelsContainer;
        [SerializeField] Panels _LevelSelectPanel;

        [Header("Buttons")]
        [SerializeField] VT_UIButton _nextBtn;
        [SerializeField] VT_UIButton _prevBtn;
        [SerializeField] VT_UIButton _exitBtn;
        [SerializeField] VT_UIHorizontalPager _pager;

        VT_UIButton[] _levelButtons;
        VT_BaseUI _parent;
        Vector2 _menuStart;

        public override void Init(VT_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
        }

        void Start()
        {
            if (_levelsContainer != null)
            {
                _levelButtons = _levelsContainer.GetComponentsInChildren<VT_UIButton>();

                for (int i = 0; i < _levelButtons.Length; i++)
                {
                    VT_UIButton btn = _levelButtons[i];

                    int level = i + 1;
                    btn.gameObject.name = $"Level {level}";
                    btn.SetText(level.ToString());

                    bool isUnlocked = VT_GameManager.Instance.IsLevelUnlocked(level);
                    btn.SetInteractable(isUnlocked);

                    if (isUnlocked)
                    {
                        btn.Bind(() =>
                        {
                            VT_GameManager.Instance.currentLevel = level;
                            VT_LoadingScreenRoot.Instance.LoadScene("GamePlay");
                        });
                    }
                }
            }

            if (_nextBtn != null && _pager != null) _nextBtn.Bind(() => _pager.Next());
            if (_prevBtn != null && _pager != null) _prevBtn.Bind(() => _pager.Prev());
            if (_exitBtn != null) _exitBtn.Bind(() => _parent.Back());
            if (_pager != null) _pager.JumpTo(0, true);
        }

        void OnDestroy()
        {
            if (_nextBtn != null) _nextBtn.UnBind();
            if (_prevBtn != null) _prevBtn.UnBind();
            if (_exitBtn != null) _exitBtn.UnBind();
        }

        public override IEnumerator Show(object ctx = null)
        {
            RefreshLevelLockStates();
            if (_pager != null) _pager.JumpTo(0, true);

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
            if (ctx is int i) _pager.JumpTo(i, true);
        }

        private void RefreshLevelLockStates()
        {
            if (_levelButtons == null) return;

            for (int i = 0; i < _levelButtons.Length; i++)
            {
                int level = i + 1;
                bool isUnlocked = VT_GameManager.Instance.IsLevelUnlocked(level);
                _levelButtons[i].SetInteractable(isUnlocked);
            }
        }
    }
}
