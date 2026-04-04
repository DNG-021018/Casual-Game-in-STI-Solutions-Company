using System.Collections.Generic;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace CataFury
{
    [RequireComponent(typeof(Collider))]
    public class CF_Projectile : MonoBehaviour, IPoolableWithInit<CF_Projectile>
    {
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 12f;
        [SerializeField] private float maxTravelDistance = 20f;

        [Header("VFX")]
        [SerializeField] private ParticleSystem trailParticle;
        [SerializeField] private ParticleSystem hitEffect;

        [Header("SFX")]
        [SerializeField] private AudioClip shootSfx;
        [SerializeField] private AudioClip explodeSfx;


        private Pooler<CF_Projectile> _pool;
        private CF_AudioManager _audioManager;
        private CF_PoolManager _poolManager;
        private CF_CurrencyManager _currencyManager;
        private CF_ScoreManager _scoreManager;
        private CF_PlayerController _playerController;

        private Transform _target;
        private Vector3 _travelDir;
        private Vector3 _spawnPos;
        private float _damage;
        private bool _isPiercing;
        private bool _isMoving;

        private readonly HashSet<Collider> _hitColliders = new();


        public void InitPool(Pooler<CF_Projectile> pool)
        {
            _pool = pool;
            _audioManager = ServiceLocator.Get<CF_AudioManager>();
            _poolManager = ServiceLocator.Get<CF_PoolManager>();
            _currencyManager = ServiceLocator.Get<CF_CurrencyManager>();
            _scoreManager = ServiceLocator.Get<CF_ScoreManager>();
            _playerController = ServiceLocator.Get<CF_PlayerController>();
        }

        public void OnGetFromPool()
        {
            _isMoving = false;
            _hitColliders.Clear();
        }

        public void OnReturnToPool()
        {
            _isMoving = false;
            _target = null;
            _hitColliders.Clear();

            trailParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hitEffect?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }


        public void SetColor(Color color)
        {
            if (trailParticle != null)
            {
                var main = trailParticle.main;
                main.startColor = new ParticleSystem.MinMaxGradient(color);
            }

            if (hitEffect != null)
            {
                var main = hitEffect.main;
                main.startColor = new ParticleSystem.MinMaxGradient(color);
            }
        }


        public void Launch(Transform target, Vector3 direction, float damage, bool isPiercing)
        {
            _target = target;
            _travelDir = direction.normalized;
            _damage = damage;
            _isPiercing = isPiercing;
            _spawnPos = transform.position;
            _isMoving = true;

            trailParticle?.Play(true);
            _audioManager?.PlaySfx(shootSfx);
        }


        void Update()
        {
            if (!_isMoving) return;

            Vector3 dir;
            if (_target != null && _target.gameObject.activeInHierarchy)
                dir = _target.position - transform.position;
            else
            {
                _target = null;
                dir = _travelDir;
            }

            if (dir.sqrMagnitude > 0.001f)
            {
                Vector3 euler = Quaternion.LookRotation(dir.normalized).eulerAngles;
                transform.eulerAngles = new Vector3(0f, euler.y, 0f);
            }

            transform.position += dir.normalized * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(_spawnPos, transform.position) >= maxTravelDistance)
                ReturnToPool();
        }


        void OnTriggerEnter(Collider other)
        {
            if (!_isMoving) return;
            if (_hitColliders.Contains(other)) return;
            if (!other.CompareTag(CF_SafetyKey.Tag.TAG_ENEMY)) return;
            if (!other.TryGetComponent(out IDamageable damageable)) return;

            _hitColliders.Add(other);

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            ProcessHit(damageable, other, hitPoint);

            PlayHitEffect(hitPoint);
            _audioManager?.PlaySfx(explodeSfx);

            if (!_isPiercing)
                ReturnToPool();
        }

        private void ProcessHit(IDamageable damageable, Collider col, Vector3 hitPoint)
        {
            bool killed = damageable.ApplyDamage(_damage, hitPoint);

            _playerController?.TriggerImpulse();

            if (killed)
            {
                _poolManager?.FloatingTextPool
                    .Get("FloatingText",
                         col.transform.position + Vector3.up * 1.5f,
                         Quaternion.identity)
                    ?.ShowFloatingText("+1", col.transform);

                _currencyManager?.AddCoins(1);
                _scoreManager?.AddScore(1);
                _playerController?.RegisterKill();
            }
            else
            {
                _poolManager?.FloatingTextPool
                    .Get("FloatingHealth",
                         col.transform.position + Vector3.up * 1.8f,
                         Quaternion.identity)
                    ?.ShowFloatingText("-1", col.transform);
            }
        }

        private void PlayHitEffect(Vector3 pos)
        {
            if (hitEffect == null) return;
            hitEffect.transform.position = pos;
            hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hitEffect.Play(true);
        }

        private void ReturnToPool()
        {
            _isMoving = false;
            _pool?.ReturnToPool(this);
        }
    }
}