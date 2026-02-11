using UnityEngine;

namespace Bowmancer
{
    public abstract class B_Upgrade_PlayerStat : B_BaseUpgrade
    {
        [Header("Stat Modifier")]
        [SerializeField] protected float baseIncrease;
        [SerializeField] protected float increasePerLevel;
        [SerializeField] protected bool isPercentage = false;

        protected abstract void ApplyStatModifier(B_PlayerRef playerRef, float value);
        protected abstract void RemoveStatModifier(B_PlayerRef playerRef, float value);
    }
}
