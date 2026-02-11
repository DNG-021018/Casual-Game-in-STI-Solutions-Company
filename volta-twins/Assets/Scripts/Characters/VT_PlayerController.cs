using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum PlayerType
{
    Blue,
    Red,
}

namespace VoltaTwins
{
    public class VT_PlayerController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] VT_PlayerConfig _playerConfig;
        public VT_PlayerConfig PlayerConfig => _playerConfig;

        [Header("Shoot pos")]
        [SerializeField] public Transform shootPos;

        [Header("Core Status")]
        [SerializeField] private bool hasCore = false;
        public bool HasCore => hasCore;

        [Header("Footstep SFX")]
        [SerializeField] private AudioClip footClip;
        [SerializeField] private AudioClip shootClip;

        private LineRenderer aimLineRenderer;

        private PlayerInputActions _input;
        public PlayerInputActions Input => _input;

        private VT_PlayerAnimationController _animController;
        public VT_PlayerAnimationController AnimController => _animController;

        private CharacterController _characterController;
        public CharacterController CharacterController => _characterController;

        private readonly List<VT_PlayerComponents> _components = new();

        [Space(8)][SerializeField] VT_PlayerMovement Movement;
        [Space(8)][SerializeField] VT_PlayerShoot Shoot;

        private VT_AudioManager _audioManager;
        public VT_AudioManager audioManager => _audioManager;

        void Awake()
        {
            InitializedPlayerComponents();
        }

        private void InitializedPlayerComponents()
        {
            _input = new PlayerInputActions();

            _audioManager = VT_AudioManager.Instance;

            _characterController = GetComponent<CharacterController>();
            _animController = GetComponent<VT_PlayerAnimationController>();

            if (_animController == null)
            {
                this.AddComponent<VT_PlayerAnimationController>();
                _animController = GetComponent<VT_PlayerAnimationController>();
            }

            aimLineRenderer = GetComponent<LineRenderer>();
            if (aimLineRenderer == null)
            {
                this.AddComponent<LineRenderer>();
                aimLineRenderer = GetComponent<LineRenderer>();
            }

            _components.Clear();
            _components.Add(Movement);
            _components.Add(Shoot);

            foreach (VT_PlayerComponents component in _components)
            {
                component.Initialized(this);
            }
        }

        void OnEnable()
        {
            _input.Enable();

            foreach (VT_PlayerComponents component in _components)
            {
                component.PlayerOnEnable();
            }
        }

        void OnDisable()
        {
            _input.Disable();
            foreach (VT_PlayerComponents component in _components)
            {
                component.PlayerOnDisable();
            }
        }

        void Start()
        {
            UpdateCoreStatus();

            foreach (VT_PlayerComponents component in _components)
            {
                component.PlayerStart();
            }
        }

        void Update()
        {
            if (!hasCore) return;

            foreach (VT_PlayerComponents component in _components)
            {
                component.PlayerUpdate();
            }
        }

        void FixedUpdate()
        {
            if (!hasCore) return;

            foreach (VT_PlayerComponents component in _components)
            {
                component.PlayerFixedUpdate();
            }
        }

        public void SetHasCore(bool value)
        {
            hasCore = value;
            UpdateCoreStatus();
        }

        private void UpdateCoreStatus()
        {
            if (_animController != null)
            {
                // _animController.SetDeadState(!hasCore);

                if (!hasCore)
                {
                    _animController.SetMoveSpeed(0);
                }
            }
        }

        public void SetBall(VT_EnergyCore ball)
        {
            Shoot.SetEnergyBall(ball);
        }

        public void ClearExternalVelocity()
        {
            Movement.ClearExternalVelocity();
        }

        public void SetExternalVelocity(Vector3 velocity)
        {
            Movement.SetExternalVelocity(velocity);
        }

        public void ShootEvent(AnimationEvent animationEvent)
        {
            Shoot.Shoot();
            audioManager.PlaySfx(shootClip);
        }

        public void FootL(AnimationEvent animationEvent)
        {
            audioManager.PlaySfx(footClip);
        }

        public void FootR(AnimationEvent animationEvent)
        {
            audioManager.PlaySfx(footClip);
        }
    }
}
