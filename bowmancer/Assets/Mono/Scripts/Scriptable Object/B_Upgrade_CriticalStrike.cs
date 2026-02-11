using UnityEngine;

namespace Bowmancer
{
    [CreateAssetMenu(fileName = "Upgrade_CriticalStrike", menuName = "Bowmancer/Upgrades/Bullet/Critical Strike")]
    public class B_Upgrade_CriticalStrike : B_BaseUpgrade
    {
        [Header("Critical Strike Settings")]
        [SerializeField] private float baseCritChance = 0.15f;
        [SerializeField] private float critChancePerLevel = 0.10f;
        [SerializeField] private float baseCritMultiplier = 2f;
        [SerializeField] private float critMultiplierPerLevel = 0.5f;

        [Header("Visual Effects")]
        [SerializeField] private Color critColor = new Color(1f, 0.5f, 0f);

        private CriticalStrikeModifier _modifier;

        public override void Apply(B_PlayerRef playerRef, int currentLevel)
        {
            var modifierSystem = playerRef.GetComponent<B_BulletModifierSystem>();
            if (modifierSystem == null)
            {
                modifierSystem = playerRef.gameObject.AddComponent<B_BulletModifierSystem>();
                modifierSystem.Initialize(playerRef);
            }

            if (_modifier != null)
            {
                modifierSystem.RemoveModifier(_modifier);
            }

            float critChance = baseCritChance + (critChancePerLevel * (currentLevel - 1));
            float critMultiplier = baseCritMultiplier + (critMultiplierPerLevel * (currentLevel - 1));

            _modifier = new CriticalStrikeModifier(critChance, critMultiplier, critColor);
            modifierSystem.AddModifier(_modifier);
        }

        public override void Remove(B_PlayerRef playerRef)
        {
            var modifierSystem = playerRef.GetComponent<B_BulletModifierSystem>();
            if (modifierSystem != null && _modifier != null)
            {
                modifierSystem.RemoveModifier(_modifier);
                _modifier = null;
            }
        }

        public override string GetDetailedDescription(int level)
        {
            float critChance = baseCritChance + (critChancePerLevel * (level - 1));
            float critMultiplier = baseCritMultiplier + (critMultiplierPerLevel * (level - 1));

            return $"{critChance * 100}% chance to deal {critMultiplier}x damage.\nCritical hits are devastating!";
        }

        private class CriticalStrikeModifier : IBulletModifier
        {
            private float _critChance;
            private float _critMultiplier;
            private Color _critColor;

            public CriticalStrikeModifier(float chance, float multiplier, Color color)
            {
                _critChance = chance;
                _critMultiplier = multiplier;
                _critColor = color;
            }

            public void ModifyStats(ref BulletStats stats)
            {
                stats.critChance = _critChance;
                stats.critMultiplier = _critMultiplier;
                stats.critColor = _critColor;
            }

            public void OnBulletFired(B_Bullet bullet, Transform target)
            {
            }
        }
    }
}