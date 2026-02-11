using System.Collections;
using UnityEngine;

namespace CB_CubeRunner
{
    public enum MoveDirections { LEFT, RIGHT }

    public class CR_PlayerMovement : MonoBehaviour
    {
        [Header("Move")]
        [SerializeField] private float _rollSpeed = 450f;

        [Header("Grid")]
        [SerializeField] private Vector3 checkCenterOffset = new(0f, 0f, 0f);

        [Header("Audio")]
        [SerializeField] private AudioClip[] stepSound;

        [Header("Fall Settings")]
        [SerializeField] private float fallDisableDelay = 5f;

        private CR_MapManager _map;
        private CR_PlayerController _controller;
        private CR_PlayerVisual _visual;
        private Rigidbody _rb;
        private CB_GameManager _GameManager;

        private bool _isMoving;
        private Vector2Int _gridPos;
        private Vector2Int _targetGridPos;

        private Coroutine _fallDisableRoutine;

        void Awake()
        {
            _GameManager = CB_GameManager.Instance;

            _rb = GetComponent<Rigidbody>();
            if (_rb != null) _rb.isKinematic = true;

            _controller = GetComponent<CR_PlayerController>();
            if (_controller != null)
                _visual = _controller.GetCurrentVisual();
        }

        void OnEnable()
        {
            if (_controller == null)
                _controller = GetComponent<CR_PlayerController>();

            if (CB_GameManager.Instance != null)
                CB_GameManager.Instance.OnSkinChanged += HandleSkinChanged;
        }

        void OnDisable()
        {
            if (CB_GameManager.Instance != null)
                CB_GameManager.Instance.OnSkinChanged -= HandleSkinChanged;
        }

        void HandleSkinChanged(int skinId)
        {
            StartCoroutine(GetNewSkin());
        }

        IEnumerator GetNewSkin()
        {
            yield return null;
            if (_controller == null)
                _controller = GetComponent<CR_PlayerController>();

            if (_controller != null)
                _visual = _controller.GetCurrentVisual();

            UpdateTileHighlight();
        }

        void Start()
        {
            _map = CR_MapManager.Instance;

            if (_map != null)
            {
                _gridPos = _map.WorldToGrid(transform.position + checkCenterOffset);
            }

            if (_controller == null)
                _controller = GetComponent<CR_PlayerController>();

            if (_controller != null)
                _visual = _controller.GetCurrentVisual();

            UpdateTileHighlight();
        }

        void Update()
        {
            if (!_isMoving && _map != null && _rb != null && _rb.isKinematic)
            {
                CheckGroundExists();
            }
        }

        void CheckGroundExists()
        {
            if (_map == null) return;

            if (!_map.HasFloorAtGrid(_gridPos))
            {
                StartFall();
            }
            else
            {
                if (_map.TryGetFloorAtGrid(_gridPos, out var currentTile))
                {
                    CheckIfTileIsDisabled(currentTile);
                }
            }
        }

        void CheckIfTileIsDisabled(CR_TileMap tile)
        {
            if (tile == null) return;

            var tilesParent = tile.GetComponentInParent<CR_Tiles>();
            if (tilesParent != null && tilesParent.IsTileDisabled(tile))
            {
                StartCoroutine(FallThroughDisabledTile(tile));
            }
        }

        IEnumerator FallThroughDisabledTile(CR_TileMap tile)
        {
            _isMoving = true;

            yield return new WaitForSeconds(0.1f);

            if (tile != null)
            {
                tile.gameObject.SetActive(false);
            }

            StartFall();
        }

        void StartFall()
        {
            if (_rb != null) _rb.isKinematic = false;

            if (CB_CameraManager.Instance != null)
                CB_CameraManager.Instance.DisableTarget();

            if (_GameManager != null)
                _GameManager.SetState(GameState.FinishGame);

            if (_map != null)
                _map.StopAutoDrop();

            _isMoving = true;

            if (_fallDisableRoutine != null)
                StopCoroutine(_fallDisableRoutine);
            _fallDisableRoutine = StartCoroutine(DisableAfterFall());
        }

        IEnumerator DisableAfterFall()
        {
            yield return new WaitForSeconds(fallDisableDelay);
            gameObject.SetActive(false);
        }

        public void Assemble(MoveDirections dir)
        {
            if (_isMoving || _map == null) return;

            Vector2Int step = GetStep(dir);
            _targetGridPos = _gridPos + step;

            if (_map.HasWallAtGrid(_targetGridPos))
            {
                if (_visual != null) _visual.PlayJellyTween();
                return;
            }

            Vector3 from = _map.GridToWorld(_gridPos);
            Vector3 to = _map.GridToWorld(_targetGridPos);
            Vector3 moveDir = (to - from);
            moveDir.y = 0f;
            moveDir.Normalize();

            Move(moveDir);
        }

        Vector2Int GetStep(MoveDirections dir) =>
            dir == MoveDirections.LEFT ? Vector2Int.down : Vector2Int.left;

        void Move(Vector3 dir)
        {
            Vector3 anchor = transform.position + (Vector3.down + dir) * (transform.localScale.x / 2f);
            Vector3 axis = Vector3.Cross(Vector3.up, dir);
            StartCoroutine(Roll(anchor, axis));
        }

        IEnumerator Roll(Vector3 anchor, Vector3 axis)
        {
            _isMoving = true;

            const float targetAngle = 90f;
            float rotated = 0f;
            float speed = Mathf.Max(_rollSpeed, 1f);

            while (rotated < targetAngle)
            {
                float step = speed * Time.deltaTime;
                if (rotated + step > targetAngle)
                    step = targetAngle - rotated;

                transform.RotateAround(anchor, axis, step);
                rotated += step;

                yield return null;
            }

            _gridPos = _targetGridPos;

            CheckGroundExists();

            if (_rb != null && !_rb.isKinematic)
            {
                yield break;
            }

            if (_GameManager != null && _GameManager.GetState() == GameState.Play)
            {
                _GameManager.AddPoint(1);
            }

            UpdateTileHighlight();

            if (_visual != null) _visual.PlayJellyTween();

            if (stepSound != null && stepSound.Length > 0)
            {
                int index = Random.Range(0, stepSound.Length);
                CB_AudioManager.Instance.PlaySfx(stepSound[index], 0.5f);
            }

            _isMoving = false;
        }

        void UpdateTileHighlight()
        {
            if (_map == null || _visual == null) return;

            if (_map.TryGetFloorAtGrid(_gridPos, out var tile) && tile != null)
            {
                _visual.HighlightTileUnderFoot(tile);
            }
        }

        public void ResetForNewRun()
        {
            StopAllCoroutines();
            _fallDisableRoutine = null;

            _isMoving = false;

            if (_rb != null)
            {
                _rb.isKinematic = true;
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }

            _map = CR_MapManager.Instance;
            if (_map != null)
            {
                _gridPos = _map.WorldToGrid(transform.position + checkCenterOffset);
            }

            UpdateTileHighlight();
        }
    }
}
