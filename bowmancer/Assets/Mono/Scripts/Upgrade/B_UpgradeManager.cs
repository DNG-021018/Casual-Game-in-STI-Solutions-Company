using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bowmancer
{
    public class B_UpgradeManager : Singleton<B_UpgradeManager>
    {
        [Header("Available Upgrades Pool")]
        [SerializeField] private List<B_BaseUpgrade> allAvailableUpgrades = new List<B_BaseUpgrade>();

        [Header("References")]
        [SerializeField] private B_PlayerRef _playerRef;

        private Dictionary<B_BaseUpgrade, int> _activeUpgrades = new Dictionary<B_BaseUpgrade, int>();

        public event Action<B_BaseUpgrade, int> OnUpgradeApplied;
        public event Action<B_BaseUpgrade> OnUpgradeRemoved;

        #region Initialization

        public void Initialize(B_PlayerRef playerRef)
        {
            _playerRef = playerRef;
        }

        #endregion

        #region Upgrade Management

        public bool ApplyUpgrade(B_BaseUpgrade upgrade)
        {
            if (upgrade == null)
            {
                return false;
            }

            if (_playerRef == null)
            {
                return false;
            }

            if (_activeUpgrades.TryGetValue(upgrade, out int currentLevel))
            {
                if (!upgrade.CanStack)
                {
                    return false;
                }

                if (upgrade.MaxLevel > 0 && currentLevel >= upgrade.MaxLevel)
                {
                    return false;
                }

                currentLevel++;
                _activeUpgrades[upgrade] = currentLevel;
            }
            else
            {
                currentLevel = 1;
                _activeUpgrades.Add(upgrade, currentLevel);
            }

            upgrade.Apply(_playerRef, currentLevel);

            OnUpgradeApplied?.Invoke(upgrade, currentLevel);

            return true;
        }

        public void RemoveUpgrade(B_BaseUpgrade upgrade)
        {
            if (!_activeUpgrades.ContainsKey(upgrade)) return;

            upgrade.Remove(_playerRef);
            _activeUpgrades.Remove(upgrade);

            OnUpgradeRemoved?.Invoke(upgrade);
        }

        public void ClearAllUpgrades()
        {
            foreach (var upgrade in _activeUpgrades.Keys.ToList())
            {
                upgrade.Remove(_playerRef);
            }
            _activeUpgrades.Clear();
        }

        #endregion

        #region Query Methods

        public bool HasUpgrade(B_BaseUpgrade upgrade)
        {
            return _activeUpgrades.ContainsKey(upgrade);
        }

        public int GetUpgradeLevel(B_BaseUpgrade upgrade)
        {
            return _activeUpgrades.TryGetValue(upgrade, out int level) ? level : 0;
        }

        public Dictionary<B_BaseUpgrade, int> GetActiveUpgrades()
        {
            return new Dictionary<B_BaseUpgrade, int>(_activeUpgrades);
        }

        public List<B_BaseUpgrade> GetUpgradesByCategory(UpgradeCategory category)
        {
            return allAvailableUpgrades.Where(u => u.Category == category).ToList();
        }

        public List<B_BaseUpgrade> GetRandomUpgradeOptions(int count = 2)
        {
            List<B_BaseUpgrade> available = allAvailableUpgrades.Where(u => CanTakeUpgrade(u)).ToList();

            if (available.Count == 0)
            {
                return new List<B_BaseUpgrade>();
            }

            List<B_BaseUpgrade> result = new();
            int takeCount = Mathf.Min(count, available.Count);

            for (int i = 0; i < takeCount; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, available.Count);
                result.Add(available[randomIndex]);
                available.RemoveAt(randomIndex);
            }

            return result;
        }

        private bool CanTakeUpgrade(B_BaseUpgrade upgrade)
        {
            if (!_activeUpgrades.ContainsKey(upgrade))
                return true;

            if (!upgrade.CanStack)
                return false;

            int currentLevel = _activeUpgrades[upgrade];
            if (upgrade.MaxLevel > 0 && currentLevel >= upgrade.MaxLevel)
                return false;

            return true;
        }
        #endregion
    }
}