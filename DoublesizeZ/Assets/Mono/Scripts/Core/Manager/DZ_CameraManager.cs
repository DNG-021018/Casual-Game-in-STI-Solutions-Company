using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_CameraManager : MonoBehaviour
    {
        [SerializeField] CinemachineVirtualCamera gameplayCamera;
        [SerializeField] CinemachineVirtualCamera menuCamera;
        [SerializeField] CinemachineVirtualCamera shopCamera;

        List<CinemachineVirtualCamera> _camerasList = new();

        private CinemachineBrain _cinemachineBrain;
        public event Action OnBlendComplete;

        void Awake()
        {
            _camerasList.Add(gameplayCamera);
            _camerasList.Add(menuCamera);
            _camerasList.Add(shopCamera);

            _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        }

        void Start()
        {
            SwitchToMenuCamera();
        }

        void LateUpdate()
        {
            if (_cinemachineBrain != null && _cinemachineBrain.IsBlending == false)
            {
                OnBlendComplete?.Invoke();
            }
        }

        public void SetTarget(Transform target)
        {
            foreach (var cam in _camerasList)
            {
                cam.Follow = target;
                cam.LookAt = target;
            }
        }

        public void SwitchToMenuCamera(Action OnAfterSwitch = null)
        {
            gameplayCamera.Priority = 0;
            menuCamera.Priority = 10;
            shopCamera.Priority = 0;

            StartCoroutine(WaitForBlendComplete(OnAfterSwitch));
            // OnAfterSwitch?.Invoke();
        }

        public void SwitchToGameplayCamera(Action OnAfterSwitch = null)
        {
            gameplayCamera.Priority = 10;
            menuCamera.Priority = 0;
            shopCamera.Priority = 0;

            // StartCoroutine(WaitForBlendComplete(OnAfterSwitch));
            OnAfterSwitch?.Invoke();
        }

        public void SwitchToShopCamera(Action OnAfterSwitch = null)
        {
            gameplayCamera.Priority = 0;
            menuCamera.Priority = 0;
            shopCamera.Priority = 10;

            // StartCoroutine(WaitForBlendComplete(OnAfterSwitch));
            OnAfterSwitch?.Invoke();
        }

        private IEnumerator WaitForBlendComplete(Action callback)
        {
            yield return null;

            float waitTimeout = 0.2f;
            float waited = 0f;
            while (!_cinemachineBrain.IsBlending && waited < waitTimeout)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            while (_cinemachineBrain.IsBlending)
            {
                yield return null;
            }

            callback?.Invoke();
        }
    }
}