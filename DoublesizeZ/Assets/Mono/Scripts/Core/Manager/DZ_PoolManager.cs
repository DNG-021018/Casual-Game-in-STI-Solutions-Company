using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_PoolManager : MonoBehaviour
    {
        [Header("VFX Pools")]
        [SerializeField] private DZ_FloatingTextPool floatingTextPool;
        public DZ_FloatingTextPool FloatingTextPool => floatingTextPool;

        [Header("Enemy Pools")]
        [SerializeField] private DZ_EnemyPool enemyPool;
        public DZ_EnemyPool EnemyPool => enemyPool;

        [Header("Effect Pools")]
        [SerializeField] private DZ_EffectPool effectPool;
        public DZ_EffectPool EffectPool => effectPool;
    }
}
