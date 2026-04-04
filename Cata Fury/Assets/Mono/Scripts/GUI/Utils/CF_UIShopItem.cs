using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CataFury
{
    public class CF_UIShopItem : CF_UIButton, IPointerDownHandler
    {
        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite EnableBackgroundImage;
        [SerializeField] private Sprite DisableBackgroundImage;
        [SerializeField] private Image weaponIcon;
        [SerializeField] private TextMeshProUGUI weaponNameText;

        [Header("Button")]
        [SerializeField] private CF_UIButton buyButton;
        [SerializeField] private Image buttonImage;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Color color;

        [SerializeField] private Sprite BuyButtonSprite;
        [SerializeField] private Sprite EquipButtonSprite;
        [SerializeField] private Sprite AlreadyEquipButtonSprite;

        [Header("Shop SFX")]
        [SerializeField] private AudioClip buySuccessClip;
        [SerializeField] private AudioClip equipSuccessClip;
        [SerializeField] private AudioClip cantBuyClip;

        private ShopConfig _config;
        private Action _onSelectImmediate;

        public void InitInfo(ShopConfig config)
        {
            _config = config;
            if (weaponIcon != null) weaponIcon.sprite = config.itemIcon;
            if (weaponNameText != null) weaponNameText.text = config.itemName;
        }

        public ShopConfig GetInfo() => _config;

        public void BindImmediate(Action onSelect)
        {
            _onSelectImmediate = onSelect;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _onSelectImmediate?.Invoke();
        }

        public void SetSelectedState(bool isSelected)
        {
            if (backgroundImage == null) return;
            backgroundImage.sprite = isSelected ? EnableBackgroundImage : DisableBackgroundImage;
        }

        public void BindActionButton(Action onActionClicked)
        {
            buyButton.UnBind();
            if (onActionClicked != null)
                buyButton.Bind(onActionClicked);
        }

        public void BindBuyButton(Func<bool> onBuy)
        {
            buyButton.UnBind();
            if (onBuy == null) return;

            buyButton.Bind(() =>
            {
                bool success = onBuy.Invoke();
                PlayShopSfx(success ? buySuccessClip : cantBuyClip);
            });
        }

        public void BindEquipButton(Action onEquip)
        {
            buyButton.UnBind();
            if (onEquip == null) return;

            buyButton.Bind(() =>
            {
                onEquip.Invoke();
                PlayShopSfx(equipSuccessClip);
            });
        }

        private void PlayShopSfx(AudioClip clip)
        {
            if (clip == null) return;
            CF_AudioManager audioManager = ServiceLocator.Get<CF_AudioManager>();
            audioManager?.PlaySfx(clip);
        }

        public void SetButtonState(bool isOwned, bool isEquipped)
        {
            if (isEquipped)
            {
                buttonText.text = "Equipped";
                buttonImage.sprite = AlreadyEquipButtonSprite;
                buyButton.SetInteractable(false);
                return;
            }

            buyButton.SetInteractable(true);

            if (!isOwned)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(color);
                buttonText.text = $"Buy <color=#{colorHex}>{_config.itemCost}</color>";
                buttonImage.sprite = BuyButtonSprite;
            }
            else
            {
                buttonText.text = "Equip";
                buttonImage.sprite = EquipButtonSprite;
            }
        }
    }
}