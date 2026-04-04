using UnityEngine;

namespace CataFury
{
    public class CF_PoolManager : MonoBehaviour
    {
        [Header("VFX Pools")]
        [SerializeField] private CF_FloatingTextPool floatingTextPool;
        public CF_FloatingTextPool FloatingTextPool => floatingTextPool;

        [Header("Enemy Pools")]
        [SerializeField] private CF_EnemyPool enemyPool;
        public CF_EnemyPool EnemyPool => enemyPool;

        [Header("Enemy Effect Pools")]
        [SerializeField] private CF_EnemyEffectPool enemyEffectPool;
        public CF_EnemyEffectPool EnemyEffectPool => enemyEffectPool;

        [Header("Projectile Pool")]
        [SerializeField] private CF_ProjectilePool projectilePool;
        public CF_ProjectilePool ProjectilePool => projectilePool;
    }
}