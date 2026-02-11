using UnityEngine;
using DG.Tweening;

namespace HexaStack
{
    public class HS_TapToPlayTween : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private RectTransform target;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Scale Tween")]
        [SerializeField] private bool useScale = true;
        [SerializeField] private float minScale = 0.9f;
        [SerializeField] private float maxScale = 1.1f;
        [SerializeField] private float scaleDuration = 0.6f;
        [SerializeField] private Ease scaleEase = Ease.InOutSine;

        [Header("Fade Tween")]
        [SerializeField] private bool useFade = true;
        [SerializeField] private float minAlpha = 0.4f;
        [SerializeField] private float maxAlpha = 1f;
        [SerializeField] private float fadeDuration = 0.6f;

        [Header("Auto Play")]
        [SerializeField] private bool playOnEnable = true;

        private Sequence _sequence;
        private Vector3 _originalScale;

        private void Awake()
        {
            if (target == null)
                target = transform as RectTransform;

            if (target != null)
                _originalScale = target.localScale;

            if (useFade)
            {
                if (canvasGroup == null)
                    canvasGroup = GetComponent<CanvasGroup>();

                if (canvasGroup == null)
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                PlayLoop();
            }
        }

        private void OnDisable()
        {
            KillSequence(true);
        }

        public void PlayLoop()
        {
            KillSequence(false);

            if (target == null)
                return;

            target.localScale = _originalScale;

            if (canvasGroup != null)
                canvasGroup.alpha = maxAlpha;

            _sequence = DOTween.Sequence();
            _sequence.SetUpdate(true);
            _sequence.SetAutoKill(false);
            _sequence.SetLoops(-1, LoopType.Yoyo);

            if (useScale)
            {
                _sequence.Join(
                    target.DOScale(_originalScale * maxScale, scaleDuration)
                          .SetEase(scaleEase)
                );
            }

            if (useFade && canvasGroup != null)
            {
                _sequence.Join(
                    canvasGroup.DOFade(minAlpha, fadeDuration)
                               .SetEase(Ease.InOutSine)
                );
            }

            _sequence.Play();
        }

        public void StopLoop(bool reset = true)
        {
            KillSequence(reset);
        }

        private void KillSequence(bool reset)
        {
            if (_sequence != null)
            {
                _sequence.Kill();
                _sequence = null;
            }

            if (!reset || target == null)
                return;

            target.localScale = _originalScale;

            if (canvasGroup != null)
                canvasGroup.alpha = maxAlpha;
        }
    }
}
