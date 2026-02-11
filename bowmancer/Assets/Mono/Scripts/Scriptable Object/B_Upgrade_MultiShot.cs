using UnityEngine;

namespace Bowmancer
{
    [CreateAssetMenu(fileName = "Upgrade_MultiShot", menuName = "Bowmancer/Upgrades/Bullet/MultiShot")]
    public class B_Upgrade_MultiShot : B_BaseUpgrade
    {
        [Header("MultiShot Settings")]
        [SerializeField] private float spreadAnglePerSide = 15f;
        [SerializeField] private float damageIncreasePerLevel = 0.5f;

        private MultiShotModifier _modifier;

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

            float damageMultiplier = 1f + (damageIncreasePerLevel * (currentLevel - 1));
            _modifier = new MultiShotModifier(spreadAnglePerSide, damageMultiplier);

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
            float damageBonus = damageIncreasePerLevel * (level - 1);
            return $"Fire 2 additional side shots.\nSide shot damage: +{damageBonus * 100}%\nSpread angle: {spreadAnglePerSide}° per side";
        }

        private class MultiShotModifier : IBulletModifier
        {
            private float _spreadAngle;
            private float _damageMultiplier;

            public MultiShotModifier(float spread, float damageMultiplier)
            {
                _spreadAngle = spread;
                _damageMultiplier = damageMultiplier;
            }

            public void ModifyStats(ref BulletStats stats)
            {
                stats.projectileCount = 3;
                stats.spreadAngle = _spreadAngle;
                stats.sideProjectileDamageMultiplier = _damageMultiplier;
            }

            public void OnBulletFired(B_Bullet bullet, Transform target)
            {
            }
        }
    }
}