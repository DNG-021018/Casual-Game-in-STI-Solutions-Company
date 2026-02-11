using UnityEngine;

namespace Bowmancer
{
    public abstract class B_BaseUpgrade : ScriptableObject
    {
        [Header("UI Display Info")]
        [SerializeField] private string upgradeName;
        [SerializeField] private Sprite upgradeIcon;
        [SerializeField, TextArea(3, 5)] private string description;
        [SerializeField] private UpgradeCategory category;

        [Header("Upgrade Properties")]
        [SerializeField]
        private int maxLevel = 1;
        [SerializeField] private bool canStack = false;

        public string UpgradeName => upgradeName;
        public Sprite UpgradeIcon => upgradeIcon;
        public string Description => description;
        public UpgradeCategory Category => category;
        public int MaxLevel => maxLevel;
        public bool CanStack => canStack;

        public abstract void Apply(B_PlayerRef playerRef, int currentLevel);

        public abstract void Remove(B_PlayerRef playerRef);

        public virtual string GetDetailedDescription(int level)
        {
            return description;
        }
    }
}
