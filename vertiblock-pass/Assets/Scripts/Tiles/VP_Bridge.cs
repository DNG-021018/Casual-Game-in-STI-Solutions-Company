using UnityEngine;
using DG.Tweening;

namespace VertiblockPass
{
    public class VP_Bridge : MonoBehaviour
    {
        [Header("Bridge Angles (Z only)")]
        [SerializeField] private float openZ = 0f;
        [SerializeField] private float closedZ = -180f;
        [SerializeField] private bool isOpenAtStart = false;

        [Header("Tween Settings")]
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private Ease easeType = Ease.OutCubic;

        private bool _isOpen;
        private Tween _tween;
        private Vector3 _baseEuler;

        void Start()
        {
            _baseEuler = transform.localEulerAngles;

            _isOpen = isOpenAtStart;
            ApplyBridgeRotationInstant(_isOpen);
        }

        public void ToggleBridge(bool value)
        {
            if (_isOpen == value) return;
            _isOpen = value;

            if (_tween != null && _tween.IsActive())
                _tween.Kill();

            float targetZ = _isOpen ? openZ : closedZ;
            Vector3 targetEuler = _baseEuler;
            targetEuler.z = targetZ;

            _tween = transform.DOLocalRotate(targetEuler, duration, RotateMode.Fast)
                .SetEase(easeType)
                .OnUpdate(() =>
                {
                    Vector3 euler = transform.localEulerAngles;
                    euler.x = _baseEuler.x;
                    euler.y = _baseEuler.y;
                    transform.localEulerAngles = euler;
                })
                .OnComplete(() => ApplyBridgeRotationInstant(_isOpen));
        }

        private void ApplyBridgeRotationInstant(bool isOpen)
        {
            Vector3 euler = _baseEuler;
            euler.z = isOpen ? openZ : closedZ;
            transform.localEulerAngles = euler;
        }
    }
}
