using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bowmancer
{
    public class B_PermanentUpgradeManager : Singleton<B_PermanentUpgradeManager>
    {
        [Header("Upgrade Costs")]
        [SerializeField] private int baseCost = 100;
        [SerializeField] private float costMultiplier = 1.5f;

        private B_CurrencyManager _currencyManager;
        private B_PlayerRef _playerRef;

        private Dictionary<PermanentUpgradeType, int> _upgradeLevels = new();

        private float _baseAttackDamage;
        private float _baseMoveSpeed;
        private float _baseMaxHealth;

        [Header("Upgrade Values Per Level")]
        [SerializeField] private float attackDamagePerLevel = 2f;
        [SerializeField] private float moveSpeedPerLevel = 0.5f;
        [SerializeField] private float maxHealthPerLevel = 10f;

        [Header("Max Levels")]
        [SerializeField] private int maxUpgradeLevel = 50;

        private const string SAVE_KEY = B_SafetyKey.PERMANENT_UPGRADE_SAVE_KEY;

        public event Action<PermanentUpgradeType, int> OnUpgradePurchased;
        public event Action<int> OnTotalLevelChanged;

        protected override void Awake()
        {
            base.Awake();
            _currencyManager = B_CurrencyManager.Instance;
        }

        public void Init(B_PlayerController playerController)
        {
            _playerRef = playerController.GetComponent<B_PlayerRef>();
            CacheBaseStats();
            LoadUpgrades();
            ApplyAllUpgrades();
        }

        #region Save/Load

        private void SaveUpgrades()
        {
            PermanentUpgradeData data = new PermanentUpgradeData
            {
                attackDamageLevel = GetUpgradeLevel(PermanentUpgradeType.AttackDamage),
                moveSpeedLevel = GetUpgradeLevel(PermanentUpgradeType.MoveSpeed),
                maxHealthLevel = GetUpgradeLevel(PermanentUpgradeType.MaxHealth)
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public void LoadUpgrades()
        {
            string json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                PermanentUpgradeData data = JsonUtility.FromJson<PermanentUpgradeData>(json);
                _upgradeLevels[PermanentUpgradeType.AttackDamage] = data.attackDamageLevel;
                _upgradeLevels[PermanentUpgradeType.MoveSpeed] = data.moveSpeedLevel;
                _upgradeLevels[PermanentUpgradeType.MaxHealth] = data.maxHealthLevel;
            }
            else
            {
                _upgradeLevels[PermanentUpgradeType.AttackDamage] = 0;
                _upgradeLevels[PermanentUpgradeType.MoveSpeed] = 0;
                _upgradeLevels[PermanentUpgradeType.MaxHealth] = 0;
            }
        }

        #endregion

        #region Cache Base Stats

        private void CacheBaseStats()
        {
            var player = _playerRef.PlayerController;
            if (player != null)
            {
                _baseAttackDamage = player.AttackPower;
                _baseMoveSpeed = player.MoveSpeed;
                _baseMaxHealth = player.GetBaseHealth();
            }
        }

        #endregion

        #region Apply Upgrades

        public void ApplyAllUpgrades()
        {
            if (_playerRef == null || _playerRef.PlayerController == null)
            {
                return;
            }

            var player = _playerRef.PlayerController;

            int attackLevel = GetUpgradeLevel(PermanentUpgradeType.AttackDamage);
            float totalAttackBonus = attackLevel * attackDamagePerLevel;
            player.SetAttackPower(_baseAttackDamage + totalAttackBonus);

            int speedLevel = GetUpgradeLevel(PermanentUpgradeType.MoveSpeed);
            float totalSpeedBonus = speedLevel * moveSpeedPerLevel;
            player.SetMoveSpeed(_baseMoveSpeed + totalSpeedBonus);

            int healthLevel = GetUpgradeLevel(PermanentUpgradeType.MaxHealth);
            float totalHealthBonus = healthLevel * maxHealthPerLevel;
            player.SetMaxHealth(_baseMaxHealth + totalHealthBonus);
            player.Heal(totalHealthBonus);
        }

        #endregion

        #region Purchase Upgrades

        public bool PurchaseUpgrade(PermanentUpgradeType type)
        {
            int currentLevel = GetUpgradeLevel(type);

            if (currentLevel >= maxUpgradeLevel)
            {
                return false;
            }

            int cost = CalculateCost();

            if (!_currencyManager.HasEnoughCoins(cost))
            {
                MB_PopupManager.Instance.ShowTopNotification($"Not enough coins to purchase upgrade!", Color.red);
                return false;
            }

            if (!_currencyManager.SpendCoins(cost))
            {
                return false;
            }

            _upgradeLevels[type] = currentLevel + 1;
            SaveUpgrades();

            ApplySingleUpgrade(type);

            OnUpgradePurchased?.Invoke(type, _upgradeLevels[type]);
            OnTotalLevelChanged?.Invoke(GetTotalLevel());


            return true;
        }

        private void ApplySingleUpgrade(PermanentUpgradeType type)
        {
            if (_playerRef == null || _playerRef.PlayerController == null)
            {
                return;
            }

            var player = _playerRef.PlayerController;
            int level = GetUpgradeLevel(type);

            switch (type)
            {
                case PermanentUpgradeType.AttackDamage:
                    float attackBonus = level * attackDamagePerLevel;
                    player.SetAttackPower(_baseAttackDamage + attackBonus);
                    break;

                case PermanentUpgradeType.MoveSpeed:
                    float speedBonus = level * moveSpeedPerLevel;
                    player.SetMoveSpeed(_baseMoveSpeed + speedBonus);
                    break;

                case PermanentUpgradeType.MaxHealth:
                    float healthBonus = level * maxHealthPerLevel;
                    player.SetMaxHealth(_baseMaxHealth + healthBonus);
                    player.Heal(healthBonus);
                    break;
            }
        }

        #endregion

        #region Cost Calculation

        public int CalculateCost()
        {
            int totalLevel = GetTotalLevel();

            int cost = Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, totalLevel) + 0.2f);

            return Mathf.RoundToInt(cost);
        }

        public int GetTotalLevel()
        {
            int total = 0;
            foreach (var kvp in _upgradeLevels)
            {
                total += kvp.Value;
            }
            return total;
        }

        #endregion

        #region Query Methods

        public int GetUpgradeLevel(PermanentUpgradeType type)
        {
            if (_upgradeLevels.TryGetValue(type, out int level))
            {
                return level;
            }
            return 0;
        }

        public float GetUpgradeValue(PermanentUpgradeType type)
        {
            int level = GetUpgradeLevel(type);

            return type switch
            {
                PermanentUpgradeType.AttackDamage => level * attackDamagePerLevel,
                PermanentUpgradeType.MoveSpeed => level * moveSpeedPerLevel,
                PermanentUpgradeType.MaxHealth => level * maxHealthPerLevel,
                _ => 0f
            };
        }

        public bool CanPurchase()
        {
            int cost = CalculateCost();
            return _currencyManager.HasEnoughCoins(cost) && GetTotalLevel() < (maxUpgradeLevel * 3);
        }

        #endregion

        #region Debug

        public void ResetAllUpgrades()
        {
            _upgradeLevels.Clear();
            _upgradeLevels[PermanentUpgradeType.AttackDamage] = 0;
            _upgradeLevels[PermanentUpgradeType.MoveSpeed] = 0;
            _upgradeLevels[PermanentUpgradeType.MaxHealth] = 0;
            SaveUpgrades();

        }
        #endregion
    }


    public enum PermanentUpgradeType
    {
        AttackDamage,
        MoveSpeed,
        MaxHealth
    }

    [Serializable]
    public class PermanentUpgradeData
    {
        public int attackDamageLevel;
        public int moveSpeedLevel;
        public int maxHealthLevel;
    }
}