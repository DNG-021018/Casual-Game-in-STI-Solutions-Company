using System.Collections;
using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_LevelSelectPage : Wja8YNiR_UIPage
    {
        [Header("Panel")]
        [SerializeField] RectTransform _levelsContainer;
        [SerializeField] Panels _LevelSelectPanel;

        [Header("Buttons")]
        [SerializeField] Wja8YNiR_UIButton _nextBtn;
        [SerializeField] Wja8YNiR_UIButton _prevBtn;
        [SerializeField] Wja8YNiR_UIButton _exitBtn;
        [SerializeField] Wja8YNiR_UIHorizontalPager _pager;

        Wja8YNiR_UIButton[] _levelButtons;
        Wja8YNiR_BaseUI _parent;
        Vector2 _menuStart;

        public override void Init(Wja8YNiR_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
        }

        void Start()
        {
            if (_levelsContainer != null)
            {
                _levelButtons = _levelsContainer.GetComponentsInChildren<Wja8YNiR_UIButton>();

                for (int i = 0; i < _levelButtons.Length; i++)
                {
                    Wja8YNiR_UIButton btn = _levelButtons[i];

                    int level = i + 1;
                    btn.gameObject.name = $"Level {level}";
                    btn.SetText(level.ToString());

                    // Kiểm tra xem level có được mở chưa
                    bool isUnlocked = Wja8YNiR_GameManager.Instance.IsLevelUnlocked(level);
                    btn.SetInteractable(isUnlocked);

                    // Chỉ bind action nếu level đã mở
                    if (isUnlocked)
                    {
                        btn.Bind(() =>
                        {
                            Wja8YNiR_GameManager.Instance.currentLevel = level;
                            Wja8YNiR_LoadingScreenRoot.Instance.LoadScene("GamePlay");
                        });
                    }
                }
            }

            if (_nextBtn != null && _pager != null) _nextBtn.Bind(() => _pager.Next());
            if (_prevBtn != null && _pager != null) _prevBtn.Bind(() => _pager.Prev());
            if (_exitBtn != null && _pager != null) _exitBtn.Bind(() => _parent.Back());
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
            // Refresh trạng thái khóa khi show lại page
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

        // Refresh lại trạng thái khóa của các level
        private void RefreshLevelLockStates()
        {
            if (_levelButtons == null) return;

            for (int i = 0; i < _levelButtons.Length; i++)
            {
                int level = i + 1;
                bool isUnlocked = Wja8YNiR_GameManager.Instance.IsLevelUnlocked(level);
                _levelButtons[i].SetInteractable(isUnlocked);
            }
        }
    }
}