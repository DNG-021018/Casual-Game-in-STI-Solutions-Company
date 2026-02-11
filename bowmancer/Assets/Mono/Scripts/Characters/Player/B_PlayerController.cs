using Cinemachine;
using UnityEngine;

namespace Bowmancer
{
    public class B_PlayerController : B_AEntity
    {
        [SerializeField] private Color coinPickupColor = Color.yellow;
        [SerializeField] private AudioClip coinCollectClip;

        private float _velocityY;
        private bool _isDead = false;

        private Transform _cameraTarget;
        private Vector2 _movePos;

        private CharacterController _characterController;
        private B_PlayerAnimationController _animationController;
        private CinemachineImpulseSource cinemachineImpulseSource;

        private B_PlayerRef _playerRef;
        private B_CameraManager _cameraManager;
        private B_InputManager _inputManager;
        private B_PlayerDetected _playerDetected;
        private B_CurrencyManager _currencyManager;
        private B_GameManager _gameManager;
        private B_UpgradeManager _upgradeManager;

        protected override void Awake()
        {
            base.Awake();
            _gameManager = B_GameManager.Instance;
            _inputManager = B_InputManager.Instance;
            _cameraManager = B_CameraManager.Instance;
            _currencyManager = B_CurrencyManager.Instance;
            _upgradeManager = B_UpgradeManager.Instance;
        }

        private void Start()
        {
            _playerRef = GetComponent<B_PlayerRef>();
            _upgradeManager.Initialize(_playerRef);
            _animationController = _playerRef.PlayerAnimationController;
            _characterController = _playerRef.CharacterController;
            _cameraTarget = _playerRef.CameraTarget;
            _playerDetected = _playerRef.PlayerDetected;
            _cameraManager.SetTarget(_cameraTarget);
        }

        private void OnEnable()
        {
            _inputManager.OnTouch += HandleTouch;
            _inputManager.OnTouchEnd += HandleTouchEnd;
        }

        private void OnDisable()
        {
            _inputManager.OnTouch -= HandleTouch;
            _inputManager.OnTouchEnd -= HandleTouchEnd;
        }

        private void HandleTouch(Vector2 pos)
        {
            _movePos = pos;
        }

        private void HandleTouchEnd(Vector2 pos)
        {
            _movePos = Vector2.zero;
        }

        private void FixedUpdate()
        {
            HandleMoving();
        }

        protected override void HandleMoving()
        {
            if (_isDead) return;

            ApplyGravity();

            Vector3 moveDir = GetMoveDirection();
            HandleRotation(moveDir);
            ApplyMovement(moveDir);
        }

        protected override void HandleDie()
        {
            _isDead = true;
            _animationController.PlayDeadAnimation();
            _healthbar.gameObject.SetActive(false);
            _gameManager.FinishGame(GameState.Lose);
        }

        protected override void ApplyGravity()
        {
            if (_characterController.isGrounded && _velocityY < 0f)
            {
                _velocityY = -2f;
            }
            else
            {
                _velocityY += Gravity * GravityMultiplier * Time.deltaTime;
            }
        }

        private Vector3 GetMoveDirection()
        {
            if (_movePos.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            Vector3 localInput = new(_movePos.x, 0, _movePos.y);
            Vector3 worldDir = Camera.main.transform.TransformDirection(localInput);
            worldDir.y = 0;

            return worldDir.normalized;
        }

        private void HandleRotation(Vector3 moveDir)
        {
            bool isShooting = _animationController.IsShooting();
            Transform currentTarget = _playerDetected.GetCurrentTarget();

            if (isShooting && currentTarget != null)
            {
                Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
                directionToTarget.y = 0;

                if (directionToTarget.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
                }
            }
            else if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
            }
        }

        protected override void ApplyMovement(Vector3 moveDir)
        {
            Vector3 movement = moveDir * MoveSpeed + Vector3.up * _velocityY;
            _characterController.Move(movement * Time.deltaTime);

            bool isShooting = _animationController.IsShooting();
            if (!isShooting)
            {
                float moveSpeed = moveDir.sqrMagnitude;
                float blendValue = moveSpeed > 0.5f ? 1f : 0.5f;

                _animationController.SetMovingBlend(blendValue);
            }
            else
            {
                Vector3 horizontalVelocity = new(movement.x, 0, movement.z);
                float speed = horizontalVelocity.magnitude;

                float velocityX = 0f;
                float velocityY = 0f;

                if (speed > 0.1f)
                {
                    Vector3 moveDirection = horizontalVelocity.normalized;
                    Vector3 localDirection = transform.InverseTransformDirection(moveDirection);

                    velocityX = localDirection.x * (speed / MoveSpeed);
                    velocityY = localDirection.z * (speed / MoveSpeed);
                }

                velocityX = Mathf.Clamp(velocityX, -0.5f, 0.5f);
                velocityY = Mathf.Clamp(velocityY, -0.5f, 0.5f);

                _animationController.SetShootingVelocity(velocityX, velocityY);
            }
        }

        public override void TakeDamage(float damage)
        {
            if (_isDead) return;
            if (cinemachineImpulseSource != null)
                cinemachineImpulseSource.GenerateImpulse();

            base.TakeDamage(damage);
            B_VolumeController.OnHitEvent?.Invoke();

            float currentHealth = GetCurrentHealth();
            float baseHealth = GetBaseHealth();
            B_VolumeController.OnLowHealthEnterEvent?.Invoke(currentHealth, baseHealth);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(B_SafetyKey.TAG_COIN))
            {
                B_FloatingText floatingText = _floatingText.Get("CoinText", transform.position + Vector3.up * 2f, Quaternion.identity);
                floatingText.InitPool(_floatingText);
                _audioManager.PlaySfx(coinCollectClip);
                floatingText.ShowFloatingText("2", transform, coinPickupColor);
                _currencyManager.AddCoins(2);
            }
        }

        public bool IsDead() => _isDead;
    }
}
