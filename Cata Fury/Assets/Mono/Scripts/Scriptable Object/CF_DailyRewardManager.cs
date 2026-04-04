using System;
using System.Collections.Generic;
using UnityEngine;

namespace CataFury
{
    [CreateAssetMenu(fileName = "Daily Reward Manager", menuName = CF_SafetyKey.KEY_GAME_NAME + "/Daily Reward Manager")]
    public class CF_DailyRewardManager : ScriptableObject
    {
        [SerializeField] private CF_CurrencyManager _currencyManager;
        [SerializeField] private List<DailyReward> dailyReward;

        private DailyRewardData saveData;

        public void Init()
        {
            LoadDailyRewards();
            CheckNewLoginDay();
        }

        void LoadDailyRewards()
        {
            string json = PlayerPrefs.GetString(CF_SafetyKey.Data.DAILY_REWARD_SAVE_KEY, "");

            if (string.IsNullOrEmpty(json))
            {
                saveData = new DailyRewardData();
                SaveData();
            }
            else
            {
                saveData = JsonUtility.FromJson<DailyRewardData>(json);
            }
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
            }
            else if (isNewDay)
            {
                int daysPassed = (now.Date - lastLogin.Date).Days;

                if (daysPassed == 1)
                {
                    saveData.loginDayCount++;
                    saveData.currentDay = ((saveData.currentDay) % 7) + 1;
                }
                else
                {
                    ResetRewards();
                }

                saveData.lastLoginTimestamp = now.ToBinary();
                saveData.canClaimToday = true;
                SaveData();
            }
            else
            {
                saveData.canClaimToday = !saveData.claimedDays[saveData.currentDay - 1];
            }
        }

        public bool ClaimTodayReward()
        {
            if (!CanClaimToday()) return false;

            int dayIndex = saveData.currentDay - 1;

            saveData.claimedDays[dayIndex] = true;
            saveData.canClaimToday = false;

            if (dayIndex < dailyReward.Count)
            {
                GiveRewardToPlayer(dailyReward[dayIndex]);
            }

            SaveData();
            return true;
        }

        void GiveRewardToPlayer(DailyReward reward)
        {
            _currencyManager.AddCoins(reward.rewardAmount);
        }

        public bool CanClaimToday()
        {
            if (saveData.currentDay == 0) return false;
            return saveData.canClaimToday;
        }

        public bool HasRewardToday() => saveData.canClaimToday;

        public bool IsClaimed(int index)
        {
            if (index < 0 || index >= saveData.claimedDays.Length) return false;
            return saveData.claimedDays[index];
        }

        public DailyReward GetRewardConfig(int index)
        {
            if (index < 0 || index >= dailyReward.Count) return default;
            return dailyReward[index];
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
            PlayerPrefs.SetString(CF_SafetyKey.Data.DAILY_REWARD_SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public List<DailyReward> GetDailyRewards() => dailyReward;
        public int GetCurrentDay() => saveData.currentDay;
        public DailyRewardData GetSaveData() => saveData;

        public void ResetAllData()
        {
            PlayerPrefs.DeleteKey(CF_SafetyKey.Data.DAILY_REWARD_SAVE_KEY);
            saveData = new DailyRewardData();
        }
    }

    [Serializable]
    public struct DailyReward
    {
        public Sprite icon;
        public int day;
        public int rewardAmount;
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
