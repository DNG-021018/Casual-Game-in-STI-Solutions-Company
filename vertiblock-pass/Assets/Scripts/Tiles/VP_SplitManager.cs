using UnityEngine;

namespace VertiblockPass
{
    public class VP_SplitManager : MonoBehaviour
    {
        public static VP_SplitManager Instance { get; private set; }

        [SerializeField] private VP_PlayerController splitCubePrefab;
        [SerializeField] private float mergeShakeDuration;
        [SerializeField] private float mergeShakeMagnitude;

        private VP_PlayerController _mainCube;
        private VP_PlayerController _cubeA;
        private VP_PlayerController _cubeB;

        private VP_PlayerController _activeCube;
        private bool _isSplit;

        public bool IsSplit => _isSplit;
        public VP_PlayerController ActiveCube => _activeCube;

        VP_CameraManager _CameraManager;
        VP_InputManager _InputManager;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _InputManager = VP_InputManager.Instance;
        }

        void OnEnable()
        {
            if (_InputManager) _InputManager.OnDoubleTap += SwapControl;
        }

        void OnDisable()
        {
            if (_InputManager) _InputManager.OnDoubleTap -= SwapControl;
        }

        void Start()
        {
            _CameraManager = VP_CameraManager.Instance;
        }

        public void Split(VP_PlayerController mainCube, Vector3 posA, Vector3 posB)
        {
            if (_isSplit) return;
            _isSplit = true;

            _mainCube = mainCube;
            _mainCube.gameObject.SetActive(false);

            posA.y = 0.5f;
            posB.y = 0.5f;
            if (_CameraManager != null) _CameraManager.ShakeCamera(mergeShakeDuration, mergeShakeMagnitude);

            _cubeA = Instantiate(splitCubePrefab, posA, Quaternion.identity);
            _cubeB = Instantiate(splitCubePrefab, posB, Quaternion.identity);

            SetActiveCube(_cubeA);
        }

        private void SetActiveCube(VP_PlayerController newActive)
        {
            _activeCube = newActive;
            TogglePointer(_cubeA, false);
            TogglePointer(_cubeB, false);
            TogglePointer(_activeCube, true);
        }

        private void TogglePointer(VP_PlayerController cube, bool isOn)
        {
            if (cube == null) return;

            VP_PlayerPointer pointer = cube.Pointer;
            if (pointer != null)
            {
                pointer.ToogleArrow(isOn);
            }
        }

        public void SwapControl()
        {
            if (!_isSplit)
                return;

            if (_cubeA == null || _cubeB == null)
                return;

            if (_cubeA.IsBusy || _cubeB.IsBusy)
                return;

            var newActive = (_activeCube == _cubeA) ? _cubeB : _cubeA;
            SetActiveCube(newActive);
        }

        public void TryMerge()
        {
            if (!_isSplit)
            {
                return;
            }
            if (_cubeA == null || _cubeB == null)
            {
                return;
            }

            Vector3 posA = _cubeA.transform.position;
            Vector3 posB = _cubeB.transform.position;

            if (Mathf.Abs(posA.y - posB.y) > 0.1f)
            {
                return;
            }

            Vector3 diff = posB - posA;
            diff.y = 0f;
            float sqrDist = diff.sqrMagnitude;

            if (sqrDist < 0.5f || sqrDist > 1.5f)
            {
                return;
            }

            if (Mathf.Abs(diff.x) > 0.1f && Mathf.Abs(diff.z) > 0.1f)
            {
                return;
            }

            Transform childA1 = _cubeA.Child1;
            Transform childA2 = _cubeA.Child2;
            Transform childB1 = _cubeB.Child1;
            Transform childB2 = _cubeB.Child2;

            if (childA1 == null || childA2 == null || childB1 == null || childB2 == null)
            {
                return;
            }

            Vector3 avgChildPos = (childA1.position + childA2.position + childB1.position + childB2.position) * 0.25f;

            Quaternion targetRotation = Quaternion.identity;
            if (diff.sqrMagnitude > 0.0001f)
            {
                Vector3 longAxis = diff.normalized;
                targetRotation = Quaternion.LookRotation(longAxis, Vector3.up) * Quaternion.Euler(-90, 0, 0);
            }

            Destroy(_cubeA.gameObject);
            Destroy(_cubeB.gameObject);

            _mainCube.transform.rotation = targetRotation;

            Vector3 mergePos = avgChildPos;
            mergePos.y = posA.y;
            _mainCube.transform.position = mergePos;
            _mainCube.gameObject.SetActive(true);

            _isSplit = false;
            _activeCube = null;
            _cubeA = null;
            _cubeB = null;
        }
    }
}
