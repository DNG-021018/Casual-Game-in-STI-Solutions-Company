using UnityEngine;

namespace Bowmancer
{
    public class B_PoolManager : PersistentSingleton<B_PoolManager>
    {
        [Header("VFX Pools")]
        [SerializeField] private B_CoinPool coinPool;
        public B_CoinPool CoinPool => coinPool;

        [SerializeField] private B_CoinBlastVFX coinBlastVFXPool;
        public B_CoinBlastVFX CoinBlastVFXPool => coinBlastVFXPool;

        [SerializeField] private B_FloatingTextPool floatingTextPool;
        public B_FloatingTextPool FloatingTextPool => floatingTextPool;

        [SerializeField] private B_BulletPool bulletPool;
        public B_BulletPool BulletPool => bulletPool;

        [SerializeField] private B_VFXPool bulletHitPool;
        public B_VFXPool BulletHitPool => bulletHitPool;

        [SerializeField] private B_CannonPool cannonPool;
        public B_CannonPool CannonPool => cannonPool;

        [SerializeField] private B_CannonBulletVFXPool cannonBulletPool;
        public B_CannonBulletVFXPool CannonBulletPool => cannonBulletPool;
    }
}
