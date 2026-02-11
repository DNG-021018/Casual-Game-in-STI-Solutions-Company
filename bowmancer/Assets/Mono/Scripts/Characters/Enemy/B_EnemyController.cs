using System.Collections;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;
using UnityEngine.AI;

namespace Bowmancer
{
    [RequireComponent(typeof(B_EnemyRef), typeof(NavMeshAgent))]
    public class B_EnemyController : B_AEntity, B_IKnockbackable
    {
        [Header("Settings")]
        [SerializeField] private float updatedTime = 0.1f;
        [SerializeField] private float Attackdelay = 0.5f;
        [SerializeField] private float AttackRadius = 1.5f;


        [Header("Knockback Settings")]
        [SerializeField] private float _knockbackCooldown = 1f;
        private float _lastKnockbackTime = 0f;
        private bool _isDead = false;
        private bool _isAttacking = false;

        private B_EnemyAnimationController _animationController;

        private B_EnemyRef _enemyRef;
        private B_EnemyDetected _enemyDetected;
        private B_AttackRadius _attackRadius;
        private NavMeshAgent _navMeshAgent;
        private CapsuleCollider _capsuleCollider;

        private B_AudioManager _AudioManager;

        private Pooler<B_CoinVFX> _coinVFX;
        private Pooler<B_Coin> _coin;

        private Coroutine LookCoroutine;
        private Coroutine AttackCoroutine;
        private Coroutine UpdateCoroutine;

        protected override void Awake()
        {
            base.Awake();
            _AudioManager = B_AudioManager.Instance;
            _enemyRef = GetComponent<B_EnemyRef>();
            _enemyDetected = _enemyRef.EnemyDetected;

            _attackRadius = _enemyRef.AttackRadius;
            _attackRadius.Init(AttackPower, AttackRadius, Attackdelay);

            _animationController = _enemyRef.EnemyAnimationController;
            _navMeshAgent = _enemyRef.NavMeshAgent;
            _capsuleCollider = _enemyRef.CapsuleCollider;

            _coinVFX = _poolManager.CoinBlastVFXPool;
            _coin = _poolManager.CoinPool;

            _isDead = false;
            _isAttacking = false;

            _attackRadius.OnAttack += OnAttack;
        }

        private void OnAttack(B_IDamage target)
        {
            if (_isAttacking) return;

            _isAttacking = true;
            _navMeshAgent.isStopped = true;
            _animationController.PlayAttackAnimation();

            if (LookCoroutine != null)
            {
                StopCoroutine(LookCoroutine);
            }

            LookCoroutine = StartCoroutine(LookAt(_enemyDetected.Target));
            if (AttackCoroutine != null)
            {
                StopCoroutine(AttackCoroutine);
            }
            AttackCoroutine = StartCoroutine(StopAttackAfterDelay());
        }

        private IEnumerator StopAttackAfterDelay()
        {
            yield return new WaitForSeconds(Attackdelay);
            _isAttacking = false;
            _navMeshAgent.isStopped = false;
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        void Start()
        {
            _navMeshAgent.speed = MoveSpeed;
            _navMeshAgent.angularSpeed = RotationSpeed;

            // TriggerEnemyUpdate();
        }

        public void TriggerEnemyUpdate()
        {
            UpdateCoroutine = StartCoroutine(UpdateEnemy());
        }

        private IEnumerator UpdateEnemy()
        {
            WaitForSeconds wait = new(updatedTime);

            while (enabled)
            {
                if (_isDead) yield break;

                if (LookCoroutine != null)
                {
                    StopCoroutine(LookCoroutine);
                }

                if (!_isDead)
                {
                    LookCoroutine = StartCoroutine(LookAt(_enemyDetected.Target));
                }

                HandleMoving();
                yield return wait;
            }
        }

        protected override void HandleMoving()
        {
            if (_isAttacking) return;
            if (_enemyDetected.Target == null)
            {
                _animationController.SetMovingBlend(0f);
                return;
            }

            float distance = Vector3.Distance(transform.position, _enemyDetected.Target.position);
            float speed = distance > 1.5f ? 1f : 0f;

            _navMeshAgent.SetDestination(_enemyDetected.Target.position);

            _animationController.SetMovingBlend(speed);
        }

        IEnumerator LookAt(Transform target)
        {
            while (true)
            {
                // Thêm check _isDead trong LookAt loop
                if (_isDead || target == null) yield break;

                Vector3 direction = (target.position - transform.position).normalized;
                direction.y = 0f;

                if (direction == Vector3.zero) yield return null;

                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = lookRotation;
                yield return null;
            }
        }

        protected override void HandleDie()
        {
            _isDead = true;

            if (UpdateCoroutine != null)
            {
                StopCoroutine(UpdateCoroutine);
                UpdateCoroutine = null;
            }

            if (LookCoroutine != null)
            {
                StopCoroutine(LookCoroutine);
                LookCoroutine = null;
            }
            if (AttackCoroutine != null)
            {
                StopCoroutine(AttackCoroutine);
                AttackCoroutine = null;
            }
            _navMeshAgent.velocity = Vector3.zero;
            _navMeshAgent.isStopped = true;
            _AudioManager.PlaySfx(DieClip);
            _capsuleCollider.enabled = false;
            _animationController.PlayDeadAnimation();
            _healthbar.gameObject.SetActive(false);
            B_CoinVFX vfx = _coinVFX.GetRandom(transform.position, Quaternion.identity);
            vfx.InitPool(_coinVFX);
            vfx.InitCoinPool(_coin);
            vfx.OnGetFromPool();
            StartCoroutine(DisableAfterDeath());
        }

        private IEnumerator DisableAfterDeath()
        {
            yield return new WaitForSeconds(3f);
            gameObject.SetActive(false);
        }

        public override void TakeDamage(float damage)
        {
            if (_isDead) return;
            base.TakeDamage(damage);
            _animationController.PlayGetHitAnimation();
        }

        public void GetKnockedBack(Vector3 force)
        {
            if (Time.time < _lastKnockbackTime + _knockbackCooldown) return;

            Vector3 knockDir = force.normalized;
            float knockDistance = force.magnitude * 0.1f;

            Vector3 targetPos = transform.position + knockDir * knockDistance;

            StartCoroutine(KnockbackRoutine(targetPos));

            _lastKnockbackTime = Time.time;
        }

        private IEnumerator KnockbackRoutine(Vector3 targetPos)
        {
            _navMeshAgent.isStopped = true;

            float duration = 0.15f;
            float time = 0f;
            Vector3 startPos = transform.position;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;

                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            _navMeshAgent.isStopped = false;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(B_SafetyKey.TAG_PLAYER))
            {
                B_AEntity playerEntity = other.gameObject.GetComponent<B_AEntity>();
                if (playerEntity != null)
                {
                    playerEntity.TakeDamage(1f);
                }
            }
        }

        public bool IsDead() => _isDead;
        protected override void ApplyGravity() { }
        protected override void ApplyMovement(Vector3 moveDir) { }

        public void DealDamageEvent()
        {
            _attackRadius.ExecuteAttack();
        }
    }
}
