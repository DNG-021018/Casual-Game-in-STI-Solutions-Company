using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bowmancer
{
    public class B_ShopUpgradeItem : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Image BackgroundImage;
        [SerializeField] private Image IconImage;
        [SerializeField] private TextMeshProUGUI PowerName;
        [SerializeField] private TextMeshProUGUI LevelName;

        [Header("Selected Settings")]
        [SerializeField] private Sprite SelectedIcon;

        [Header("UnSelected Settings")]
        [SerializeField] private Sprite UnSelectedIcon;

        [SerializeField] private B_UIButton _button;

        private PermanentUpgradeType _upgradeType;

        void Awake()
        {
            _button = GetComponent<B_UIButton>();
        }

        public void bindButton(Action onClick)
        {
            if (_button != null)
            {
                _button.Bind(onClick);
            }
        }

        public void UnBind()
        {
            if (_button != null)
            {
                _button.UnBind();
            }
        }

        public void SetUpgradeData(PermanentUpgradeType upgradeType, Sprite icon, string powerName, int currentLevel)
        {
            _upgradeType = upgradeType;
            IconImage.sprite = icon;
            PowerName.text = powerName;
            LevelName.text = $"Level: {currentLevel}";
        }

        public void SetData(Sprite icon, string powerName, string levelName)
        {
            IconImage.sprite = icon;
            PowerName.text = powerName;
            LevelName.text = levelName;
        }

        public void SetSelected(bool selected)
        {
            BackgroundImage.sprite = selected ? SelectedIcon : UnSelectedIcon;
        }

        public PermanentUpgradeType GetUpgradeType()
        {
            return _upgradeType;
        }
    }
}