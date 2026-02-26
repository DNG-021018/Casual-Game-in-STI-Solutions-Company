using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_ShopMenu : DZ_UIPage
    {
        [Header("References")]
        [SerializeField] TextMeshProUGUI coinText;

        [Header("Shop Item UI References")]
        [SerializeField] TextMeshProUGUI weaponNameText;
        [SerializeField] TextMeshProUGUI weaponPriceText;
        [SerializeField] DZ_ShopEquipButton equipButton;
        [SerializeField] DZ_UIButton buyButton;
        [SerializeField] DZ_UIButton closeShopButton;

        [SerializeField] Transform shopItemContainer;
        [SerializeField] DZ_UIShopItem shopItemPrefab;

        private DZ_WeaponManager _weaponManager;
        private DZ_CurrencyManager _currencyManager;
        private List<DZ_UIShopItem> _shopItems;
        private WeaponType _selectedWeaponID;
        private DZ_UIShopItem _selectedShopItem;
        private DZ_CameraManager _cameraManager;
        private DZ_BaseUI parent;
        private DZ_PopupManager _popupManager;

        private WeaponType _equippedBeforeShop;

        [Header("Tween Elements")]
        [SerializeField] private RectTransform panelMenu;
        [SerializeField] private RectTransform panelShop;

        [Header("Tween Settings")]
        [SerializeField] private float slideDuration = 0.5f;
        [SerializeField] private float shopDelay = 0.12f;

        private Vector2 _menuStartPos;
        private Vector2 _shopStartPos;
        private bool _cached;
        private Sequence _showSeq;
        private Sequence _hideSeq;

        private float OffscreenLeft(RectTransform rt)
        {
            if (rt == null) return 0f;
            var parent = rt.parent as RectTransform;
            float parentW = parent != null ? parent.rect.width : Screen.width;
            float selfW = rt.rect.width;
            return parentW + selfW + 50f;
        }

        void Awake()
        {
            _weaponManager = ServiceLocator.Get<DZ_WeaponManager>();
            _currencyManager = ServiceLocator.Get<DZ_CurrencyManager>();
            _cameraManager = ServiceLocator.Get<DZ_CameraManager>();
            _popupManager = ServiceLocator.Get<DZ_PopupManager>();
            InitItems();
        }

        public override void Init(DZ_BaseUI parent)
        {
            base.Init(parent);
            this.parent = parent;
        }

        public void InitItems()
        {
            List<WeaponConfig> weapons = _weaponManager.GetAllWeapons();
            _shopItems = new List<DZ_UIShopItem>();

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponType id = weapons[i].weaponID;

                DZ_UIShopItem item = Instantiate(shopItemPrefab, shopItemContainer);
                item.InitInfo(weapons[i]);
                item.Bind(() => OnShopItemClicked(id));
                _shopItems.Add(item);
            }

            OnShopItemClicked(_weaponManager.GetEquipped());
        }

        void OnEnable()
        {
            _currencyManager.OnCoinsChanged += RefreshShopItems;
            _weaponManager.OnWeaponEquipped += OnRealEquipChanged;

            buyButton.Bind(OnBuyButtonClicked);
            equipButton.Bind(OnEquipButtonClicked);
            closeShopButton.Bind(OnCloseShopButtonClicked);
        }

        void OnDisable()
        {
            _currencyManager.OnCoinsChanged -= RefreshShopItems;
            _weaponManager.OnWeaponEquipped -= OnRealEquipChanged;

            buyButton.UnBind();
            equipButton.UnBind();
            closeShopButton.UnBind();
        }

        private void OnRealEquipChanged(WeaponType weaponType)
        {
            RefreshAllItemStates();
            UpdateSelectionUI(weaponType);
        }

        private void OnShopItemClicked(WeaponType weaponID)
        {
            _selectedWeaponID = weaponID;

            UpdateSelectionUI(weaponID);

            _weaponManager.PreviewWeapon(weaponID);
        }

        private void UpdateSelectionUI(WeaponType weaponID)
        {
            DZ_UIShopItem clickedItem = _shopItems.Find(item => item.GetInfo().weaponID == weaponID);

            if (_selectedShopItem != null)
                _selectedShopItem.SetSelectedState(false);

            if (clickedItem != null)
            {
                _selectedShopItem = clickedItem;
                _selectedShopItem.SetSelectedState(true);
            }

            WeaponConfig config = _weaponManager.GetConfig(weaponID);

            weaponNameText.text = config.weaponName;
            weaponPriceText.text = config.weaponCost.ToString();

            bool isUnlocked = _weaponManager.IsUnlocked(weaponID);
            bool isEquipped = _weaponManager.IsEquipped(weaponID);

            buyButton.gameObject.SetActive(!isUnlocked);
            equipButton.gameObject.SetActive(isUnlocked);

            if (isUnlocked)
            {
                equipButton.SetEquipState(isEquipped);
            }
        }

        private void OnBuyButtonClicked()
        {
            if (_weaponManager.Unlock(_selectedWeaponID))
            {
                _equippedBeforeShop = _weaponManager.GetEquipped();
                RefreshAllItemStates();
            }
            else
            {
                _popupManager.ShowTopNotification("Not enough coins!", Color.red);
            }
        }

        private void OnEquipButtonClicked()
        {
            if (!_weaponManager.IsEquipped(_selectedWeaponID))
            {
                _weaponManager.Equip(_selectedWeaponID);
                _equippedBeforeShop = _selectedWeaponID;
            }
        }

        private void OnCloseShopButtonClicked()
        {
            _weaponManager.PreviewWeapon(_equippedBeforeShop);
            StartCoroutine(Hide());

            _cameraManager.SwitchToMenuCamera(() =>
            {
                parent.Open(UIPageId.Mainmenu);
            });
        }

        public void RefreshShopItems(int coinsAmount = 0)
        {
            coinText.text = _currencyManager.GetCoins().ToString();
            RefreshAllItemStates();
        }

        private void RefreshAllItemStates()
        {
            foreach (DZ_UIShopItem item in _shopItems)
            {
                WeaponConfig config = item.GetInfo();
                bool isUnlocked = _weaponManager.IsUnlocked(config.weaponID);
                item.SetButtonState(!isUnlocked);
            }
        }

        public override IEnumerator Show()
        {
            coinText.text = _currencyManager.GetCoins().ToString();
            _equippedBeforeShop = _weaponManager.GetEquipped();

            RefreshAllItemStates();
            OnShopItemClicked(_equippedBeforeShop);

            CachePositions();

            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            if (panelMenu != null) panelMenu.anchoredPosition = _menuStartPos + Vector2.left * OffscreenLeft(panelMenu);
            if (panelShop != null) panelShop.anchoredPosition = _shopStartPos + Vector2.left * OffscreenLeft(panelShop);

            KillTweens();
            _showSeq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            if (panelMenu != null)
                _showSeq.Insert(0f, panelMenu.DOAnchorPos(_menuStartPos, slideDuration).SetEase(Ease.OutExpo));

            if (panelShop != null)
                _showSeq.Insert(shopDelay, panelShop.DOAnchorPos(_shopStartPos, slideDuration).SetEase(Ease.OutExpo));

            bool done = false;
            _showSeq.OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                done = true;
            });

            while (!done) yield return null;
        }

        public override IEnumerator Hide()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            KillTweens();
            _hideSeq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            if (panelShop != null)
                _hideSeq.Insert(0f, panelShop.DOAnchorPos(_shopStartPos + Vector2.left * OffscreenLeft(panelShop), slideDuration * 0.8f).SetEase(Ease.InExpo));

            if (panelMenu != null)
                _hideSeq.Insert(shopDelay, panelMenu.DOAnchorPos(_menuStartPos + Vector2.left * OffscreenLeft(panelMenu), slideDuration * 0.8f).SetEase(Ease.InExpo));

            _hideSeq.OnComplete(() =>
            {
                canvasGroup.alpha = 0f;
                canvasGroup.gameObject.SetActive(false);

                if (panelMenu != null) panelMenu.anchoredPosition = _menuStartPos;
                if (panelShop != null) panelShop.anchoredPosition = _shopStartPos;
            });

            yield return base.Hide();
        }

        private void CachePositions()
        {
            if (_cached) return;
            _cached = true;
            if (panelMenu != null) _menuStartPos = panelMenu.anchoredPosition;
            if (panelShop != null) _shopStartPos = panelShop.anchoredPosition;
        }

        private void KillTweens()
        {
            _showSeq?.Kill();
            _hideSeq?.Kill();
        }
    }
}