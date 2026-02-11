using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

namespace NightEscape
{
    [DefaultExecutionOrder(-90)]
    public class NE_CameraManager : MonoBehaviour
    {
        public static NE_CameraManager Instance { get; private set; }

        [SerializeField] private CinemachineCamera _gameplayCamera;
        [SerializeField] private CinemachineCamera _winGameCamera;
        [SerializeField] private CinemachineCamera _loseGameCamera;

        [SerializeField] private float _blendDuration = 1f;

        private Coroutine _blendCoroutine;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (_gameplayCamera != null)
            {
                // _gameplayCamera.enabled = true;
                _gameplayCamera.Priority = 100;
            }
            else
            {
                Debug.LogError("[NE_CameraManager] Gameplay camera not assigned!");
            }

            if (_winGameCamera != null)
            {
                // _winGameCamera.enabled = false;
                _winGameCamera.Priority = 0;
            }
            else
            {
                Debug.LogError("[NE_CameraManager] Win game camera not assigned!");
            }

            if (_loseGameCamera != null)
            {
                // _loseGameCamera.enabled = false;
                _loseGameCamera.Priority = 0;
            }
            else
            {
                Debug.LogError("[NE_CameraManager] Lose game camera not assigned!");
            }
        }

        public void SetTarget(Transform target, CinemachineCamera camera)
        {
            if (camera != null && target != null)
            {
                camera.Target.TrackingTarget = target;
            }
        }

        public void BlendToWinCamera(float duration = -1f)
        {
            BlendCinemachineCamera(_gameplayCamera, _winGameCamera, duration);
        }

        public void BlendToLoseCamera(float duration = -1f)
        {
            BlendCinemachineCamera(_gameplayCamera, _loseGameCamera, duration);
        }

        // public void BlendToGameplayCamera(float duration = -1f)
        // {
        //     BlendCinemachineCamera(_loseGameCamera, _gameplayCamera, duration);
        // }

        private void BlendCinemachineCamera(CinemachineCamera fromCamera, CinemachineCamera toCamera, float duration = -1f)
        {
            if (fromCamera == null || toCamera == null)
            {
                Debug.LogError("[NE_CameraManager] Camera không được null!");
                return;
            }

            if (_blendCoroutine != null)
            {
                StopCoroutine(_blendCoroutine);
            }

            float blendTime = duration > 0 ? duration : _blendDuration;
            _blendCoroutine = StartCoroutine(BlendCinemachineCameraCoroutine(fromCamera, toCamera, blendTime));
        }

        private IEnumerator BlendCinemachineCameraCoroutine(
            CinemachineCamera fromCamera,
            CinemachineCamera toCamera,
            float duration)
        {
            float elapsed = 0f;

            toCamera.enabled = true;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                fromCamera.Priority = Mathf.RoundToInt(Mathf.Lerp(100f, 0f, t));
                toCamera.Priority = Mathf.RoundToInt(Mathf.Lerp(0f, 100f, t));

                yield return null;
            }

            fromCamera.enabled = false;
            toCamera.enabled = true;
            fromCamera.Priority = 0;
            toCamera.Priority = 100;
        }

        // public CinemachineCamera GetGameplayCamera() => _gameplayCamera;
        public CinemachineCamera GetWinGameCamera() => _winGameCamera;
        public CinemachineCamera GetLoseGameCamera() => _loseGameCamera;
    }
}
