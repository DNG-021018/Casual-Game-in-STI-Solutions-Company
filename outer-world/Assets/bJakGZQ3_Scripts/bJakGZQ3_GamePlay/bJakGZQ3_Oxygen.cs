using System;
using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_Oxygen : MonoBehaviour
    {
        private float startOxygenSeconds = 60f;
        private bool drainOverTime = true;
        private float drainPerSecond = 1f;

        private float _oxyNow;
        private float _oxyMax;
        private bool _alive = true;
        public bool IsAlive => _alive;

        public event Action<float, float> OnOxygenChanged;

        bJakGZQ3_GameManager _GameManager;
        bJakGZQ3_GridMovement _GridMovement;

        void Awake()
        {
            _oxyMax = startOxygenSeconds;
            _oxyNow = _oxyMax;
        }

        void OnEnable()
        {
            BroadcastOxygen();
        }

        void Start()
        {
            _GameManager = bJakGZQ3_GameManager.Instance;
            _GridMovement = GetComponent<bJakGZQ3_GridMovement>();
        }

        void Update()
        {
            if (!_GameManager) return;
            if (!(_GameManager.GetState() == GameState.Play)) return;
            if (!_alive) return;
            if (!drainOverTime) return;

            _oxyNow -= drainPerSecond * Time.deltaTime;
            if (_oxyNow < 0f) _oxyNow = 0f;

            BroadcastOxygen();

            if (_oxyNow <= 0f)
            {
                HandleDeath();
            }
        }

        public void TakeOxygenDamage(float amount)
        {
            if (!_alive) return;
            if (amount <= 0f) return;

            _oxyNow -= amount;
            if (_oxyNow < 0f) _oxyNow = 0f;

            BroadcastOxygen();

            if (_oxyNow <= 0f)
            {
                HandleDeath();
            }
        }

        void BroadcastOxygen()
        {
            OnOxygenChanged?.Invoke(_oxyNow, _oxyMax);
        }

        void HandleDeath()
        {
            if (!_alive) return;
            bJakGZQ3_GameManager.Instance?.SetState(GameState.FinishGame);
            _alive = false;
            _oxyNow = 0f;
            BroadcastOxygen();
            if (_GridMovement) _GridMovement.DisableMovement();
        }

        public float CurrentOxygen => _oxyNow;
        public float MaxOxygen => _oxyMax;

        public void AddOxygen(float amount)
        {
            if (!_alive) return;
            if (amount <= 0f) return;

            _oxyNow += amount;
            if (_oxyNow > _oxyMax) _oxyNow = _oxyMax;

            BroadcastOxygen();
        }
    }
}
