using UnityEngine;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;

namespace Bowmancer
{
    public class B_Bullet : MonoBehaviour, IPoolableWithInit<B_Bullet>
    {
        [Header("Settings")]
        [SerializeField] private float knockBackStrength = 5f;
        [SerializeField] private float maxLifeTime = 5f;
        [SerializeField] private LayerMask blockLayer;

        private Vector3 _direction = Vector3.forward;
        private bool _useDirection = false;
        private bool _isInitialized = false;
        private float _damage;
        private float _lifeTime = 0f;
        private Vector3 _StartPosition;
        public Transform Target { get; private set; }
        private Pooler<B_Bullet> _bulletPool;
        private Pooler<B_HitEffect> _effectPool;

        public void InitPool(Pooler<B_Bullet> pool)
        {
            _bulletPool = pool;
        }

        public void InitHitEffectPool(Pooler<B_HitEffect> pool)
        {
            _effectPool = pool;
        }

        public void OnGetFromPool()
        {
            gameObject.SetActive(true);
            _isInitialized = false;
            _lifeTime = 0f;
            _useDirection = false;
        }

        public void OnReturnToPool()
        {
            gameObject.SetActive(false);
            _bulletPool.ReturnToPool(this);
            _isInitialized = false;
            _lifeTime = 0f;
        }

        public void InitializedWithDirection(float damage, Transform startPos, Vector3 direction)
        {
            _damage = damage;
            _StartPosition = startPos.position;
            _direction = direction.normalized;
            _useDirection = true;
            Target = null;

            transform.position = startPos.position;
            if (_direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(_direction);
            }

            _isInitialized = true;
        }

        void Update()
        {
            if (!_isInitialized) return;
            _lifeTime += Time.deltaTime;

            if (_lifeTime >= maxLifeTime)
            {
                OnReturnToPool();
                return;
            }

            float step = 50f * Time.deltaTime;

            if (_useDirection)
            {
                transform.position += _direction * step;
            }
            else if (Target != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, Target.position, step);

                Vector3 direction = (Target.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }

                if (Vector3.Distance(transform.position, Target.position) < 0.001f)
                {
                    OnReturnToPool();
                }
            }
            else
            {
                OnReturnToPool();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            Vector3 hitPos = transform.position;
            B_HitEffect hitEffect = _effectPool.Get("hitEffect", hitPos, Quaternion.identity);
            hitEffect.InitPool(_effectPool);
            hitEffect.OnReturnToPool();

            if (((1 << other.gameObject.layer) & blockLayer) != 0)
            {
                OnReturnToPool();
                return;
            }

            if (other.TryGetComponent(out B_IDamage entity))
            {
                float finalDamage = _damage;
                entity.TakeDamage(finalDamage);
            }

            if (other.TryGetComponent(out B_IKnockbackable knockbackable))
            {
                Vector3 forceDir = (other.transform.position - _StartPosition).normalized;
                Vector3 force = forceDir * knockBackStrength;
                knockbackable.GetKnockedBack(force);
            }

            OnReturnToPool();
        }
    }
}
