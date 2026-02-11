using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace NightEscape
{
    public class NE_Police : NE_AEnemy
    {
        [SerializeField] SplineContainer splinePath;
        [SerializeField] float patrolSpeed = 2f;
        [SerializeField] float rotationSpeed = 2f;
        [SerializeField] float rotationThreshold = 0.95f;
        [SerializeField] ParticleSystem hitEffect;
        [SerializeField] AudioClip hitSound;

        private Animator _animator;
        private List<Vector3> _patrolPoints;
        private NE_VisionCone _visionCone;

        private int _currentPatrolPointIndex = 0;
        private bool _isPatrolPointsInitialized = false;
        private bool _hasDetectedPlayer = false;
        private bool _isDead = false;

        void Start()
        {
            if (splinePath == null)
            {
                splinePath = GetComponentInChildren<SplineContainer>();
            }
            _animator = GetComponentInChildren<Animator>();
            _visionCone = GetComponentInChildren<NE_VisionCone>();
            hitEffect.gameObject.SetActive(false);

            InitializePatrolPoints();
        }

        private void Update()
        {
            if (_isDead) return;

            if (!_hasDetectedPlayer && _isPatrolPointsInitialized)
            {
                PatrolAlongSpline();
            }
        }

        private void InitializePatrolPoints()
        {
            _patrolPoints = new List<Vector3>();
            Spline spline = splinePath.Spline;

            for (int i = 0; i < spline.Count; i++)
            {
                BezierKnot knot = spline[i];
                Vector3 worldPos = splinePath.transform.TransformPoint(knot.Position);
                _patrolPoints.Add(worldPos);
            }

            if (_patrolPoints.Count > 0)
            {
                transform.position = _patrolPoints[0];
            }

            splinePath.gameObject.SetActive(false);

            _isPatrolPointsInitialized = true;
        }

        private void PatrolAlongSpline()
        {
            if (_patrolPoints.Count == 0) return;

            Vector3 targetPosition = _patrolPoints[_currentPatrolPointIndex];
            Vector3 directionToTarget = targetPosition - transform.position;

            float distanceToTarget = directionToTarget.magnitude;

            if (distanceToTarget < 0.1f)
            {
                _currentPatrolPointIndex = (_currentPatrolPointIndex + 1) % _patrolPoints.Count;
                return;
            }

            Vector3 direction = directionToTarget.normalized;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                float dotProduct = Vector3.Dot(transform.forward, direction);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                if (dotProduct > rotationThreshold)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);
                }
            }
        }

        public void DetectedPlayer(NE_PlayerController player)
        {
            if (_hasDetectedPlayer) return;

            _hasDetectedPlayer = true;

            CaughtPlayer(player);
        }

        public override void CaughtPlayer()
        {
            base.CaughtPlayer();
        }

        public override void CaughtPlayer(NE_PlayerController player)
        {
            base.CaughtPlayer(player);
            if (player != null)
            {
                player.GetCaught(true);
                player.RotateToCamera();
                _animator.SetTrigger(NE_SafetyKey.ANIM_POLICE_TRIGGER_CAUGHT);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (_isDead) return;
            if (other.CompareTag(NE_SafetyKey.KEY_TAG_PLAYER))
            {
                _isDead = true;

                if (_visionCone != null)
                {
                    _visionCone.gameObject.SetActive(false);
                }

                if (hitEffect != null)
                {
                    hitEffect.transform.position = other.transform.position;
                    hitEffect.gameObject.SetActive(true);
                }

                if (hitSound != null)
                {
                    NE_AudioManager.Instance.PlaySfx(hitSound, 1f);
                }

                _animator.SetTrigger(NE_SafetyKey.ANIM_POLICE_TRIGGER_HIT);
            }
        }
    }
}
