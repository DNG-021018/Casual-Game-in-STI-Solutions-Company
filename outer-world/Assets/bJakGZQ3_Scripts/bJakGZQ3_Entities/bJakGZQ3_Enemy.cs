using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_Enemy : MonoBehaviour
    {
        [SerializeField] bJakGZQ3_EnemyConfig _config;

        bJakGZQ3_GridMovement _GridMovement;
        bJakGZQ3_LevelManager _LevelManager;

        [Header("VFX")]
        [SerializeField] private GameObject attackEffectPrefab;
        [SerializeField] private GameObject deadEffectPrefab;
        bJakGZQ3_EnemyStateMachine _stateMachine;
        public bJakGZQ3_EnemyStateMachine StateMachine => _stateMachine;

        void Awake()
        {
            _GridMovement = GetComponent<bJakGZQ3_GridMovement>();
            _stateMachine = GetComponent<bJakGZQ3_EnemyStateMachine>();
        }

        void OnEnable()
        {
            _LevelManager = bJakGZQ3_LevelManager.Instance;
            if (_LevelManager != null)
            {
                _LevelManager.RegisterEnemy(this);
                _LevelManager.OnPlayerMoveStart += HandlePlayerMoveStart;
            }
        }

        void OnDisable()
        {
            if (_LevelManager != null)
            {
                _LevelManager.UnregisterEnemy(this);
                _LevelManager.OnPlayerMoveStart -= HandlePlayerMoveStart;
            }
        }

        void HandlePlayerMoveStart()
        {
            if (_GridMovement == null)
            {
                if (_LevelManager != null)
                    _LevelManager.NotifyEnemyMoveFinish();
                return;
            }

            if (_GridMovement.IsMoving)
            {
                if (_LevelManager != null)
                    _LevelManager.NotifyEnemyMoveFinish();
                return;
            }

            // build shuffled directions (same approach as GridMovement.TryGetRandomDirection)
            var dirs = new System.Collections.Generic.List<CellDirection>
            {
                CellDirection.UP,
                CellDirection.DOWN,
                CellDirection.LEFT,
                CellDirection.RIGHT
            };
            for (int i = 0; i < dirs.Count; i++)
            {
                int r = UnityEngine.Random.Range(i, dirs.Count);
                var tmp = dirs[i];
                dirs[i] = dirs[r];
                dirs[r] = tmp;
            }

            float checkRadius = 0.25f;
            bool movedOrHandled = false;

            foreach (var dir in dirs)
            {
                // check if next cell exists
                if (!_GridMovement.TryPeekNextCellCenter(dir, out var center))
                    continue;

                // detect colliders at target cell
                Collider[] hits = Physics.OverlapSphere(center, checkRadius);
                bool blockedByEnemy = false;
                bool foundPlayer = false;
                bJakGZQ3_Player foundPlayerComp = null;

                foreach (var h in hits)
                {
                    if (h == null) continue;
                    if (h.CompareTag("Enemy"))
                    {
                        blockedByEnemy = true;
                        break; // skip this direction
                    }
                    if (h.TryGetComponent<bJakGZQ3_Player>(out var p))
                    {
                        foundPlayer = true;
                        foundPlayerComp = p;
                        break;
                    }
                }

                if (blockedByEnemy)
                {
                    // try next direction
                    continue;
                }

                if (foundPlayer)
                {
                    // don't move into player cell — switch to attack or dead depending on player's gun
                    var sm = GetComponent<bJakGZQ3_EnemyStateMachine>();
                    if (sm != null)
                    {
                        // if (foundPlayerComp.GetCurrentGun() <= 0)
                        //     sm.SwitchState(EntityState.Attack);
                        // else
                            sm.SwitchState(EntityState.Dead);
                    }

                    if (_LevelManager != null)
                        _LevelManager.NotifyEnemyMoveFinish();

                    movedOrHandled = true;
                    break;
                }

                // target cell is free (no enemy, no player) -> move there
                _GridMovement.Move(dir,
                    OnMove: () => { },
                    OnMoveSucess: () =>
                    {
                        if (_LevelManager != null)
                        {
                            _LevelManager.NotifyEnemyMoveFinish();
                        }
                    },
                    OnMoveNotSucess: () =>
                    {
                        if (_LevelManager != null)
                        {
                            _LevelManager.NotifyEnemyMoveFinish();
                        }
                    }
                );

                movedOrHandled = true;
                break;
            }

            if (!movedOrHandled)
            {
                // no valid direction found
                if (_LevelManager != null)
                    _LevelManager.NotifyEnemyMoveFinish();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<bJakGZQ3_Player>(out var player))
            {
                if (_stateMachine != null)
                {
                    // if (player.GetCurrentGun() <= 0)
                    //     _stateMachine.SwitchState(EntityState.Attack);
                    // else
                        _stateMachine.SwitchState(EntityState.Dead);
                    return;
                }

                Destroy(this.gameObject);
            }
        }

        public float GetEnemyDamage() => _config != null ? _config.GetDamage() : 100f;

        public void PlayAttackEffect(Vector3 position)
        {
            if (attackEffectPrefab == null) return;
            GameObject go = Instantiate(attackEffectPrefab, position, Quaternion.identity);
            Destroy(go, 2f);
        }

        public void PlayDeadEffect(Vector3 position)
        {
            if (deadEffectPrefab == null) return;
            GameObject go = Instantiate(deadEffectPrefab, position, Quaternion.identity);
            Destroy(go, 3f);
        }

    }
}
