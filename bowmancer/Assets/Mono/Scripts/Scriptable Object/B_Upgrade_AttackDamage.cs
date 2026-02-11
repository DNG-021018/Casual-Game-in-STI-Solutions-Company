using UnityEngine;

namespace Bowmancer
{
    [CreateAssetMenu(fileName = "Upgrade_AttackDamage", menuName = "Bowmancer/Upgrades/PlayerStat/Attack Damage")]
    public class B_Upgrade_AttackDamage : B_Upgrade_PlayerStat
    {
        private float _baseDamage = 0f;
        private float _currentModifier = 0f;

        public override void Apply(B_PlayerRef playerRef, int currentLevel)
        {
            float damageModifier = baseIncrease + (increasePerLevel * (currentLevel - 1));

            ApplyStatModifier(playerRef, damageModifier);
            _currentModifier = damageModifier;
        }

        public override void Remove(B_PlayerRef playerRef)
        {
            RemoveStatModifier(playerRef, _currentModifier);
            _currentModifier = 0f;
        }

        protected override void ApplyStatModifier(B_PlayerRef playerRef, float value)
        {
            var player = playerRef.PlayerController;
            if (player != null)
            {
                if (_baseDamage == 0f)
                    _baseDamage = player.AttackPower;

                if (isPercentage)
                {
                    player.SetAttackPower(_baseDamage * (1f + value));
                }
                else
                {
                    player.SetAttackPower(_baseDamage + value);
                }
            }
        }

        protected override void RemoveStatModifier(B_PlayerRef playerRef, float value)
        {
            var player = playerRef.PlayerController;
            if (player != null && _baseDamage > 0f)
            {
                player.AttackPower = _baseDamage;
            }
        }

        public override string GetDetailedDescription(int level)
        {
            float damageIncrease = baseIncrease + (increasePerLevel * (level - 1));
            string valueText = isPercentage ? $"{damageIncrease * 100}%" : $"{damageIncrease}";
            return $"Increase attack damage by {valueText}.\nDeal more damage to enemies!";
        }
    }
}
