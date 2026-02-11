using UnityEngine;

namespace Bowmancer
{
    public class B_CurrencyManager : Singleton<B_CurrencyManager>
    {
        [System.Serializable]
        public class CurrencyData
        {
            public int coins;
        }

        private CurrencyData _data = new();
        private const string SAVE_KEY = "PlayerCurrency";

        protected override void Awake()
        {
            base.Awake();
            LoadData();
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
        public event System.Action<int> OnCoinsChanged;
        public event System.Action<int> OnGemsChanged;
        public event System.Action<int> OnEnergyChanged;
        #endregion

        #region Save/Load
        private void SaveData()
        {
            string json = JsonUtility.ToJson(_data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private void LoadData()
        {
            string json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                _data = JsonUtility.FromJson<CurrencyData>(json);
            }
        }

        public void ResetAll()
        {
            _data = new CurrencyData();
            SaveData();
        }
        #endregion
    }
}
