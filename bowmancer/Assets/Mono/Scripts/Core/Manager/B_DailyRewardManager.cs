using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bowmancer
{
    public class B_DailyRewardManager : Singleton<B_DailyRewardManager>
    {
        [SerializeField] private B_DailyRewardItems dailyRewardItems;
        private DailyRewardData saveData;
        private B_CurrencyManager _currencyManager;

        protected override void Awake()
        {
            base.Awake();
            _currencyManager = B_CurrencyManager.Instance;
        }

        void Start()
        {
            Init();
        }

        void Init()
        {
            LoadDailyRewards();
            CheckNewLoginDay();
        }

        void LoadDailyRewards()
        {
            string json = PlayerPrefs.GetString(B_SafetyKey.DAILY_REWARD_SAVE_KEY, "");

            if (string.IsNullOrEmpty(json))
            {
                saveData = new DailyRewardData();
                SaveData();
            }
            else
            {
                saveData = JsonUtility.FromJson<DailyRewardData>(json);
            }

            UpdateRewardItemsStatus();
        }

        void CheckNewLoginDay()
        {
            DateTime now = DateTime.Now;
            DateTime lastLogin = DateTime.FromBinary(saveData.lastLoginTimestamp);

            bool isNewDay = now.Date > lastLogin.Date;

            if (saveData.currentDay == 0)
            {
                saveData.currentDay = 1;
                saveData.loginDayCount = 1;
                saveData.lastLoginTimestamp = now.ToBinary();
                saveData.canClaimToday = true;
                SaveData();
                UpdateRewardItemsStatus();
            }
            else if (isNewDay)
            {
                int daysPassed = (now.Date - lastLogin.Date).Days;

                if (daysPassed == 1)
                {
                    saveData.loginDayCount++;
                    saveData.currentDay = ((saveData.currentDay) % 7) + 1;

                    if (saveData.currentDay == 1 && saveData.loginDayCount > 7)
                    {
                        ResetRewards();
                    }
                }
                else if (daysPassed > 1)
                {
                    ResetRewards();
                }

                saveData.lastLoginTimestamp = now.ToBinary();
                saveData.canClaimToday = true;
                SaveData();
                UpdateRewardItemsStatus();
            }
            else
            {
                saveData.canClaimToday = !saveData.claimedDays[saveData.currentDay - 1];
            }
        }

        void UpdateRewardItemsStatus()
        {
            if (dailyRewardItems == null || dailyRewardItems.dailyReward == null) return;

            for (int i = 0; i < dailyRewardItems.dailyReward.Count && i < 7; i++)
            {
                var reward = dailyRewardItems.dailyReward[i];
                reward.claimed = saveData.claimedDays[i];
                dailyRewardItems.dailyReward[i] = reward;
            }
        }

        public bool CanClaimToday()
        {
            if (saveData.currentDay == 0) return false;

            DateTime now = DateTime.Now;
            DateTime lastLogin = DateTime.FromBinary(saveData.lastLoginTimestamp);
            bool isNewDay = now.Date > lastLogin.Date;

            if (isNewDay)
            {
                CheckNewLoginDay();
            }

            return saveData.canClaimToday;
        }

        public bool HasRewardToday()
        {
            return saveData.canClaimToday;
        }

        public bool ClaimTodayReward()
        {
            if (!CanClaimToday()) return false;

            int dayIndex = saveData.currentDay - 1;
            saveData.claimedDays[dayIndex] = true;
            saveData.canClaimToday = false;

            if (dayIndex < dailyRewardItems.dailyReward.Count)
            {
                var reward = dailyRewardItems.dailyReward[dayIndex];
                reward.claimed = true;
                dailyRewardItems.dailyReward[dayIndex] = reward;

                GiveRewardToPlayer(reward);
            }

            SaveData();

            if (IsAllDaysClaimed())
            {
            }

            return true;
        }

        void GiveRewardToPlayer(DailyReward reward)
        {
            _currencyManager.AddCoins(reward.rewardAmount);
        }

        bool IsAllDaysClaimed()
        {
            foreach (bool claimed in saveData.claimedDays)
            {
                if (!claimed) return false;
            }
            return true;
        }

        void ResetRewards()
        {
            saveData = new DailyRewardData
            {
                currentDay = 1,
                loginDayCount = 1,
                lastLoginTimestamp = DateTime.Now.ToBinary(),
                canClaimToday = true
            };
            SaveData();
        }

        void SaveData()
        {
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(B_SafetyKey.DAILY_REWARD_SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public List<DailyReward> GetDailyRewards() => dailyRewardItems.dailyReward;
        public int GetCurrentDay() => saveData.currentDay;
        public int GetTotalLoginDays() => saveData.loginDayCount;
        public DailyRewardData GetSaveData() => saveData;

        public TimeSpan GetTimeUntilNextDay()
        {
            DateTime now = DateTime.Now;
            DateTime tomorrow = now.Date.AddDays(1);
            return tomorrow - now;
        }

        public void ResetAllData()
        {
            PlayerPrefs.DeleteKey(B_SafetyKey.DAILY_REWARD_SAVE_KEY);
            saveData = new DailyRewardData();
            UpdateRewardItemsStatus();
        }
    }

    [Serializable]
    public struct DailyReward
    {
        public Sprite icon;
        public int day;
        public int rewardAmount;
        public bool claimed;
    }

    [Serializable]
    public class DailyRewardData
    {
        public int currentDay;
        public int loginDayCount;
        public bool[] claimedDays;
        public long lastLoginTimestamp;
        public bool canClaimToday;

        public DailyRewardData()
        {
            currentDay = 0;
            loginDayCount = 0;
            claimedDays = new bool[7];
            lastLoginTimestamp = 0;
            canClaimToday = false;
        }
    }
}