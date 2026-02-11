using System.Collections;
using UnityEngine;

namespace VertiblockPass
{
    [DefaultExecutionOrder(-90)]
    public class VP_CameraManager : MonoBehaviour
    {
        public static VP_CameraManager Instance { get; private set; }

        private Camera _camera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                _camera = GetComponentInChildren<Camera>();
            }
        }

        public void ShakeCamera(float duration, float magnitude)
        {
            StartCoroutine(ShakeEffect(duration, magnitude));
        }

        IEnumerator ShakeEffect(float duration, float magnitude)
        {
            Vector3 originalPos = _camera.transform.localPosition;

            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                _camera.transform.localPosition = new Vector3(x, originalPos.y, y);

                elapsed += Time.deltaTime;

                yield return null;
            }

            _camera.transform.localPosition = originalPos;
        }
    }
}
