using System.Collections;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace CataFury
{
    public class CF_EnemyController : MonoBehaviour, IPoolableWithInit<CF_EnemyController>, IDamageable
    {
        [Header("Stats")]
        [SerializeField] private float health = 1f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2f;

        [Header("Effects")]
        [SerializeField] private Material hitFlashMaterial;
        [SerializeField] private float hitFlashDuration = 0.08f;
        [SerializeField] private SkinnedMeshRenderer[] meshRenderers;

        private Material[] _originalMaterials;
        private Coroutine _hitFlashCoroutine;

        private Pooler<CF_EnemyEffect> _enemyEffectPool;
        private float originHealth;
        private float _originY;
        private EDirection _direction;
        private Pooler<CF_EnemyController> pool;

        private Vector3 _targetPos;
        private bool _isMoving;

        void Awake()
        {
            _originY = 1.3f;
            _enemyEffectPool = ServiceLocator.Get<CF_PoolManager>().EnemyEffectPool;

            if (meshRenderers == null || meshRenderers.Length == 0)
                meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            _originalMaterials = new Material[meshRenderers.Length];
            for (int i = 0; i < meshRenderers.Length; i++)
                if (meshRenderers[i] != null)
                    _originalMaterials[i] = meshRenderers[i].material;
        }

        void OnEnable() => CF_PlayerController.OnPlayerDead += HandleVictory;
        void OnDisable() => CF_PlayerController.OnPlayerDead -= HandleVictory;

        void Update()
        {
            if (!_isMoving) return;

            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetPos,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, _targetPos) < 0.05f)
            {
                _isMoving = false;
            }
        }


        public void InitPool(Pooler<CF_EnemyController> pool)
        {
            this.pool = pool;
            originHealth = health;
        }

        public void OnGetFromPool()
        {
            health = originHealth;
            _isMoving = false;
            transform.rotation = Quaternion.identity;
            RestoreOriginalMaterial();
        }

        public void OnReturnToPool()
        {
            _isMoving = false;

            if (_hitFlashCoroutine != null)
            {
                StopCoroutine(_hitFlashCoroutine);
                _hitFlashCoroutine = null;
            }

            health = originHealth;
            transform.rotation = Quaternion.identity;
            RestoreOriginalMaterial();
        }


        public void Init(EDirection direction, Vector3 spawnPos, Vector3 centerPos)
        {
            _direction = direction;

            transform.position = new Vector3(spawnPos.x, _originY, spawnPos.z);
            _targetPos = new Vector3(centerPos.x, _originY, centerPos.z);

            Vector3 lookDir = (_targetPos - transform.position).normalized;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);

            _isMoving = true;
        }


        public bool ApplyDamage(float damage, Vector3 hitPoint)
        {
            health -= damage;
            PlayHitFlash();

            if (health <= 0f)
            {
                CF_EnemyEffect fx = _enemyEffectPool?.GetRandom(hitPoint, Quaternion.identity);
                fx?.PlayParticleEffectsAt(hitPoint);
                ReturnToPool();
                return true;
            }

            return false;
        }


        private void PlayHitFlash()
        {
            if (meshRenderers == null || meshRenderers.Length == 0 || hitFlashMaterial == null) return;
            if (_hitFlashCoroutine != null) StopCoroutine(_hitFlashCoroutine);
            _hitFlashCoroutine = StartCoroutine(HitFlashCoroutine());
        }

        private IEnumerator HitFlashCoroutine()
        {
            foreach (var mr in meshRenderers)
                if (mr) mr.material = hitFlashMaterial;

            yield return new WaitForSeconds(hitFlashDuration);
            RestoreOriginalMaterial();
            _hitFlashCoroutine = null;
        }

        private void RestoreOriginalMaterial()
        {
            for (int i = 0; i < meshRenderers.Length; i++)
                if (meshRenderers[i] != null && _originalMaterials != null && i < _originalMaterials.Length)
                    meshRenderers[i].material = _originalMaterials[i];
        }

        private void HandleVictory()
        {
            _isMoving = false;
        }

        private void ReturnToPool()
        {
            _isMoving = false;
            pool?.ReturnToPool(this);
        }

        public float GetOriginY() => _originY;
    }
}
