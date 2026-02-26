using UnityEngine;
using UnityEngine.UI;

namespace DoublesideZ
{
    public class DZ_ShopEquipButton : DZ_UIButton
    {
        [SerializeField] private Sprite equipSprite;
        [SerializeField] private Sprite unequipSprite;

        private Image image;

        void Awake()
        {
            image = GetComponent<Image>();
        }

        public void SetEquipState(bool isEquipped)
        {
            image = GetComponent<Image>();
            if (image != null)
            {
                image.sprite = !isEquipped ? equipSprite : unequipSprite;
            }
        }
    }
}
