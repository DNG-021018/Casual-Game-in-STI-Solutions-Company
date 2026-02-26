using System.Collections.Generic;
using UnityEngine;

namespace DoublesideZ
{
    [CreateAssetMenu(fileName = "Weapon Manager", menuName = DZ_SafetyKey.KEY_GAME_NAME + "/Weapon Manager")]
    public class DZ_WeaponManager : ScriptableObject
    {
        [Header("Weapon Config")]
        [SerializeField] private List<WeaponConfig> weapons;

        private Dictionary<WeaponType, WeaponRuntimeData> _runtimeData;
        private Dictionary<WeaponType, WeaponConfig> _configMap;

        private DZ_CurrencyManager _CurrencyManager;

        public event System.Action<WeaponType> OnWeaponEquipped;
        public event System.Action<WeaponType> OnWeaponPreviewed;


        public void Init(DZ_CurrencyManager currencyManager)
        {
            _CurrencyManager = currencyManager;
            _runtimeData = new Dictionary<WeaponType, WeaponRuntimeData>();
            _configMap = new Dictionary<WeaponType, WeaponConfig>();

            foreach (var weapon in weapons)
            {
                _configMap[weapon.weaponID] = weapon;

                _runtimeData[weapon.weaponID] = new WeaponRuntimeData
                {
                    isUnlocked = weapon.isDefault,
                    isEquipped = weapon.isDefault
                };
            }

            Load();
        }

        public List<WeaponConfig> GetAllWeapons() => weapons;

        public WeaponConfig GetConfig(WeaponType id) => _configMap[id];

        public bool IsUnlocked(WeaponType id) => _runtimeData[id].isUnlocked;

        public bool IsEquipped(WeaponType id) => _runtimeData[id].isEquipped;

        public bool Unlock(WeaponType id)
        {
            if (_CurrencyManager.SpendCoins(_configMap[id].weaponCost))
            {
                _runtimeData[id].isUnlocked = true;
                Equip(id);
                Save();
                return true;
            }
            return false;
        }

        public void Equip(WeaponType id)
        {
            foreach (var data in _runtimeData.Values)
                data.isEquipped = false;

            _runtimeData[id].isEquipped = true;
            Save();
            OnWeaponEquipped?.Invoke(id);
        }

        public WeaponType GetEquipped()
        {
            foreach (var kv in _runtimeData)
                if (kv.Value.isEquipped)
                    return kv.Key;

            return default;
        }

        public void PreviewWeapon(WeaponType id)
        {
            OnWeaponPreviewed?.Invoke(id);
        }

        public void Save()
        {
            WeaponSaveData saveData = new WeaponSaveData();

            foreach (var kv in _runtimeData)
            {
                if (kv.Value.isUnlocked)
                    saveData.unlockedWeapons.Add(kv.Key);

                if (kv.Value.isEquipped)
                    saveData.equippedWeapon = kv.Key;
            }

            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(DZ_SafetyKey.WEAPON_SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey(DZ_SafetyKey.WEAPON_SAVE_KEY))
                return;

            string json = PlayerPrefs.GetString(DZ_SafetyKey.WEAPON_SAVE_KEY);
            WeaponSaveData saveData = JsonUtility.FromJson<WeaponSaveData>(json);

            foreach (var kv in _runtimeData)
            {
                kv.Value.isUnlocked = false;
                kv.Value.isEquipped = false;
            }

            foreach (var id in saveData.unlockedWeapons)
                _runtimeData[id].isUnlocked = true;

            if (_runtimeData.ContainsKey(saveData.equippedWeapon))
                _runtimeData[saveData.equippedWeapon].isEquipped = true;
        }
    }

    [System.Serializable]
    public struct WeaponConfig
    {
        public WeaponType weaponID;
        public Sprite weaponIcon;
        public string weaponName;
        public int weaponCost;
        // public GameObject weaponPrefab;
        public bool isDefault;
    }

    public class WeaponRuntimeData
    {
        public bool isUnlocked;
        public bool isEquipped;
    }

    [System.Serializable]
    public class WeaponSaveData
    {
        public List<WeaponType> unlockedWeapons = new();
        public WeaponType equippedWeapon;
    }
}
