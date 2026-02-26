using System;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_EnemyController : MonoBehaviour, IPoolableWithInit<DZ_EnemyController>
    {
        private Animator animator;

        private Transform targetPosition;
        private float moveSpeed;

        private bool isDead;
        private bool isStopped;

        private Pooler<DZ_EnemyController> pool;

        private readonly int key_deadth = Animator.StringToHash(DZ_SafetyKey.ANIM_TRIGGER_DEAD);
        private readonly int key_attack = Animator.StringToHash(DZ_SafetyKey.ANIM_TRIGGER_ATTACK);

        public event System.Action OnDespawned;

        void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        void OnEnable()
        {
            DZ_PlayerController.OnPlayerDeath += StopMovement;
        }

        void OnDisable()
        {
            DZ_PlayerController.OnPlayerDeath -= StopMovement;
        }

        void FixedUpdate()
        {
            if (isDead || isStopped) return;

            MoveTowardsTarget();
            RotateTowardsTarget();
        }

        private void MoveTowardsTarget()
        {
            Vector3 direction = (targetPosition.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.fixedDeltaTime;
        }

        private void RotateTowardsTarget()
        {
            if (targetPosition == null) return;

            Vector3 direction = (targetPosition.position - transform.position).normalized;

            if (direction == Vector3.zero) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                360f * Time.fixedDeltaTime
            );
        }

        public void InitPool(Pooler<DZ_EnemyController> pool)
        {
            this.pool = pool;
        }

        public void InitPlayerPos(Transform player)
        {
            targetPosition = player;
        }

        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        public void OnGetFromPool()
        {
            isDead = false;
            isStopped = false;
            moveSpeed = 0f;
            ResetAnimation();
        }

        public void OnReturnToPool()
        {
            isDead = true;
            isStopped = false;
            OnDespawned?.Invoke();
            OnDespawned = null;
            moveSpeed = 0f;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            pool.ReturnToPool(this);
        }

        private void ResetAnimation()
        {
            animator.ResetTrigger(key_deadth);
            animator.ResetTrigger(key_attack);
            animator.Rebind();
        }

        public void Death()
        {
            if (isDead) return;

            HandleDeath();
        }

        private void HandleDeath()
        {
            isDead = true;
            animator.SetTrigger(key_deadth);

            Invoke(nameof(ReturnToPool), 0.3f);
        }

        private void ReturnToPool()
        {
            OnReturnToPool();
        }

        private void StopMovement()
        {
            isStopped = true;
        }

        public float GetMoveSpeed()
        {
            return moveSpeed;
        }
    }
}
