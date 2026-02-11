using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bowmancer
{

    [CreateAssetMenu(fileName = "Upgrade_MaxHealth", menuName = "Bowmancer/Upgrades/PlayerStat/Max Health")]
    public class B_Upgrade_MaxHealth : B_Upgrade_PlayerStat
    {
        private float _totalHealthAdded = 0f;
        private float _baseMaxHealth = 0f;

        public override void Apply(B_PlayerRef playerRef, int currentLevel)
        {
            float healthToAdd = baseIncrease + (increasePerLevel * (currentLevel - 1));

            ApplyStatModifier(playerRef, healthToAdd);
            _totalHealthAdded = healthToAdd;
        }

        public override void Remove(B_PlayerRef playerRef)
        {
            RemoveStatModifier(playerRef, _totalHealthAdded);
            _totalHealthAdded = 0f;
            _baseMaxHealth = 0f;
        }

        protected override void ApplyStatModifier(B_PlayerRef playerRef, float value)
        {
            var player = playerRef.PlayerController;
            if (player != null)
            {
                float newMaxHealth = player.MaxHealth + value;
                float healthDifference = newMaxHealth - player.MaxHealth;

                player.SetMaxHealth(newMaxHealth);
                player.Heal(healthDifference);
            }
        }

        protected override void RemoveStatModifier(B_PlayerRef playerRef, float value)
        {
            var player = playerRef.PlayerController;
            if (player != null && _baseMaxHealth > 0f)
            {
                player.SetMaxHealth(_baseMaxHealth);
                if (player.Health > _baseMaxHealth)
                {
                    player.Health = _baseMaxHealth;
                }
            }
        }

        public override string GetDetailedDescription(int level)
        {
            float healthIncrease = baseIncrease + (increasePerLevel * (level - 1));
            return $"Increase maximum health by {healthIncrease}.\nSurvive longer in battle!";
        }
    }
}