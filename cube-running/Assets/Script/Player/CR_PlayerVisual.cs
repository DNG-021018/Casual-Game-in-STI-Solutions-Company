using System.Collections;
using UnityEngine;

namespace CB_CubeRunner
{
    public class CR_PlayerVisual : MonoBehaviour
    {
        [Header("Visual Root")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Color baseColor;

        [Header("Jelly Tween")]
        [SerializeField] private float jellyDuration = 0.2f;
        [SerializeField] private float squashAmount = 0.25f;
        [SerializeField] private float stretchAmount = 0.15f;

        private Vector3 _defaultScale;
        private Coroutine _jellyRoutine;

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

        public void HighlightTileUnderFoot(CR_TileMap tile)
        {
            if (tile == null) return;
            tile.SetQuad(true, baseColor);
        }
    }
}
