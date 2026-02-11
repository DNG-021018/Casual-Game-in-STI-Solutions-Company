using Cinemachine;
using UnityEngine;

namespace Bowmancer
{
    public class B_GunController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform ShootPoint;
        [SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;

        private B_PoolManager _poolManager;
        private B_BulletPool BulletPool;
        private B_VFXPool HitEffectPool;
        private B_PlayerRef _playerRef;
        private B_BulletModifierSystem _bulletModifierSystem;

        void Awake()
        {
            _poolManager = B_PoolManager.Instance;
            BulletPool = _poolManager.BulletPool;
            HitEffectPool = _poolManager.BulletHitPool;
        }

        void Start()
        {
            cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
            _playerRef = GetComponent<B_PlayerRef>();

            _bulletModifierSystem = _playerRef.GetComponent<B_BulletModifierSystem>();
            if (_bulletModifierSystem == null)
            {
                _bulletModifierSystem = _playerRef.gameObject.AddComponent<B_BulletModifierSystem>();
            }
            _bulletModifierSystem.Initialize(_playerRef);
        }

        public void StartShooting(Transform target)
        {
            if (target == null) return;

            _bulletModifierSystem.ShootProjectiles(ShootPoint, target, BulletPool, HitEffectPool);

            if (cinemachineImpulseSource != null)
                cinemachineImpulseSource.GenerateImpulse();
        }
    }
}