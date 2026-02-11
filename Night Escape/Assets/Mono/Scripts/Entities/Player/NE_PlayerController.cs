using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace NightEscape
{
    [RequireComponent(typeof(CharacterController))]
    public class NE_PlayerController : MonoBehaviour
    {
        [Header("Move Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _swipeThreshold = 50f;

        [Header("Layers")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private LayerMask _obstacleLayer;

        [Header("VFX")]
        [SerializeField] private ParticleSystem RunVFX;
        [SerializeField] private ParticleSystem shockVFX;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip PlayerMove;
        [SerializeField] private AudioClip PlayerTouchWall;

        [Header("Physics Settings")]
        [SerializeField] private float stepsize = 2f;
        [SerializeField] public GameObject targetCamera;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private float _blendChangeSpeed = 5f;

        // CONSTANTS
        private string _blendParam = NE_SafetyKey.ANIM_PLAYER_BLEND_IDLE_RUN;
        private string _dogCatchTrigger = NE_SafetyKey.ANIM_PLAYER_TRIGGER_DOG_CATCH;
        private string _getCatchTrigger = NE_SafetyKey.ANIM_PLAYER_TRIGGER_GET_CATCH;
        private string _reachGoalTrigger = NE_SafetyKey.ANIM_PLAYER_TRIGGER_REACH_GOAL;
        private string _shockTrigger = NE_SafetyKey.ANIM_PLAYER_TRIGGER_GET_SHOCK;

        // COMPONENTS
        private CharacterController _characterController;
        private NE_AudioManager _audioManager => NE_AudioManager.Instance;

        // VARIABLES
        private float _currentBlend = 0f;
        private bool _isMoving;
        private bool _getCaught;
        private bool _reachedGoal;

        private System.Action _cooldownFinishedHandler;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            _cooldownFinishedHandler = () =>
            {
                if (this != null && gameObject != null)
                {
                    GetCaught(true);
                }
            };
        }

        void Start()
        {
            if (RunVFX != null)
            {
                RunVFX.Stop();
            }

            NE_CameraManager.Instance.SetTarget(transform, NE_CameraManager.Instance.GetLoseGameCamera());
        }

        private void OnEnable()
        {
            if (NE_InputManager.Instance != null)
            {
                NE_InputManager.Instance.OnEndTouch += HandleEndTouch;
            }

            if (NE_GameManager.Instance != null)
            {
                NE_GameManager.Instance.OnCooldownFinished += _cooldownFinishedHandler;
            }
        }

        private void OnDisable()
        {
            if (NE_InputManager.Instance != null)
            {
                NE_InputManager.Instance.OnEndTouch -= HandleEndTouch;
            }

            if (NE_GameManager.Instance != null)
            {
                NE_GameManager.Instance.OnCooldownFinished -= _cooldownFinishedHandler;
            }
        }

        private void Update()
        {
            if (_animator == null) return;

            float targetBlend = _isMoving ? 1f : 0f;

            _currentBlend = Mathf.MoveTowards(
                _currentBlend,
                targetBlend,
                _blendChangeSpeed * Time.deltaTime
            );

            _animator.SetFloat(_blendParam, _currentBlend);
        }

        private void HandleEndTouch(Vector2 startPos, Vector2 endPos, float time)
        {
            if (NE_GameManager.Instance.GetState() != GameState.Play) return;
            if (_getCaught) return;
            if (_isMoving) return;
            if (_reachedGoal) return;

            Vector2 swipeDelta = endPos - startPos;
            if (swipeDelta.magnitude < _swipeThreshold) return;

            Camera cam = Camera.main;
            Vector3 camForward = cam.transform.forward;
            Vector3 camRight = cam.transform.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = Vector3.zero;

            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                moveDir = swipeDelta.x > 0 ? camRight : -camRight;
            }
            else
            {
                moveDir = swipeDelta.y > 0 ? camForward : -camForward;
            }

            Vector3 currentPos = transform.position;
            int steps = 0;

            Vector3 nextPos = currentPos + (moveDir * stepsize);

            while (IsWalkable(nextPos))
            {
                steps++;
                nextPos = nextPos + (moveDir * stepsize);
            }

            if (steps == 0)
            {
                return;
            }

            StartCoroutine(MoveForward(moveDir, steps));
        }

        private bool IsWalkable(Vector3 pos)
        {
            Vector3 rayStart = pos + Vector3.up * 5f;
            bool hasGround = Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 10f, _groundLayer);

            if (!hasGround)
            {
                return false;
            }

            if (_obstacleLayer.value != 0)
            {
                bool hasObstacle = Physics.CheckBox(
                    pos + Vector3.up * 1f,
                    new Vector3(0.4f, 0.8f, 0.4f),
                    Quaternion.identity,
                    _obstacleLayer
                );

                if (hasObstacle)
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerator MoveForward(Vector3 dir, int steps)
        {
            _isMoving = true;

            if (_audioManager && PlayerMove)
            {
                _audioManager.PlaySfx(PlayerMove);
            }

            if (RunVFX != null)
            {
                if (!RunVFX.isPlaying)
                {
                    RunVFX.Play();
                }
                else
                {
                    RunVFX.Stop();
                    RunVFX.Play();
                }
            }

            Quaternion targetRotation = Quaternion.LookRotation(dir);
            float rotationSpeed = 10f;

            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + (2f * steps * dir);

            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / _moveSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t);

                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

                Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);
                Vector3 movement = newPos - transform.position;
                _characterController.Move(movement);

                yield return null;
            }

            transform.rotation = targetRotation;
            Vector3 finalMovement = targetPos - transform.position;
            _characterController.Move(finalMovement);

            Vector3 pos = transform.position;
            transform.position = new Vector3(
                Mathf.Round(pos.x),
                transform.position.y,
                Mathf.Round(pos.z)
            );

            if (RunVFX != null)
            {
                RunVFX.Stop();
            }

            _isMoving = false;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(NE_SafetyKey.KEY_TAG_DOG))
            {
                HandleDogCollision(other);
            }
            else if (other.gameObject.CompareTag(NE_SafetyKey.KEY_TAG_TRAP))
            {
                HandleTrapCollision(other);
            }
        }

        private void HandleDogCollision(Collider other)
        {
            if (other != null) other.GetComponent<NE_AEnemy>()?.CaughtPlayer();
            transform.position = new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z);
            _animator.SetTrigger(_dogCatchTrigger);
            GetCaught(false);
        }

        private void HandleTrapCollision(Collider other)
        {
            if (other != null) other.GetComponent<NE_AEnemy>()?.CaughtPlayer();
            transform.position = new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z);
            _animator.SetTrigger(_shockTrigger);
            GetCaught(false);

            if (shockVFX != null)
            {
                shockVFX.Play();
                StartCoroutine(TurnOffShockVFX());
            }
        }

        IEnumerator TurnOffShockVFX()
        {
            yield return new WaitForSeconds(3f);
            shockVFX.Stop();
        }

        public void GetCaught(bool playAnimation)
        {
            RotateToCamera();
            _getCaught = true;
            if (playAnimation) _animator.SetTrigger(_getCatchTrigger);

            if (_isMoving)
            {
                StopAllCoroutines();

                if (RunVFX != null)
                {
                    RunVFX.Stop();
                }

                _isMoving = false;
                _currentBlend = 0f;
            }
        }

        public void RotateToCamera()
        {
            CinemachineCamera cam = NE_CameraManager.Instance.GetLoseGameCamera();
            NE_CameraManager.Instance.BlendToLoseCamera(0.1f);
            if (cam == null) return;

            Vector3 toCam = (cam.transform.position - transform.position).normalized;
            toCam.y = 0f;
            toCam = toCam.normalized;

            if (toCam.sqrMagnitude < 0.0001f) return;

            float angle = Vector3.SignedAngle(Vector3.forward, toCam, Vector3.up);

            float roundedAngle = Mathf.Round(angle / 90f) * 90f;

            transform.rotation = Quaternion.Euler(0, roundedAngle, 0);
        }

        private void OnDrawGizmos()
        {
            Vector3[] directions = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
            Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow };

            for (int i = 0; i < directions.Length; i++)
            {
                Vector3 nextPos = transform.position + directions[i] * stepsize;
                Vector3 rayStart = nextPos + Vector3.up * 5f;

                Gizmos.color = Color.white;
                Gizmos.DrawLine(rayStart, rayStart + Vector3.down * 10f);

                if (IsWalkable(nextPos))
                {
                    Gizmos.color = colors[i];
                    Gizmos.DrawSphere(nextPos, 0.15f);
                }
                else
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawSphere(nextPos, 0.1f);
                }
            }
        }

        internal void ReachGoal()
        {
            StartCoroutine(HandleReachGoal());
        }

        private IEnumerator HandleReachGoal()
        {
            while (_isMoving)
            {
                yield return null;
            }

            _reachedGoal = true;
            RunVFX.Stop();
            RunVFX.Clear();
            RunVFX.gameObject.SetActive(false);
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + 180f, 0);
            _animator.SetTrigger(_reachGoalTrigger);
        }
    }
}
