using System.Collections;
using UnityEngine;

namespace CubeSokoban
{
    public class CS_PlayerVisual : MonoBehaviour
    {
        [Header("Visual Root")]
        [SerializeField] private Transform visualRoot;

        [Header("Jelly Tween")]
        [SerializeField] private float jellyDuration = 0.2f;
        [SerializeField] private float squashAmount = 0.25f;
        [SerializeField] private float stretchAmount = 0.15f;

        private Vector3 _defaultScale;
        private Coroutine _jellyRoutine;
        private Vector3 _moveDirection;

        void Awake()
        {
            if (visualRoot == null)
                visualRoot = transform;

            _defaultScale = visualRoot.localScale;
        }

        public void PlayJellyTween()
        {
            if (visualRoot == null) return;

            if (_jellyRoutine != null)
                StopCoroutine(_jellyRoutine);

            if (this.gameObject.activeInHierarchy) _jellyRoutine = StartCoroutine(JellyCoroutine());
        }

        public void SetMoveDirection(Vector3 direction)
        {
            _moveDirection = direction;
        }

        IEnumerator JellyCoroutine()
        {
            float t = 0f;
            float half = jellyDuration * 0.5f;

            while (t < half)
            {
                float n = t / half;
                float squash = Mathf.SmoothStep(0f, 1f, n);

                float y = 1f - squash * squashAmount;
                float xz = 1f + squash * stretchAmount;

                visualRoot.localScale = new Vector3(
                    _defaultScale.x * xz,
                    _defaultScale.y * y,
                    _defaultScale.z * xz
                );

                t += Time.deltaTime;
                yield return null;
            }

            if (_moveDirection != Vector3.zero)
            {
                RotateToDirection(_moveDirection);
            }

            t = 0f;
            while (t < half)
            {
                float n = t / half;
                float back = 1f - Mathf.SmoothStep(0f, 1f, n);

                float y = 1f - back * (squashAmount * 0.3f);
                float xz = 1f + back * (stretchAmount * 0.3f);

                visualRoot.localScale = new Vector3(
                    _defaultScale.x * xz,
                    _defaultScale.y * y,
                    _defaultScale.z * xz
                );

                t += Time.deltaTime;
                yield return null;
            }

            visualRoot.localScale = _defaultScale;
            _jellyRoutine = null;
        }

        private void RotateToDirection(Vector3 direction)
        {
            if (direction == Vector3.right)
                visualRoot.localRotation = Quaternion.Euler(0, 0, -90);
            else if (direction == Vector3.left)
                visualRoot.localRotation = Quaternion.Euler(0, 0, 90);
            else if (direction == Vector3.forward)
                visualRoot.localRotation = Quaternion.Euler(90, 0, 0);
            else if (direction == Vector3.back)
                visualRoot.localRotation = Quaternion.Euler(-90, 0, 0);
        }
    }
}
