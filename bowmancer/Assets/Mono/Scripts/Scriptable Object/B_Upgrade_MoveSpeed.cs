using UnityEngine;

namespace Bowmancer
{
    [CreateAssetMenu(fileName = "Upgrade_MoveSpeed", menuName = "Bowmancer/Upgrades/PlayerStat/Move Speed")]
    public class B_Upgrade_MoveSpeed : B_Upgrade_PlayerStat
    {
        private float _baseSpeed = 0f;
        private float _currentModifier = 0f;

        public override void Apply(B_PlayerRef playerRef, int currentLevel)
        {
            float speedModifier = baseIncrease + (increasePerLevel * (currentLevel - 1));

            ApplyStatModifier(playerRef, speedModifier);
            _currentModifier = speedModifier;
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
                if (_baseSpeed == 0f)
                    _baseSpeed = player.MoveSpeed;

                if (isPercentage)
                {
                    player.SetMoveSpeed(_baseSpeed * (1f + value));
                }
                else
                {
                    player.SetMoveSpeed(_baseSpeed + value);
                }
            }
        }

        protected override void RemoveStatModifier(B_PlayerRef playerRef, float value)
        {
            var player = playerRef.PlayerController;
            if (player != null && _baseSpeed > 0f)
            {
                player.MoveSpeed = _baseSpeed;
            }
        }

        public override string GetDetailedDescription(int level)
        {
            float speedIncrease = baseIncrease + (increasePerLevel * (level - 1));
            string valueText = isPercentage ? $"{speedIncrease * 100}%" : $"{speedIncrease}";
            return $"Increase movement speed by {valueText}.\nDodge attacks more easily!";
        }
    }
}