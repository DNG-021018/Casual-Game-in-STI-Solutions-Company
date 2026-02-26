using UnityEngine;
using UnityEngine.UI;

namespace DoublesideZ
{
    public class DZ_UIShopItem : DZ_UIButton
    {
        [SerializeField] private Image EnableBackgroundImage;
        [SerializeField] private Image DisableBackgroundImage;

        [SerializeField] private Image weaponIcon;
        [SerializeField] private Image lockOverlay;

        WeaponConfig _config;

        public void InitInfo(WeaponConfig config)
        {
            _config = config;
            weaponIcon.sprite = config.weaponIcon;
        }

        public WeaponConfig GetInfo() => _config;

        public void SetButtonState(bool isLocked)
        {
            if (lockOverlay != null)
            {
                lockOverlay.gameObject.SetActive(isLocked);
            }
        }

        public void SetSelectedState(bool isSelected)
        {
            if (EnableBackgroundImage != null)
                EnableBackgroundImage.gameObject.SetActive(isSelected);

            if (DisableBackgroundImage != null)
                DisableBackgroundImage.gameObject.SetActive(!isSelected);
        }
    }
}
