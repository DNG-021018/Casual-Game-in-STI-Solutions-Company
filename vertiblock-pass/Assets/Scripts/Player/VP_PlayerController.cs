using System.Collections;
using UnityEngine;

namespace VertiblockPass
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class VP_PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _rollSpeed = 5;
        [SerializeField] private float _swipeThreshold = 0.5f;
        [SerializeField] private float raycastLength = 2f;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private AudioClip PlayerMove1;
        [SerializeField] private AudioClip PlayerMove2;
        [SerializeField] private AudioClip PlayerFall;

        [Header("Fall Settings")]
        [SerializeField] private float fallPushForce = 2f;
        [SerializeField] private float fallTorqueForce = 2f;

        [Header("Child Transforms")]
        [SerializeField] private Transform child1;
        [SerializeField] private Transform child2;

        private Rigidbody _rb;
        private BoxCollider _col;
        private BoxCollider _childCol1;
        private BoxCollider _childCol2;
        public Transform Child1 => child1;
        public Transform Child2 => child2;

        private bool _isMoving;
        private bool _isFalling;
        public bool IsBusy => _isMoving || _isFalling;

        private Vector3 _lastMoveDir = Vector3.forward;
        private Vector3 _fallDirection = Vector3.zero;

        private VP_PlayerState _state;

        private VP_PlayerPointer _pointer;
        public VP_PlayerPointer Pointer => _pointer;

        private VP_AudioManager _audioManager;

        void Awake()
        {
            _audioManager = VP_AudioManager.Instance;
            _pointer = GetComponent<VP_PlayerPointer>();
        }

        void Start()
        {
            _state = GetComponent<VP_PlayerState>();
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<BoxCollider>();

            if (child1 != null)
            {
                _childCol1 = child1.GetComponent<BoxCollider>();
            }

            if (child2 != null)
            {
                _childCol2 = child2.GetComponent<BoxCollider>();
            }
        }

        private void OnEnable()
        {
            VP_InputManager.Instance.OnEndTouch += HandleEndTouch;
        }

        private void OnDisable()
        {
            VP_InputManager.Instance.OnEndTouch -= HandleEndTouch;
        }

        private void HandleEndTouch(Vector2 startPos, Vector2 endPos, float time)
        {
            if (VP_SplitManager.Instance != null && VP_SplitManager.Instance.IsSplit)
            {
                if (VP_SplitManager.Instance.ActiveCube != this) return;
            }

            if (_isMoving || _isFalling) return;

            Vector2 swipeDelta = endPos - startPos;

            if (swipeDelta.magnitude < _swipeThreshold) return;

            Vector3 moveDir;

            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                moveDir = swipeDelta.x > 0 ? Vector3.right : Vector3.left;
            }
            else
            {
                moveDir = swipeDelta.y > 0 ? Vector3.forward : Vector3.back;
            }

            _lastMoveDir = moveDir.normalized;

            Assemble(moveDir);
        }

        private void Assemble(Vector3 dir)
        {
            Vector3 anchor;
            Vector3 axis;

            _lastMoveDir = dir.normalized;

            if (_rb != null && _col != null)
            {
                float height = _col.bounds.size.y;
                float width = (Mathf.Abs(dir.x) > 0.5f) ? _col.bounds.size.x : _col.bounds.size.z;

                Vector3 anchorOffset = (Vector3.down * height + dir * width) * 0.5f;
                anchor = _rb.position + anchorOffset;
                axis = Vector3.Cross(Vector3.up, dir);
            }
            else
            {
                Vector3 fallbackAnchor = transform.position + (Vector3.down + dir) * 0.5f;
                Vector3 fallbackAxis = Vector3.Cross(Vector3.up, dir);
                anchor = fallbackAnchor;
                axis = fallbackAxis;
            }
            StartCoroutine(Roll(anchor, axis));
        }

        private IEnumerator Roll(Vector3 anchor, Vector3 axis)
        {
            _isMoving = true;

            if (_rb != null)
            {
                _rb.isKinematic = true;
                _rb.useGravity = false;
            }

            float angle = 0f;

            while (angle < 90f)
            {
                float step = _rollSpeed * 100f * Time.fixedDeltaTime;
                if (angle + step > 90f) step = 90f - angle;
                angle += step;

                Quaternion deltaRot = Quaternion.AngleAxis(step, axis);

                if (_rb != null)
                {
                    Vector3 offset = _rb.position - anchor;
                    offset = deltaRot * offset;
                    Vector3 newPos = anchor + offset;
                    Quaternion newRot = deltaRot * _rb.rotation;

                    _rb.MovePosition(newPos);
                    _rb.MoveRotation(newRot);
                }
                else
                {
                    transform.RotateAround(anchor, axis, step);
                }

                yield return new WaitForFixedUpdate();
            }

            if (_audioManager != null)
            {
                if (_state.IsStanding)
                {
                    _audioManager.PlaySfx(PlayerMove1);
                }
                else if (_state.IsLying)
                {
                    _audioManager.PlaySfx(PlayerMove2);
                }
            }

            RoundPosition();
            _state.UpdateState();
            CheckGround();
            if (VP_SplitManager.Instance != null && VP_SplitManager.Instance.IsSplit)
            {
                VP_SplitManager.Instance.TryMerge();
            }

            VP_LevelManager.Instance?.PlayerStepCount();
            _isMoving = false;
        }

        private void RoundPosition()
        {
            Vector3 pos = transform.position;
            const float snap = 2f;

            int ix = Mathf.RoundToInt(pos.x * snap);
            int iy = Mathf.RoundToInt(pos.y * snap);
            int iz = Mathf.RoundToInt(pos.z * snap);

            pos.x = ix * 0.5f;
            pos.y = iy * 0.5f;
            pos.z = iz * 0.5f;

            if (_rb != null)
            {
                _rb.position = pos;
            }
            else
            {
                transform.position = pos;
            }
        }

        private void CheckGround()
        {
            if (child1 == null || child2 == null || _childCol1 == null || _childCol2 == null)
                return;

            Vector3 rayStart1 = child1.position + Vector3.up * (_childCol1.size.y / 2f);
            Vector3 rayStart2 = child2.position + Vector3.up * (_childCol2.size.y / 2f);

            bool isGrounded1 = Physics.Raycast(rayStart1, Vector3.down, out RaycastHit hit1, raycastLength, _groundLayer);
            bool isGrounded2 = Physics.Raycast(rayStart2, Vector3.down, out RaycastHit hit2, raycastLength, _groundLayer);

            HandleTilesFromRaycasts(isGrounded1, hit1, isGrounded2, hit2);

            Vector3 dir1 = child1.position - transform.position;
            dir1.y = 0f;
            if (dir1.sqrMagnitude > 0.0001f) dir1.Normalize();

            Vector3 dir2 = child2.position - transform.position;
            dir2.y = 0f;
            if (dir2.sqrMagnitude > 0.0001f) dir2.Normalize();

            if (!isGrounded1 && isGrounded2)
            {
                _fallDirection = dir1;
                StartFall();
            }
            else if (isGrounded1 && !isGrounded2)
            {
                _fallDirection = dir2;
                StartFall();
            }
            else if (!isGrounded1 && !isGrounded2)
            {
                _fallDirection = _lastMoveDir;
                StartFall();
            }
        }

        private void HandleTilesFromRaycasts(bool isGrounded1, RaycastHit hit1, bool isGrounded2, RaycastHit hit2)
        {
            VP_TilesBase tile1 = null;
            VP_TilesBase tile2 = null;

            if (isGrounded1)
            {
                hit1.collider.TryGetComponent(out tile1);
            }

            if (isGrounded2)
            {
                hit2.collider.TryGetComponent(out tile2);
            }

            if (tile1 != null)
            {
                tile1.HandleCubeEnter(this, _state);
            }

            if (tile2 != null && tile2 != tile1)
            {
                tile2.HandleCubeEnter(this, _state);
            }
        }

        private void StartFall()
        {
            if (_rb == null) return;
            if (_isFalling) return;
            if (_pointer != null) _pointer.ToogleArrow(false);

            if (_audioManager != null)
            {
                _audioManager.PlaySfx(PlayerFall);
            }

            _isFalling = true;
            _isMoving = false;

            _rb.isKinematic = false;
            _rb.useGravity = true;

            Vector3 dir = _fallDirection.sqrMagnitude > 0.0001f ? _fallDirection.normalized : _lastMoveDir.normalized;

            Vector3 pushDir = (dir + Vector3.down).normalized;

            _rb.AddForce(pushDir * fallPushForce, ForceMode.Impulse);

            if (fallTorqueForce > 0f && dir != Vector3.zero)
            {
                Vector3 torqueAxis = Vector3.Cross(Vector3.up, dir);
                _rb.AddTorque(torqueAxis * fallTorqueForce, ForceMode.Impulse);
            }
            _col.isTrigger = true;

            StartCoroutine(FallCheck(2f));
        }

        IEnumerator FallCheck(float duration)
        {
            yield return new WaitForSeconds(duration);

            VP_GameManager.Instance.SetState(GameState.Lose);

            yield return new WaitForSeconds(1f);
            _isFalling = false;
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _col.isTrigger = false;
        }

        public void FallStraightDown()
        {
            if (!_state.IsStanding) return;

            if (_rb == null) return;
            if (_isFalling) return;

            if (_audioManager != null)
            {
                _audioManager.PlaySfx(PlayerFall);
            }

            _isFalling = true;
            _isMoving = false;

            _rb.isKinematic = false;
            _rb.useGravity = true;

            _rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

            _rb.AddForce(Vector3.down * fallPushForce, ForceMode.Impulse);

            _col.isTrigger = true;

            StartCoroutine(FallFreezeCheck(0.05f));
        }

        IEnumerator FallFreezeCheck(float duration)
        {
            yield return new WaitForSeconds(duration);

            if (_col == null) yield break;
            _col.isTrigger = false;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.gameObject.CompareTag("FinishLevel"))
            {
                if (_rb != null) _rb.constraints = RigidbodyConstraints.FreezeAll;
                VP_GameManager.Instance.SetState(GameState.Win);
                VP_LevelManager.Instance.FinishGame();
            }
        }
    }
}
