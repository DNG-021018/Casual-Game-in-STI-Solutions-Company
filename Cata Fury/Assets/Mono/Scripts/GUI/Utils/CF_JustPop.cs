using DG.Tweening;
using UnityEngine;

namespace CataFury
{
    public class CF_JustPop : MonoBehaviour
    {
        [Header("Smooth Notify Settings")]
        [SerializeField] private float scaleUp = 1.12f;
        [SerializeField] private float scaleDown = 0.96f;
        [SerializeField] private float duration = 0.18f;
        [SerializeField] private float delayBetweenLoop = 0.8f;

        private Sequence _loopSeq;
        private Vector3 _baseScale;

        void Awake()
        {
            _baseScale = transform.localScale;
        }

        void OnEnable()
        {
            PlayLoop();
        }

        void OnDisable()
        {
            Stop();
            transform.localScale = _baseScale;
        }

        private void PlayLoop()
        {
            Stop();

            _loopSeq = DOTween.Sequence();

            _loopSeq.Append(
                transform.DOScale(_baseScale * scaleUp, duration)
                    .SetEase(Ease.OutSine)
            );

            _loopSeq.Append(
                transform.DOScale(_baseScale * scaleDown, duration)
                    .SetEase(Ease.InOutSine)
            );

            _loopSeq.Append(
                transform.DOScale(_baseScale, duration * 1.2f)
                    .SetEase(Ease.OutCubic)
            );

            _loopSeq.AppendInterval(delayBetweenLoop);

            _loopSeq.SetLoops(-1);
            _loopSeq.SetUpdate(true);
        }

        private void Stop()
        {
            if (_loopSeq != null && _loopSeq.IsActive())
            {
                _loopSeq.Kill();
                _loopSeq = null;
            }
        }
    }
}
