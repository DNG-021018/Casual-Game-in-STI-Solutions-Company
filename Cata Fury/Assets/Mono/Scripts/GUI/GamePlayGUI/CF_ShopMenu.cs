using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CataFury
{
    public class CF_ShopMenu : CF_UIPage
    {
        [Header("Tween")]
        [SerializeField] Panels shopMenu;
        private Vector2 _originalPanelPos;

        [Header("References")]
        [SerializeField] TextMeshProUGUI coinText;

        [Header("Shop Item UI References")]
        [SerializeField] CF_UIButton closeShopButton;
        [SerializeField] Transform shopItemContainer;
        [SerializeField] CF_UIShopItem shopItemPrefab;
        [SerializeField] Image ItemsPreviewImage;

        private CF_ShopManager _weaponManager;
        private CF_CurrencyManager _currencyManager;
        private List<CF_UIShopItem> _shopItems;
        private CF_UIShopItem _selectedShopItem;
        private CF_BaseUI _parent;
        private CF_PopupManager _popupManager;
        private ShopItemType _equippedBeforeShop;

        void Awake()
        {
            _weaponManager = ServiceLocator.Get<CF_ShopManager>();
            _currencyManager = ServiceLocator.Get<CF_CurrencyManager>();
            _popupManager = ServiceLocator.Get<CF_PopupManager>();
            InitItems();
        }

        public override void Init(CF_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
            CacheStartPositions();
        }

        public void InitItems()
        {
            List<ShopConfig> weapons = _weaponManager.GetAllItems();
            _shopItems = new List<CF_UIShopItem>();

            foreach (ShopConfig cfg in weapons)
            {
                ShopItemType id = cfg.id;
                CF_UIShopItem item = Instantiate(shopItemPrefab, shopItemContainer);
                item.InitInfo(cfg);

                item.BindImmediate(() =>
                {
                    UpdateSelectionHighlight(id);
                    UpdatePreviewImage(id);
                });

                item.Bind(() => OnShopItemClicked(id));

                _shopItems.Add(item);
            }

            OnShopItemClicked(_weaponManager.GetEquipped());
        }

        void OnEnable()
        {
            _currencyManager.OnCoinsChanged += RefreshShopItems;
            _weaponManager.OnItemEquipped += OnRealEquipChanged;
            closeShopButton.Bind(OnCloseShopButtonClicked);
        }

        void OnDisable()
        {
            _currencyManager.OnCoinsChanged -= RefreshShopItems;
            _weaponManager.OnItemEquipped -= OnRealEquipChanged;
            closeShopButton.UnBind();
        }

        private void OnRealEquipChanged(ShopItemType weaponType)
        {
            RefreshAllItemStates();
            UpdateSelectionHighlight(weaponType);
        }

        private void UpdatePreviewImage(ShopItemType weaponID)
        {
            if (ItemsPreviewImage == null) return;
            ShopConfig config = _weaponManager.GetConfig(weaponID);
            ItemsPreviewImage.sprite = config.itemIcon;
        }

        private void OnShopItemClicked(ShopItemType weaponID)
        {
            _weaponManager.PreviewItem(weaponID);
        }


        private bool OnBuyButtonClicked(ShopItemType id)
        {
            if (_weaponManager.Unlock(id))
            {
                _equippedBeforeShop = _weaponManager.GetEquipped();
                RefreshAllItemStates();
                return true;
            }
            else
            {
                _popupManager.ShowTopNotification("Not enough coins!", Color.red);
                return false;
            }
        }

        private void OnEquipButtonClicked(ShopItemType id)
        {
            if (!_weaponManager.IsEquipped(id))
            {
                _weaponManager.Equip(id);
                _equippedBeforeShop = id;
            }
        }

        private void OnCloseShopButtonClicked()
        {
            _weaponManager.PreviewItem(_equippedBeforeShop);
            StartCoroutine(Hide());
            _parent.Open(UIPageId.Mainmenu);
        }


        public void RefreshShopItems(int coinsAmount = 0)
        {
            coinText.text = _currencyManager.GetCoins().ToString();
            RefreshAllItemStates();
        }

        private void RefreshAllItemStates()
        {
            foreach (CF_UIShopItem item in _shopItems)
            {
                ShopConfig config = item.GetInfo();
                ShopItemType id = config.id;
                bool isUnlocked = _weaponManager.IsUnlocked(id);
                bool isEquipped = _weaponManager.IsEquipped(id);

                item.SetButtonState(isUnlocked, isEquipped);

                if (!isUnlocked)
                {
                    // Dùng BindBuyButton → tự play buySuccess hoặc cantBuy sound
                    item.BindBuyButton(() => OnBuyButtonClicked(id));
                }
                else if (!isEquipped)
                {
                    // Dùng BindEquipButton → tự play equip sound
                    item.BindEquipButton(() => OnEquipButtonClicked(id));
                }
                else
                {
                    item.BindActionButton(null);
                }
            }
        }

        private void UpdateSelectionHighlight(ShopItemType weaponID)
        {
            CF_UIShopItem target = _shopItems.Find(item => item.GetInfo().id == weaponID);

            if (_selectedShopItem != null)
                _selectedShopItem.SetSelectedState(false);

            _selectedShopItem = target;

            if (_selectedShopItem != null)
                _selectedShopItem.SetSelectedState(true);
        }

        protected override void CacheStartPositions()
        {
            if (shopMenu.panel != null)
                _originalPanelPos = shopMenu.panel.anchoredPosition;
        }

        public override IEnumerator Show()
        {
            coinText.text = _currencyManager.GetCoins().ToString();
            _equippedBeforeShop = _weaponManager.GetEquipped();

            RefreshAllItemStates();
            UpdateSelectionHighlight(_equippedBeforeShop);
            UpdatePreviewImage(_equippedBeforeShop);
            OnShopItemClicked(_equippedBeforeShop);

            Vector2 offscreen = GetOffscreenPos(shopMenu.panel, SlideDir.Up, _originalPanelPos, shopMenu.offscreenPadding);

            yield return ShowMovePanels(
                shopMenu.duration, shopMenu.showEase,
                0f, 1f,
                (shopMenu.panel, offscreen, _originalPanelPos)
            );
        }

        public override IEnumerator Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.gameObject.SetActive(false);

            if (shopMenu.panel != null)
                shopMenu.panel.anchoredPosition = _originalPanelPos;

            yield break;
        }
    }
}
