using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public enum EntityState
    {
        Idle,
        Move,
        Dead,
        Attack,
        Hit,
        Victory
    }

    public class bJakGZQ3_CharacterStateMachine : MonoBehaviour, bJakGZQ3_IStateMachine
    {
        [SerializeField] public AudioClip hitClip;
        [SerializeField] public AudioClip deadClip;

        private bJakGZQ3_Oxygen _oxygen;
        public bJakGZQ3_Oxygen Oxygen => _oxygen;

        private bJakGZQ3_GridMovement _gridMovement;
        public bJakGZQ3_GridMovement GridMovement => _gridMovement;

        private bJakGZQ3_ACharacterState currentState;
        public bJakGZQ3_IState CurrentState => currentState;

        private bJakGZQ3_Player _player;
        public bJakGZQ3_Player Player => _player;

        private bJakGZQ3_AudioManager _audioManager;

        EntityState _state;
        public EntityState State => _state;

        [Header("AFK Dancing Config")]
        [SerializeField] private float _afkDelay = 5f;
        public float afkDelay => _afkDelay;

        bJakGZQ3_CharacterIdleState idle = new();
        bJakGZQ3_CharacterMoveState move = new();
        bJakGZQ3_CharacterDeadState dead = new();
        bJakGZQ3_CharacterAttackState attack = new();
        bJakGZQ3_CharacterHitState hit = new();

        void Awake()
        {
            _gridMovement = GetComponent<bJakGZQ3_GridMovement>();
            _oxygen = GetComponent<bJakGZQ3_Oxygen>();
            _player = GetComponent<bJakGZQ3_Player>();
            _audioManager = bJakGZQ3_AudioManager.Instance;
            currentState = null;
        }

        void Start()
        {
            SwitchState(EntityState.Idle);
        }

        public void SwitchState(EntityState state)
        {
            _state = state;
            switch (state)
            {
                case EntityState.Idle:
                    ChangeState(idle);
                    break;
                case EntityState.Move:
                    ChangeState(move);
                    break;
                case EntityState.Dead:
                    _audioManager.PlaySfx(deadClip);
                    ChangeState(dead);
                    break;
                case EntityState.Attack:
                    ChangeState(attack);
                    break;
                case EntityState.Hit:
                    _audioManager.PlaySfx(hitClip);
                    ChangeState(hit);
                    break;
            }
        }

        public void ChangeState(bJakGZQ3_IState newState)
        {
            if (currentState != null && currentState == newState)
                return;

            if (currentState != null)
            {
                currentState.Exit(this);
            }

            currentState = (bJakGZQ3_ACharacterState)newState;
            currentState.Enter(this);
        }

        void Update()
        {
            if (currentState == null)
            {
                return;
            }

            currentState.OnUpdateState(this);
        }

        void OnTriggerEnter(Collider other)
        {
            currentState.OnTriggerEnter(this, other);
        }

        public void ResetState() { }
        public void UpdateState(float deltaTime) { }
    }
}
