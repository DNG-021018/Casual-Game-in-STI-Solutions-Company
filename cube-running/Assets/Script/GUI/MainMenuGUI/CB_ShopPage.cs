using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace CB_CubeRunner
{
    public class CB_ShopPage : CB_UIPage
    {
        [Header("Panel")]
        [SerializeField] private Panels TutorialPanel;

        [Header("Shop Items Prefabs")]
        [SerializeField] private CB_ShopContentItems itemPrefab;
        [SerializeField] private Transform itemsContainer;

        [Header("Buttons")]
        [SerializeField] private CB_UIButton nextBtn;
        [SerializeField] private CB_UIButton prevBtn;
        [SerializeField] private CB_UIButton exitBtn;
        [SerializeField] private CB_UIButton buyButton;

        [Header("Buy Button Visuals")]
        [SerializeField] private Image buyButtonBackground;
        [SerializeField] private Sprite availableToBuySprite;
        [SerializeField] private Sprite notAvailableToBuySprite;
        [SerializeField] private Sprite selectedSprite;

        [Header("Pager")]
        [SerializeField] private CB_UIHorizontalPager pager;

        [Header("Skin Config")]
        [SerializeField] private CR_PlayerSkinConfig skinConfig;

        private CB_BaseUI parent;
        private Vector2 _menuStart;
        private readonly List<CB_ShopContentItems> itemInstances = new();

        private CB_ShopManager shopManager;
        private int currentSelectedIndex = 0;

        public override void Init(CB_BaseUI p)
        {
            base.Init(p);
            parent = p;

            shopManager = CB_ShopManager.Instance;
            if (shopManager != null)
                skinConfig = shopManager.SkinConfig;

            if (!pager && TryGetComponent(out CB_UIHorizontalPager pagerFound))
                pager = pagerFound;

            nextBtn.Bind(() =>
            {
                if (pager == null) return;
                pager.Next();
                SyncSelectedIndexFromPager();
                RefreshAllItems();
            });

            prevBtn.Bind(() =>
            {
                if (pager == null) return;
                pager.Prev();
                SyncSelectedIndexFromPager();
                RefreshAllItems();
            });

            exitBtn.Bind(() => parent.Back());
            buyButton.Bind(OnBuyButtonClicked);

            GenerateShopItems();

            if (CB_GameManager.Instance != null)
            {
                CB_GameManager.Instance.OnCoinChanged += OnCoinChanged;
                CB_GameManager.Instance.OnSkinChanged += OnSkinChanged;
            }
        }

        void OnDestroy()
        {
            nextBtn.UnBind();
            prevBtn.UnBind();
            exitBtn.UnBind();
            buyButton.UnBind();

            if (CB_GameManager.Instance != null)
            {
                CB_GameManager.Instance.OnCoinChanged -= OnCoinChanged;
                CB_GameManager.Instance.OnSkinChanged -= OnSkinChanged;
            }
        }

        int GetIndexBySkinId(int skinId)
        {
            if (skinConfig == null || skinConfig.skinConfig == null || skinConfig.skinConfig.Length == 0)
                return 0;

            for (int i = 0; i < skinConfig.skinConfig.Length; i++)
            {
                if (skinConfig.skinConfig[i].ID == skinId)
                    return i;
            }

            return 0;
        }

        void SyncPagerToSelectedSkin()
        {
            if (skinConfig == null || skinConfig.skinConfig == null || skinConfig.skinConfig.Length == 0)
                return;

            int skinId = 0;

            if (CB_GameManager.Instance != null)
                skinId = CB_GameManager.Instance.CurrentSkinId;

            currentSelectedIndex = GetIndexBySkinId(skinId);

            if (pager != null)
            {
                pager.JumpTo(currentSelectedIndex, true);
                SyncSelectedIndexFromPager();
            }

            RefreshAllItems();
        }

        void GenerateShopItems()
        {
            if (skinConfig == null || skinConfig.skinConfig == null) return;
            if (itemsContainer == null) return;

            foreach (var item in itemInstances)
            {
                if (item != null) Destroy(item.gameObject);
            }
            itemInstances.Clear();

            for (int i = 0; i < skinConfig.skinConfig.Length; i++)
            {
                var skin = skinConfig.skinConfig[i];
                var itemGO = Instantiate(itemPrefab, itemsContainer);
                var item = itemGO.GetComponent<CB_ShopContentItems>();

                if (item != null)
                {
                    itemInstances.Add(item);
                    int index = i;
                    item.BindClick(() => OnItemSelected(index));
                }
            }

            RefreshAllItems();
        }

        void RefreshAllItems()
        {
            if (skinConfig == null || skinConfig.skinConfig == null) return;

            string priceText = CB_ShopManager.SKIN_PRICE.ToString();

            for (int i = 0; i < skinConfig.skinConfig.Length && i < itemInstances.Count; i++)
            {
                var skin = skinConfig.skinConfig[i];
                var item = itemInstances[i];

                item.SetContent(
                    skin.displayName,
                    priceText,
                    skin.icon
                );
            }

            UpdateBuyButton();
        }

        void OnItemSelected(int index)
        {
            currentSelectedIndex = index;

            if (pager != null)
            {
                pager.JumpTo(index, true);
                SyncSelectedIndexFromPager();
            }

            RefreshAllItems();
        }

        void SyncSelectedIndexFromPager()
        {
            if (pager == null || skinConfig == null || skinConfig.skinConfig == null) return;
            int count = skinConfig.skinConfig.Length;
            currentSelectedIndex = Mathf.Clamp(pager.Index, 0, count - 1);
        }


        void SetBuyButtonState(string text, Sprite sprite, bool interactable)
        {
            var btn = buyButton.GetComponent<Button>();
            if (btn != null)
                btn.interactable = interactable;

            if (buyButtonBackground != null && sprite != null)
                buyButtonBackground.sprite = sprite;

        }

        void UpdateBuyButton()
        {
            if (skinConfig == null || skinConfig.skinConfig == null) return;
            if (currentSelectedIndex < 0 || currentSelectedIndex >= skinConfig.skinConfig.Length) return;

            var skin = skinConfig.skinConfig[currentSelectedIndex];

            int totalCoin = CB_GameManager.Instance.TotalCoin;
            int currentSkinId = CB_GameManager.Instance.CurrentSkinId;

            bool isUnlocked = shopManager.IsSkinUnlocked(skin.ID);
            bool isSelected = skin.ID == currentSkinId;

            if (isUnlocked)
            {
                if (isSelected)
                {
                    SetBuyButtonState("SELECTED", selectedSprite, false);
                }
                else
                {
                    SetBuyButtonState("SELECT", selectedSprite, true);
                }

                return;
            }

            bool canAfford = totalCoin >= CB_ShopManager.SKIN_PRICE;

            if (canAfford)
            {
                SetBuyButtonState("BUY", availableToBuySprite, true);
            }
            else
            {
                SetBuyButtonState("BUY", notAvailableToBuySprite, false);
            }
        }

        void OnBuyButtonClicked()
        {
            if (skinConfig == null || skinConfig.skinConfig == null ||
                currentSelectedIndex < 0 || currentSelectedIndex >= skinConfig.skinConfig.Length ||
                shopManager == null)
                return;

            var skin = skinConfig.skinConfig[currentSelectedIndex];
            int skinId = skin.ID;

            int currSkinId = CB_GameManager.Instance != null ? CB_GameManager.Instance.CurrentSkinId : -1;

            bool isUnlocked = shopManager.IsSkinUnlocked(skinId);
            bool isSelected = skinId == currSkinId;

            if (isUnlocked)
            {
                if (isSelected)
                {
                    return;
                }
                shopManager.SelectAndApplySkin(skinId);
                StartCoroutine(RefreshAfterSelect());
            }
            else
            {
                if (!shopManager.TryBuySkin(skinId))
                {
                    RefreshAllItems();
                    return;
                }

                shopManager.SelectAndApplySkin(skinId);
                RefreshAllItems();
            }
        }

        private IEnumerator RefreshAfterSelect()
        {
            yield return null;
            RefreshAllItems();
            UpdateBuyButton();
        }

        void OnCoinChanged(int newAmount)
        {
            RefreshAllItems();
        }

        void OnSkinChanged(int skinId)
        {
            currentSelectedIndex = GetIndexBySkinId(skinId);

            if (pager != null)
            {
                pager.JumpTo(currentSelectedIndex, true);
                SyncSelectedIndexFromPager();
            }

            RefreshAllItems();
        }


        public override IEnumerator Show(object ctx = null)
        {
            yield return null;

            RefreshAllItems();
            SyncPagerToSelectedSkin();

            Vector2 from = GetOffscreenPos(TutorialPanel.panel, TutorialPanel.slideDir, _menuStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (TutorialPanel.panel, from, _menuStart)
            );
        }

        public override IEnumerator Hide()
        {
            Vector2 to = GetOffscreenPos(TutorialPanel.panel, TutorialPanel.slideDir, _menuStart, offscreenPadding);

            RefreshAllItems();

            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (TutorialPanel.panel, _menuStart, to)
            );
        }

        public override void ApplyContext(object ctx)
        {
            if (ctx is int i)
            {
                currentSelectedIndex = i;

                if (pager != null)
                {
                    pager.JumpTo(currentSelectedIndex, true);
                    SyncSelectedIndexFromPager();
                }

                RefreshAllItems();
            }
        }
    }
}