using System.Collections;
using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_Player : MonoBehaviour
    {
        [Header("Swipe Settings")]
        [SerializeField] private float minSwipeDistanceWorld = 0.35f;
        [SerializeField] private float axisBias = 0.02f;

        [SerializeField] private AudioClip move;
        [SerializeField] private AudioClip cantMove;

        [Header("VFX")]
        [SerializeField] private GameObject attackEffectPrefab;

        bool _swiping;
        Vector2 _swipeStartXZ;
        bJakGZQ3_InputSystem _inputSystem;
        bJakGZQ3_GridMovement _grid;
        bJakGZQ3_LevelManager _LevelManager;
        bJakGZQ3_AudioManager _AudioManager;
        bJakGZQ3_Oxygen _Oxygen;
        public bJakGZQ3_Oxygen OxyGen => _Oxygen;
        bJakGZQ3_CharacterStateMachine _characterStateMachine;

        private int _maxGun = 3;
        private int _currentGun = 0;

        void Awake()
        {
            _inputSystem = bJakGZQ3_InputSystem.Instance;
            _AudioManager = bJakGZQ3_AudioManager.Instance;
            _LevelManager = bJakGZQ3_LevelManager.Instance;
        }

        void Start()
        {
            _grid = GetComponent<bJakGZQ3_GridMovement>();
            _Oxygen = GetComponent<bJakGZQ3_Oxygen>();
            _characterStateMachine = GetComponent<bJakGZQ3_CharacterStateMachine>();

            _maxGun = 3;
            _currentGun = 0;
            isUpdated = false;
        }

        void OnEnable()
        {
            if (_inputSystem != null)
            {
                _inputSystem.OnStartTouch += HandleStartTouch;
                _inputSystem.OnEndTouch += HandleEndTouch;
            }

            if (_LevelManager != null)
            {
                bJakGZQ3_LevelManager.OnEnemyMoveFinish += HandleEnemyMoveFinish;
            }
        }

        void OnDisable()
        {
            if (_inputSystem != null)
            {
                _inputSystem.OnStartTouch -= HandleStartTouch;
                _inputSystem.OnEndTouch -= HandleEndTouch;
            }

            if (_LevelManager != null)
            {
                bJakGZQ3_LevelManager.OnEnemyMoveFinish -= HandleEnemyMoveFinish;
            }
        }

        bool isUpdated = false;

        void Update()
        {
            if (isUpdated) return;
            if (OxyGen.IsAlive == false)
            {
                isUpdated = true;
                _characterStateMachine.SwitchState(EntityState.Dead);
            }
        }

        void HandleStartTouch(Vector2 worldXZ, float time)
        {
            if (_characterStateMachine.State != EntityState.Idle) return;
            if (_grid.IsMoving) return;
            _swiping = true;
            _swipeStartXZ = worldXZ;
        }

        void HandleEndTouch(Vector2 worldXZ, float time)
        {
            if (_characterStateMachine.State != EntityState.Idle) return;
            if (!_swiping) return;
            _swiping = false;

            Vector2 deltaXZ = worldXZ - _swipeStartXZ;

            if (!_grid.TryGetDirection(deltaXZ, minSwipeDistanceWorld, axisBias, out var dir))
                return;

            _grid.Move(
                dir,
                OnMove: () =>
                {
                    if (_AudioManager && move) _AudioManager.PlaySfx(move);
                    _characterStateMachine.SwitchState(EntityState.Move);
                },
                OnMoveSucess: () =>
                {
                    if (_LevelManager != null)
                    {
                        _LevelManager.AddStep();
                        _grid.DisableMovement();
                        _LevelManager.NotifyPlayerMoveStart();
                    }
                },
                OnMoveNotSucess: () =>
                {
                    if (_AudioManager && cantMove) _AudioManager.PlaySfx(cantMove);
                }
            );
        }

        private void HandleEnemyMoveFinish()
        {
            if (_characterStateMachine.State == EntityState.Idle ||
                _characterStateMachine.State == EntityState.Move)
            {
                _grid.EnableMovement();
            }
        }

        public int EquipGun()
        {
            if (_currentGun >= _maxGun) return _maxGun;
            _currentGun++;
            int amount = _currentGun;
            bJakGZQ3_DataManager.Instance?.OnPlayerGun(GetCurrentGun());
            return amount;
        }

        public int UseGun()
        {
            if (_currentGun <= 0) return 0;
            _currentGun--;
            int amount = _currentGun;
            bJakGZQ3_DataManager.Instance?.OnPlayerGun(GetCurrentGun());
            return amount;
        }

        public int GetCurrentGun() => _currentGun;

        public void PlayAttackEffect(Vector3 position)
        {
            if (attackEffectPrefab == null) return;
            GameObject go = Instantiate(attackEffectPrefab, position, Quaternion.identity);
            Destroy(go, 3f);
        }
    }
}

