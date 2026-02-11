using System.Collections.Generic;
using UnityEngine;

namespace Bowmancer
{
    [CreateAssetMenu(fileName = "DailyRewardItems", menuName = "Bowmancer/Daily Reward/Items")]
    public class B_DailyRewardItems : ScriptableObject
    {
        public List<DailyReward> dailyReward = new(7);
    }
}
