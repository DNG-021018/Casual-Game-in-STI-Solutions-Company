using UnityEngine;

namespace NightEscape
{
    public class NE_Star : MonoBehaviour
    {
        [SerializeField] private GameObject starUncollect;
        [SerializeField] private GameObject starCollect;
        [SerializeField] private ParticleSystem collectVFX;

        [SerializeField] private AudioClip starCollectSound;
        private NE_AudioManager _audioManager => NE_AudioManager.Instance;

        private RectTransform _collectRect;
        private Vector2 _collectOriginPos;
        private Vector3 _collectOriginScale;

        void Awake()
        {
            if (starCollect != null)
            {
                _collectRect = starCollect.GetComponent<RectTransform>();
                if (_collectRect != null)
                {
                    _collectOriginPos = _collectRect.anchoredPosition;
                    _collectOriginScale = _collectRect.localScale;
                }
            }
        }

        public RectTransform CollectRect => _collectRect;
        public Vector2 CollectOriginPos => _collectOriginPos;
        public Vector3 CollectOriginScale => _collectOriginScale;

        public void ResetVisual()
        {
            if (starUncollect != null)
                starUncollect.SetActive(true);

            if (starCollect != null)
                starCollect.SetActive(false);

            if (collectVFX != null)
            {
                collectVFX.Stop();
                collectVFX.gameObject.SetActive(false);
            }

            if (_collectRect != null)
            {
                _collectRect.anchoredPosition = _collectOriginPos;
                _collectRect.localScale = Vector3.zero;
            }
        }

        public void PrepareForAnimation(Vector2 startPos)
        {
            if (_collectRect == null) return;

            if (starUncollect != null)
                starUncollect.SetActive(true);

            if (starCollect != null)
                starCollect.SetActive(true);

            _collectRect.anchoredPosition = startPos;
            _collectRect.localScale = Vector3.zero;
        }

        public void SetCollected(bool collected)
        {
            if (starUncollect != null)
                starUncollect.SetActive(!collected);

            if (starCollect != null)
                starCollect.SetActive(collected);

            if (collectVFX != null && collected)
            {
                collectVFX.gameObject.SetActive(true);
                collectVFX.Play();
            }

            if (collected && _audioManager != null && starCollectSound != null)
            {
                _audioManager.PlaySfx(starCollectSound);
            }
        }

        public void Hide()
        {
            if (starUncollect != null)
                starUncollect.SetActive(false);

            if (starCollect != null)
                starCollect.SetActive(false);

            if (collectVFX != null)
            {
                collectVFX.Stop();
                collectVFX.gameObject.SetActive(false);
            }
        }
    }
}
