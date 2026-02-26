using System;
using UnityEngine;

namespace DoublesideZ
{
    [CreateAssetMenu(fileName = "Currency Manager", menuName = DZ_SafetyKey.KEY_GAME_NAME + "/Currency Manager")]
    public class DZ_CurrencyManager : ScriptableObject
    {
        [Serializable]
        public struct CurrencyRuntimeData
        {
            public int coins;
        }

        [Header("Config")]
        [SerializeField] private int defaultCoins = 0;

        private CurrencyRuntimeData _data;

        public void Init()
        {
            _data = new CurrencyRuntimeData
            {
                coins = defaultCoins
            };

            LoadData();
            SaveData();
        }

        #region Coins
        public int GetCoins() => _data.coins;
        public bool HasEnoughCoins(int amount) => _data.coins >= amount;

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            _data.coins += amount;
            SaveData();
            OnCoinsChanged?.Invoke(_data.coins);
        }

        public bool SpendCoins(int amount)
        {
            if (!HasEnoughCoins(amount)) return false;

            _data.coins -= amount;
            SaveData();
            OnCoinsChanged?.Invoke(_data.coins);
            return true;
        }
        #endregion

        #region Events
        public event Action<int> OnCoinsChanged;
        #endregion

        #region Save/Load
        private void SaveData()
        {
            string json = JsonUtility.ToJson(_data);
            PlayerPrefs.SetString(DZ_SafetyKey.COIN_SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private void LoadData()
        {
            string json = PlayerPrefs.GetString(DZ_SafetyKey.COIN_SAVE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                _data = JsonUtility.FromJson<CurrencyRuntimeData>(json);
            }
        }

        public void ResetAll()
        {
            _data = new CurrencyRuntimeData();
            SaveData();
        }
        #endregion
    }
}
