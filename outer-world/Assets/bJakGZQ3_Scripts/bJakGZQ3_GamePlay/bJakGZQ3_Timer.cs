using System;
using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_Timer : MonoBehaviour
    {
        private float _timer = 0f;
        private bool _start = false;

        public event Action<float> OnTimerUpdated;

        bJakGZQ3_GameManager _GameManager;

        void Start()
        {
            _GameManager = bJakGZQ3_GameManager.Instance;
        }

        public void StartRecordTime()
        {
            _start = true;
            _timer = 0f;
            OnTimerUpdated?.Invoke(_timer);
        }

        void Update()
        {
            if (!_GameManager) return;
            if (_start == false) return;
            if (_GameManager.GetState() == GameState.FinishGame)
            {
                _start = false;
                OnTimerUpdated?.Invoke(_timer);
                return;
            }
            else if (_GameManager.GetState() == GameState.Play)
            {
                _timer += Time.deltaTime;
                OnTimerUpdated?.Invoke(_timer);
            }
        }

        public float FinalTime() => _timer;
    }
}
