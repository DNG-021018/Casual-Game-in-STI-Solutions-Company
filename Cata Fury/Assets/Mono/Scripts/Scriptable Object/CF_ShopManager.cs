using System;
using System.Collections.Generic;
using UnityEngine;

namespace CataFury
{
    [CreateAssetMenu(fileName = "Shop Manager", menuName = CF_SafetyKey.KEY_GAME_NAME + "/Shop Manager")]
    public class CF_ShopManager : ScriptableObject
    {
        [Header("Shop Config")]
        [SerializeField] private List<ShopConfig> ListItems;

        private Dictionary<ShopItemType, ShopRuntimeData> _runtimeData;
        private Dictionary<ShopItemType, ShopConfig> _configMap;

        private CF_CurrencyManager _CurrencyManager;

        public event Action<ShopItemType> OnItemEquipped;
        public event Action<ShopItemType> OnItemPreviewed;

        public void Init(CF_CurrencyManager currencyManager)
        {
            _CurrencyManager = currencyManager;
            _runtimeData = new Dictionary<ShopItemType, ShopRuntimeData>();
            _configMap = new Dictionary<ShopItemType, ShopConfig>();

            foreach (var item in ListItems)
            {
                _configMap[item.id] = item;

                _runtimeData[item.id] = new ShopRuntimeData
                {
                    isUnlocked = item.isDefault,
                    isEquipped = item.isDefault
                };
            }

            Load();
        }

        public List<ShopConfig> GetAllItems() => ListItems;

        public ShopConfig GetConfig(ShopItemType id) => _configMap[id];

        public bool IsUnlocked(ShopItemType id) => _runtimeData[id].isUnlocked;

        public bool IsEquipped(ShopItemType id) => _runtimeData[id].isEquipped;

        public bool Unlock(ShopItemType id)
        {
            if (_CurrencyManager.SpendCoins(_configMap[id].itemCost))
            {
                _runtimeData[id].isUnlocked = true;
                Equip(id);
                Save();
                return true;
            }
            return false;
        }

        public void Equip(ShopItemType id)
        {
            foreach (var data in _runtimeData.Values)
                data.isEquipped = false;

            _runtimeData[id].isEquipped = true;
            Save();
            OnItemEquipped?.Invoke(id);
        }

        public ShopItemType GetEquipped()
        {
            foreach (var kv in _runtimeData)
                if (kv.Value.isEquipped)
                    return kv.Key;

            return default;
        }

        public void PreviewItem(ShopItemType id)
        {
            OnItemPreviewed?.Invoke(id);
        }

        public void Save()
        {
            ShopSaveData saveData = new ShopSaveData();

            foreach (var kv in _runtimeData)
            {
                if (kv.Value.isUnlocked)
                    saveData.unlockedItems.Add(kv.Key);

                if (kv.Value.isEquipped)
                    saveData.equippedItem = kv.Key;
            }

            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(CF_SafetyKey.Data.SHOP_SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey(CF_SafetyKey.Data.SHOP_SAVE_KEY))
                return;

            string json = PlayerPrefs.GetString(CF_SafetyKey.Data.SHOP_SAVE_KEY);
            ShopSaveData saveData = JsonUtility.FromJson<ShopSaveData>(json);

            foreach (var kv in _runtimeData)
            {
                kv.Value.isUnlocked = false;
                kv.Value.isEquipped = false;
            }

            foreach (var id in saveData.unlockedItems)
                _runtimeData[id].isUnlocked = true;

            if (_runtimeData.ContainsKey(saveData.equippedItem))
                _runtimeData[saveData.equippedItem].isEquipped = true;
        }
    }

    [Serializable]
    public struct ShopConfig
    {
        public ShopItemType id;
        public Sprite itemIcon;
        public string itemName;
        public int itemCost;
        // public GameObject itemPrefab;
        public bool isDefault;
    }

    public class ShopRuntimeData
    {
        public bool isUnlocked;
        public bool isEquipped;
    }

    [Serializable]
    public class ShopSaveData
    {
        public List<ShopItemType> unlockedItems = new();
        public ShopItemType equippedItem;
    }
}
