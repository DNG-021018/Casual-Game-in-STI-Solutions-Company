using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

namespace CataFury
{
    public class CF_CameraManager : MonoBehaviour
    {
        [Header("Virtual Cameras")]
        [SerializeField] private CinemachineVirtualCamera mainMenuCamera;
        [SerializeField] private CinemachineVirtualCamera gameplayCamera;

        public static event Action<GameState> OnTransitionComplete;

        private const int PRIORITY_ACTIVE = 20;
        private const int PRIORITY_INACTIVE = 0;

        private CinemachineBrain _brain;
        private Coroutine _blendCoroutine;

        void Awake()
        {
            _brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
        }

        void OnEnable() => CF_GameManager.OnGameStateChanged += HandleGameState;
        void OnDisable() => CF_GameManager.OnGameStateChanged -= HandleGameState;

        private void HandleGameState(GameState state)
        {
            switch (state)
            {
                case GameState.Ready:
                    SwitchAndNotify(mainMenuCamera, state);
                    break;

                case GameState.Tutorial:
                case GameState.Play:
                    SwitchAndNotify(gameplayCamera, state);
                    break;
            }
        }

        private void SwitchAndNotify(CinemachineVirtualCamera target, GameState state)
        {
            SetPriority(mainMenuCamera, PRIORITY_INACTIVE);
            SetPriority(gameplayCamera, PRIORITY_INACTIVE);
            SetPriority(target, PRIORITY_ACTIVE);

            if (_blendCoroutine != null) StopCoroutine(_blendCoroutine);
            _blendCoroutine = StartCoroutine(WaitForBlend(state));
        }

        private IEnumerator WaitForBlend(GameState state)
        {
            yield return null;

            if (_brain != null)
            {
                while (_brain.IsBlending)
                    yield return null;
            }

            OnTransitionComplete?.Invoke(state);
        }

        private void SetPriority(CinemachineVirtualCamera cam, int priority)
        {
            if (cam != null) cam.Priority = priority;
        }
    }
}