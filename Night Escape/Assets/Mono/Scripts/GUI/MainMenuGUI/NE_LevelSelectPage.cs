using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NightEscape
{
    public class NE_LevelSelectPage : NE_UIPage
    {
        [Header("Panel")]
        [SerializeField] Panels _LevelSelectPanel;

        [Header("Buttons")]
        [SerializeField] NE_UIButton _exitBtn;

        [Header("Level")]
        [SerializeField] RectTransform _levelsContainer;
        [SerializeField] Sprite[] levelSprites;

        NE_UIButton[] _levelButtons;
        Image[] _buttonImages;
        NE_BaseUI _parent;
        Vector2 _menuStart;

        public override void Init(NE_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
        }

        void Start()
        {
            if (_levelsContainer != null)
            {
                _levelButtons = _levelsContainer.GetComponentsInChildren<NE_UIButton>();
                _buttonImages = new Image[_levelButtons.Length];

                for (int i = 0; i < _levelButtons.Length; i++)
                {
                    Transform buttonTransform = _levelButtons[i].transform;
                    Image[] childImages = new Image[buttonTransform.childCount];
                    int imageCount = 0;

                    for (int j = 0; j < buttonTransform.childCount; j++)
                    {
                        Image img = buttonTransform.GetChild(j).GetComponent<Image>();
                        if (img != null)
                        {
                            childImages[imageCount] = img;
                            imageCount++;
                        }
                    }

                    if (imageCount > 0)
                    {
                        _buttonImages[i] = childImages[0];
                    }
                }

                for (int i = 0; i < _levelButtons.Length; i++)
                {
                    int level = i + 1;
                    _levelButtons[i].gameObject.name = $"Level {level}";

                    if (i < levelSprites.Length && _buttonImages[i] != null)
                    {
                        _buttonImages[i].sprite = levelSprites[i];
                    }

                    bool isUnlocked = NE_GameManager.Instance.IsLevelUnlocked(level);
                    _levelButtons[i].SetInteractable(isUnlocked);

                    if (isUnlocked)
                    {
                        _levelButtons[i].Bind(() =>
                        {
                            NE_GameManager.Instance.LoadLevelScene(level);
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

        private void RefreshLevelLockStates()
        {
            if (_levelButtons == null) return;

            for (int i = 0; i < _levelButtons.Length; i++)
            {
                int level = i + 1;
                bool isUnlocked = NE_GameManager.Instance.IsLevelUnlocked(level);
                _levelButtons[i].SetInteractable(isUnlocked);
            }
        }
    }
}
