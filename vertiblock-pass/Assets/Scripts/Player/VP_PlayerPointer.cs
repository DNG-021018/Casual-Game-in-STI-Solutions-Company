using UnityEngine;
using UnityEngine.UI;

namespace VertiblockPass
{
    public class VP_PlayerPointer : MonoBehaviour
    {
        [Header("Position Settings")]
        [SerializeField] private float heightOffset = 1.2f;
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobSpeed = 4f;

        private Canvas canvas;
        private Image arrow;
        private Camera _cam;
        private bool _isOn;

        void Awake()
        {
            canvas = GetComponentInChildren<Canvas>();
            arrow = GetComponentInChildren<Image>();
            _cam = Camera.main;
            canvas.worldCamera = _cam;
            if (arrow != null) arrow.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (!_isOn || arrow == null || !arrow.gameObject.activeSelf)
                return;

            UpdateArrowPosition();
            ArrowViewFollowCamera();
        }

        public void ToogleArrow(bool isOpen)
        {
            if (arrow == null) return;

            _isOn = isOpen;

            if (!isOpen)
            {
                arrow.gameObject.SetActive(false);
                return;
            }

            arrow.gameObject.SetActive(true);

            UpdateArrowPosition();
            ArrowViewFollowCamera();
        }

        private void UpdateArrowPosition()
        {
            if (arrow == null) return;

            Vector3 basePos = transform.position + Vector3.up * heightOffset;

            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            basePos.y += bob;

            arrow.rectTransform.position = basePos;
        }

        private void ArrowViewFollowCamera()
        {
            if (arrow == null) return;

            if (_cam == null)
            {
                _cam = Camera.main;
                if (_cam == null) return;
            }

            Transform t = arrow.rectTransform;

            Vector3 camForward = _cam.transform.forward;
            Vector3 camUp = _cam.transform.up;

            t.rotation = Quaternion.LookRotation(camForward, camUp);
        }
    }
}
