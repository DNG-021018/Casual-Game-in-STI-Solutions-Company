using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

namespace Bowmancer
{
    public class B_ShopUpgradeMenu : B_UIPage
    {
        [Header("Refs")]
        [SerializeField] private List<B_ShopUpgradeItem> upgradeItems;
        [SerializeField] private B_UIButton closeButton;
        [SerializeField] private B_UIButton buyUpgradeButton;
        [SerializeField] private TextMeshProUGUI buyUpgradeCostText;

        [Header("Upgrade Icons")]
        [SerializeField] private Sprite attackDamageIcon;
        [SerializeField] private Sprite moveSpeedIcon;
        [SerializeField] private Sprite maxHealthIcon;

        [Header("Tween Settings")]
        [SerializeField] private float showDuration = 0.3f;
        [SerializeField] private float hideDuration = 0.25f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;
        [SerializeField] private float itemStaggerDelay = 0.08f;
        [SerializeField] private float buttonScalePunch = 0.15f;

        private B_ShopUpgradeItem _currentSelectedItem;
        private int _currentSelectedIndex = -1;
        private B_PermanentUpgradeManager _permanentUpgradeManager;
        private B_BaseUI _parent;

        private Sequence _showSequence;
        private Sequence _hideSequence;

        void Awake()
        {
            _permanentUpgradeManager = B_PermanentUpgradeManager.Instance;
        }

        public override void Init(B_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
        }

        public override IEnumerator Show()
        {
            _showSequence?.Kill();
            _hideSequence?.Kill();

            foreach (var item in upgradeItems)
            {
                if (item != null)
                {
                    item.transform.DOKill();
                }
            }
            if (closeButton != null) closeButton.transform.DOKill();
            if (buyUpgradeButton != null) buyUpgradeButton.transform.DOKill();

            _showSequence = DOTween.Sequence();

            canvasGroup.alpha = 0f;

            _showSequence.Append(canvasGroup.DOFade(1f, showDuration).SetEase(showEase));

            for (int i = 0; i < upgradeItems.Count; i++)
            {
                if (upgradeItems[i] != null)
                {
                    var itemTransform = upgradeItems[i].transform;

                    itemTransform.localScale = Vector3.zero;

                    _showSequence.Insert(showDuration * 0.3f + (i * itemStaggerDelay),
                        itemTransform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack));
                }
            }

            if (closeButton != null)
            {
                var closeTransform = closeButton.transform;
                closeTransform.localScale = Vector3.zero;
                _showSequence.Insert(showDuration * 0.5f,
                    closeTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
            }

            if (buyUpgradeButton != null)
            {
                var buyTransform = buyUpgradeButton.transform;
                buyTransform.localScale = Vector3.zero;
                _showSequence.Insert(showDuration * 0.5f,
                    buyTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
            }

            yield return base.Show();
            yield return _showSequence.WaitForCompletion();
        }

        public override IEnumerator Hide()
        {
            _showSequence?.Kill();
            _hideSequence?.Kill();

            _hideSequence = DOTween.Sequence();

            for (int i = upgradeItems.Count - 1; i >= 0; i--)
            {
                if (upgradeItems[i] != null)
                {
                    var itemTransform = upgradeItems[i].transform;
                    int reverseIndex = upgradeItems.Count - 1 - i;

                    _hideSequence.Insert(reverseIndex * itemStaggerDelay,
                        itemTransform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack));
                }
            }

            _hideSequence.Insert(upgradeItems.Count * itemStaggerDelay,
                canvasGroup.DOFade(0f, hideDuration).SetEase(hideEase));

            yield return _hideSequence.WaitForCompletion();
            yield return base.Hide();
        }

        void Start()
        {
            InitializeShopItems();
            DeselectAll();
            UpdateBuyButtonState();
        }

        void OnEnable()
        {
            RefreshShopDisplay();

            closeButton.Bind(() =>
            {
                closeButton.transform.DOPunchScale(Vector3.one * buttonScalePunch, 0.2f, 5, 0.5f);
                _parent.Back();
            });

            foreach (var item in upgradeItems)
            {
                if (item != null)
                {
                    int index = upgradeItems.IndexOf(item);
                    item.bindButton(() => OnItemClicked(item, index));
                }
            }

            if (buyUpgradeButton != null)
            {
                buyUpgradeButton.Bind(OnBuyUpgradeClicked);
            }

            if (_permanentUpgradeManager != null)
            {
                _permanentUpgradeManager.OnUpgradePurchased += OnUpgradePurchasedHandler;
            }
        }

        void OnDisable()
        {
            _showSequence?.Kill();
            _hideSequence?.Kill();

            foreach (var item in upgradeItems)
            {
                if (item != null)
                {
                    item.transform.DOKill();
                }
            }

            if (buyUpgradeButton != null)
            {
                buyUpgradeButton.UnBind();
                buyUpgradeButton.transform.DOKill();
            }

            if (closeButton != null)
            {
                closeButton.UnBind();
                closeButton.transform.DOKill();
            }

            foreach (var item in upgradeItems)
            {
                if (item != null)
                {
                    item.UnBind();
                }
            }

            if (_permanentUpgradeManager != null)
            {
                _permanentUpgradeManager.OnUpgradePurchased -= OnUpgradePurchasedHandler;
            }
        }

        private void InitializeShopItems()
        {
            if (upgradeItems.Count < 3)
            {
                return;
            }

            upgradeItems[0].SetUpgradeData(
                PermanentUpgradeType.AttackDamage,
                attackDamageIcon,
                "Attack Damage",
                _permanentUpgradeManager.GetUpgradeLevel(PermanentUpgradeType.AttackDamage)
            );

            upgradeItems[1].SetUpgradeData(
                PermanentUpgradeType.MoveSpeed,
                moveSpeedIcon,
                "Move Speed",
                _permanentUpgradeManager.GetUpgradeLevel(PermanentUpgradeType.MoveSpeed)
            );

            upgradeItems[2].SetUpgradeData(
                PermanentUpgradeType.MaxHealth,
                maxHealthIcon,
                "Max Health",
                _permanentUpgradeManager.GetUpgradeLevel(PermanentUpgradeType.MaxHealth)
            );

            UpdateCostDisplay();
        }

        private void RefreshShopDisplay()
        {
            if (_permanentUpgradeManager == null) return;
            for (int i = 0; i < upgradeItems.Count && i < 3; i++)
            {
                var item = upgradeItems[i];
                if (item == null) continue;

                PermanentUpgradeType upgradeType = (PermanentUpgradeType)i;
                Sprite icon = GetIconForUpgradeType(upgradeType);
                string name = GetNameForUpgradeType(upgradeType);
                int currentLevel = _permanentUpgradeManager.GetUpgradeLevel(upgradeType);

                item.SetUpgradeData(
                    upgradeType,
                    icon,
                    name,
                    currentLevel
                );
            }

            UpdateCostDisplay();
            DeselectAll();
        }

        private void UpdateCostDisplay()
        {
            if (buyUpgradeCostText != null && _permanentUpgradeManager != null)
            {
                int cost = _permanentUpgradeManager.CalculateCost();
                buyUpgradeCostText.text = $"Cost: {cost}";

                buyUpgradeCostText.transform.DOKill();
                buyUpgradeCostText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.5f);
            }
        }

        private void OnItemClicked(B_ShopUpgradeItem clickedItem, int index)
        {
            clickedItem.transform.DOKill();
            clickedItem.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f);

            SelectItem(clickedItem, index);
        }

        private void SelectItem(B_ShopUpgradeItem item, int index)
        {
            if (_currentSelectedItem == item)
            {
                DeselectAll();
                return;
            }

            if (_currentSelectedItem != null)
            {
                _currentSelectedItem.SetSelected(false);
                _currentSelectedItem.transform.DOKill();
                _currentSelectedItem.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
            }

            _currentSelectedItem = item;
            _currentSelectedIndex = index;
            _currentSelectedItem.SetSelected(true);

            _currentSelectedItem.transform.DOKill();
            _currentSelectedItem.transform.DOScale(Vector3.one * 1.05f, 0.3f).SetEase(Ease.OutBack);

            UpdateBuyButtonState();
        }

        private void DeselectAll()
        {
            foreach (var item in upgradeItems)
            {
                if (item != null)
                {
                    item.SetSelected(false);
                    item.transform.DOKill();
                    item.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
                }
            }

            _currentSelectedItem = null;
            _currentSelectedIndex = -1;
            UpdateBuyButtonState();
        }

        private void OnBuyUpgradeClicked()
        {
            if (_currentSelectedItem == null || _currentSelectedIndex < 0 || _permanentUpgradeManager == null)
            {
                return;
            }

            buyUpgradeButton.transform.DOKill();
            buyUpgradeButton.transform.DOPunchScale(Vector3.one * buttonScalePunch, 0.2f, 5, 0.5f);

            PermanentUpgradeType upgradeType = _currentSelectedItem.GetUpgradeType();
            bool success = _permanentUpgradeManager.PurchaseUpgrade(upgradeType);

            if (success)
            {
                MB_PopupManager.Instance.ShowTopNotification($"{upgradeType} upgraded!", Color.green);

                if (_currentSelectedItem != null)
                {
                    Sequence successSequence = DOTween.Sequence();
                    successSequence.Append(_currentSelectedItem.transform.DOScale(Vector3.one * 1.2f, 0.15f).SetEase(Ease.OutQuad));
                    successSequence.Append(_currentSelectedItem.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
                }

                DeselectAll();
            }
            else
            {

                if (_currentSelectedItem != null)
                {
                    _currentSelectedItem.transform.DOShakePosition(0.3f, strength: 10f, vibrato: 20, randomness: 90f);
                }
            }
        }

        private void UpdateBuyButtonState()
        {
            if (buyUpgradeButton == null)
            {
                return;
            }
            bool hasSelection = _currentSelectedIndex >= 0 && _currentSelectedItem != null;
            buyUpgradeButton.SetInteractable(hasSelection);

            if (hasSelection)
            {
                buyUpgradeButton.transform.DOKill();
                buyUpgradeButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
            else
            {
                buyUpgradeButton.transform.DOKill();
                buyUpgradeButton.transform.DOScale(Vector3.one * 0.95f, 0.2f).SetEase(Ease.OutQuad);
            }
        }

        private void OnUpgradePurchasedHandler(PermanentUpgradeType type, int newLevel)
        {
            RefreshShopDisplay();
        }

        private Sprite GetIconForUpgradeType(PermanentUpgradeType type)
        {
            return type switch
            {
                PermanentUpgradeType.AttackDamage => attackDamageIcon,
                PermanentUpgradeType.MoveSpeed => moveSpeedIcon,
                PermanentUpgradeType.MaxHealth => maxHealthIcon,
                _ => null
            };
        }

        private string GetNameForUpgradeType(PermanentUpgradeType type)
        {
            return type switch
            {
                PermanentUpgradeType.AttackDamage => "Attack Damage",
                PermanentUpgradeType.MoveSpeed => "Move Speed",
                PermanentUpgradeType.MaxHealth => "Max Health",
                _ => "Unknown"
            };
        }

        public B_ShopUpgradeItem GetSelectedItem()
        {
            return _currentSelectedItem;
        }
    }
}