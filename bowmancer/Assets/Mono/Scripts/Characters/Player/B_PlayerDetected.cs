using UnityEngine;

namespace Bowmancer
{
    public class B_PlayerDetected : MonoBehaviour
    {
        [Header("Detect Settings")]
        [SerializeField] private float detectRadius = 10f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Shoot Check")]
        [SerializeField] private Transform shootPoint;
        [SerializeField] private float shootCooldown = 0.5f;

        private B_PlayerRef _PlayerRef;
        private B_PlayerController _playerController;
        private B_PlayerAnimationController _animationController;
        private B_GunController _gunController;

        private Transform _previousTarget;
        private bool _wasTargeting = false;
        private float _lastShootTime = 0f;

        public Transform CurrentTarget { get; private set; }

        void Start()
        {
            _PlayerRef = GetComponent<B_PlayerRef>();
            _animationController = _PlayerRef.PlayerAnimationController;
            _gunController = _PlayerRef.GunController;
            _playerController = _PlayerRef.PlayerController;
        }

        void FixedUpdate()
        {
            if (_playerController.IsDead()) return;
            DetectEnemy();
        }

        void DetectEnemy()
        {
            CurrentTarget = null;

            Collider[] enemies = Physics.OverlapSphere(transform.position, detectRadius, enemyLayer);

            float closestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                B_EnemyController e = enemy.GetComponent<B_EnemyController>();
                if (e == null || e.IsDead()) continue;
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDistance)
                {
                    if (HasClearShot(enemy.transform))
                    {
                        closestDistance = dist;
                        CurrentTarget = enemy.transform;
                    }
                }
            }

            bool isTargeting = CurrentTarget != null;
            if (isTargeting != _wasTargeting || CurrentTarget != _previousTarget)
            {
                _wasTargeting = isTargeting;
                _previousTarget = CurrentTarget;
            }

            if (isTargeting && Time.time >= _lastShootTime + shootCooldown)
            {
                _gunController.StartShooting(CurrentTarget);
                _animationController.PlayShootingAnimation(true);
                _lastShootTime = Time.time;
            }

            if (!isTargeting)
            {
                _animationController.PlayShootingAnimation(false);
            }
        }

        public Transform GetCurrentTarget() => CurrentTarget;

        bool HasClearShot(Transform enemy)
        {
            Vector3 origin = shootPoint.position;
            Vector3 dir = (enemy.position - origin).normalized;
            float distance = Vector3.Distance(origin, enemy.position);

            if (Physics.Raycast(origin, dir, out RaycastHit hit, distance, obstacleLayer))
            {
                return false;
            }

            return true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRadius);

            if (shootPoint != null && CurrentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(shootPoint.position, CurrentTarget.position);
            }
        }
#endif
    }
}