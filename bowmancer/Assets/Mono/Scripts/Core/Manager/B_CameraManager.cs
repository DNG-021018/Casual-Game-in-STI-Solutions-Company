using Cinemachine;
using UnityEngine;

namespace Bowmancer
{
    public class B_CameraManager : Singleton<B_CameraManager>
    {
        [SerializeField] private CinemachineVirtualCamera _PlayerFollowCamera;

        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            if (_PlayerFollowCamera == null)
            {
                Debug.LogError("Player Follow Camera is not assigned in AS_CameraManager.");
                return;
            }
        }

        public void SetTarget(Transform target)
        {
            _PlayerFollowCamera.Follow = target;
            _PlayerFollowCamera.LookAt = target;
        }
    }
}
