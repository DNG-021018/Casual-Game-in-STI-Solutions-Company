using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_EnemyStateMachine : MonoBehaviour, bJakGZQ3_IStateMachine
    {
        [SerializeField] public AudioClip deadClip;

        private bJakGZQ3_GridMovement _gridMovement;
        public bJakGZQ3_GridMovement GridMovement => _gridMovement;

        private bJakGZQ3_Enemy _enemy;
        public bJakGZQ3_Enemy Enemy => _enemy;

        private bJakGZQ3_AEnemyState currentState;
        public bJakGZQ3_IState CurrentState => currentState;

        private bJakGZQ3_AudioManager _audioManager;

        EntityState _state;
        public EntityState State => _state;

        private bJakGZQ3_GameManager _gameManager;
        public bJakGZQ3_GameManager GameManager => _gameManager;

        bJakGZQ3_EnemyIdleState idle = new();
        bJakGZQ3_EnemyMoveState move = new();
        bJakGZQ3_EnemyDeadState dead = new();
        // bJakGZQ3_EnemyAttackState attack = new();
        bJakGZQ3_EnemyVictoryState win = new();

        void Awake()
        {
            _gameManager = bJakGZQ3_GameManager.Instance;
            _gridMovement = GetComponent<bJakGZQ3_GridMovement>();
            _enemy = GetComponent<bJakGZQ3_Enemy>();
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
                // case EntityState.Attack:
                //     ChangeState(attack);
                //     break;
                case EntityState.Victory:
                    ChangeState(win);
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

            currentState = (bJakGZQ3_AEnemyState)newState;
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
