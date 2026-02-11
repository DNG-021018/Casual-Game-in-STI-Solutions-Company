using Unity.Cinemachine;
using UnityEngine;

namespace CB_CubeRunner
{
    [DefaultExecutionOrder(-50)]
    public class CB_CameraManager : MonoBehaviour
    {
        public static CB_CameraManager Instance { get; private set; }

        [SerializeField] CinemachineCamera CM;

        private Vector3 _startPos;
        private Quaternion _startRot;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            if (CM == null)
            {
                CM = GetComponentInChildren<CinemachineCamera>();
            }

            if (CM != null)
            {
                _startPos = CM.transform.position;
                _startRot = CM.transform.rotation;
            }
        }

        public void DisableTarget()
        {
            if (CM == null) return;
            CM.Target.TrackingTarget = null;
        }

        public void SetTarget(CR_PlayerController target)
        {
            if (CM == null || target == null) return;
            CM.Target.TrackingTarget = target.Target;
        }

        public void ResetCamera(CR_PlayerController target)
        {
            if (CM == null) return;

            CM.transform.position = _startPos;
            CM.transform.rotation = _startRot;

            if (target != null)
            {
                CM.Target.TrackingTarget = target.Target;
            }
            else
            {
                CM.Target.TrackingTarget = null;
            }
        }
    }
}