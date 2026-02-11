using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Bowmancer
{
    public class B_DailyRewardMenu : B_UIPage
    {
        [Header("Panel Root")]
        [SerializeField] Panels popup;

        [Header("Buttons")]
        [SerializeField] B_UIButton[] exitButton;

        [Header("Reward Items")]
        [SerializeField] B_DailyRewardItem[] dailyRewardItems;
        [SerializeField] Transform coinSpawnPoint;

        Vector3 popupOriginalScale;
        Vector3[] itemOriginalScales;

        B_BaseUI parent;
        B_DailyRewardManager dailyRewardManager;
        B_CurrencyManager currencyManager;
        List<DailyReward> rewards;

        public override void Init(B_BaseUI parent)
        {
            base.Init(parent);
            this.parent = parent;
            dailyRewardManager = B_DailyRewardManager.Instance;
            currencyManager = B_CurrencyManager.Instance;
        }

        void Start()
        {
            CacheScale();
            InitializeRewardUI();
        }

        public void CacheScale()
        {
            popupOriginalScale = popup.panel.localScale;

            itemOriginalScales = new Vector3[dailyRewardItems.Length];
            for (int i = 0; i < dailyRewardItems.Length; i++)
            {
                itemOriginalScales[i] = dailyRewardItems[i].transform.localScale;
            }
        }

        void InitializeRewardUI()
        {
            if (dailyRewardItems == null || dailyRewardItems.Length == 0) return;
            rewards = dailyRewardManager.GetDailyRewards();
            for (int i = 0; i < dailyRewardItems.Length; i++)
            {
                DailyReward reward = rewards[i];
                B_DailyRewardItem item = dailyRewardItems[i];

                item.SetInfo(reward.rewardAmount, reward.day.ToString(), reward.icon, coinSpawnPoint);
                UpdateRewardItemState(i, reward);
            }
        }

        void OnEnable()
        {
            foreach (B_UIButton btn in exitButton)
            {
                btn.Bind(() =>
                {
                    parent.Back();
                });
            }

            for (int i = 0; i < dailyRewardItems.Length; i++)
            {
                int index = i;
                dailyRewardItems[i].Bind(() =>
                {
                    if (OnRewardButtonClicked(index))
                    {
                        dailyRewardItems[index].OnRewardClaimed();
                    }
                });
            }
        }

        void OnDisable()
        {
            foreach (B_UIButton btn in exitButton)
            {
                btn.UnBind();
            }

            for (int i = 0; i < dailyRewardItems.Length; i++)
            {
                dailyRewardItems[i].UnBind();
            }
        }

        bool OnRewardButtonClicked(int dayIndex)
        {
            if (dayIndex == dailyRewardManager.GetCurrentDay() - 1)
            {
                if (dailyRewardManager.ClaimTodayReward())
                {
                    UpdateRewardItemState(dayIndex, rewards[dayIndex]);
                    return true;
                }
            }
            return false;
        }

        void UpdateRewardItemState(int dayIndex, DailyReward reward)
        {
            B_DailyRewardItem item = dailyRewardItems[dayIndex];

            if (reward.claimed)
            {
                item.SetRewardState(StateDailyReward.AlreadyClaimed);
            }
            else if (dayIndex == dailyRewardManager.GetCurrentDay() - 1)
            {
                item.SetRewardState(StateDailyReward.Active);
            }
            else
            {
                item.SetRewardState(StateDailyReward.Inactive);
            }
        }

        public override IEnumerator Show()
        {
            canvasGroup.alpha = 1f;
            yield return base.Show();
            InitializeRewardUI();

            popup.panel.DOKill();
            popup.panel.localScale = popupOriginalScale * 0.8f;

            popup.panel
                .DOScale(popupOriginalScale, popup.duration)
                .SetEase(popup.showEase);

            for (int i = 0; i < dailyRewardItems.Length; i++)
            {
                Transform t = dailyRewardItems[i].transform;
                t.DOKill();

                t.localScale = Vector3.zero;

                t.DOScale(itemOriginalScales[i], 0.35f)
                 .SetEase(Ease.OutBack)
                 .SetDelay(0.05f * i);
            }

            yield return new WaitForSeconds(popup.duration);
        }

        public override IEnumerator Hide()
        {
            for (int i = 0; i < dailyRewardItems.Length; i++)
            {
                dailyRewardItems[i].transform.DOKill();
            }

            popup.panel.DOKill();

            popup.panel
                .DOScale(Vector3.zero, popup.duration * 0.8f)
                .SetEase(popup.hideEase);

            yield return new WaitForSeconds(popup.duration * 0.8f);
            canvasGroup.alpha = 0f;
            yield return base.Hide();
        }
    }
}
