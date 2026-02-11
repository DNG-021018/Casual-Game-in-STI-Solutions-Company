using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VertiblockPass
{
    public class VP_LevelSelectPage : VP_UIPage
    {
        [Header("Panel")]
        [SerializeField] RectTransform _levelsContainer;
        [SerializeField] Panels _LevelSelectPanel;

        [Header("Buttons")]
        [SerializeField] VP_UIButton _exitBtn;

        [SerializeField] private Sprite[] levelInfo;

        VP_UIButton[] _levelButtons;
        Image[] _sprite;

        VP_BaseUI _parent;
        Vector2 _menuStart;

        public override void Init(VP_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
        }

        void Start()
        {
            if (_levelsContainer != null)
            {
                _levelButtons = _levelsContainer.GetComponentsInChildren<VP_UIButton>();
                _sprite = new Image[_levelButtons.Length];

                for (int i = 0; i < _levelButtons.Length; i++)
                {
                    VP_UIButton btn = _levelButtons[i];

                    int level = i + 1;
                    btn.gameObject.name = $"Level {level}";

                    // Try to get the first child Image of the button and assign the sprite from levelInfo
                    Image img = null;
                    if (btn.transform.childCount > 0)
                        img = btn.transform.GetChild(0).GetComponent<Image>();

                    if (img != null && levelInfo != null && levelInfo.Length >= level)
                    {
                        img.sprite = levelInfo[i]; // levelInfo is assumed ordered to match levels
                        _sprite[i] = img;
                    }
                    else
                    {
                        // Fallback to text if no image child or sprite missing
                        btn.SetText(level.ToString());
                    }

                    bool isUnlocked = VP_GameManager.Instance.IsLevelUnlocked(level);
                    btn.SetInteractable(isUnlocked);

                    if (isUnlocked)
                    {
                        int capturedLevel = level;
                        btn.Bind(() =>
                        {
                            VP_GameManager.Instance.currentLevel = capturedLevel;
                            VP_LoadingScreenRoot.Instance.LoadScene("GamePlay");
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
                bool isUnlocked = VP_GameManager.Instance.IsLevelUnlocked(level);
                _levelButtons[i].SetInteractable(isUnlocked);
            }
        }
    }
}
