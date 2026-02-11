using UnityEngine;
using DG.Tweening;

namespace Wja8YNiR_PrismPath
{
    public enum direction
    {
        dirX, dirY, dirZ
    }

    public class Wja8YNiR_Obstacle : Wja8YNiR_Entities
    {
        [Header("Tween Config")]
        [SerializeField] float amplitude = 10f;
        [SerializeField] float duration = 1.2f;
        [SerializeField] direction _moveDirection = direction.dirX;
        [SerializeField] Ease ease = Ease.InOutSine;
        [SerializeField] bool playOnEnable = true;

        Vector3 _startLocalPos;
        Tween _tween;

        void Awake()
        {
            _startLocalPos = transform.position;
        }

        void OnEnable()
        {
            if (playOnEnable) Play();
        }

        void OnDisable()
        {
            _tween?.Kill();
        }

        public void Play()
        {
            _tween?.Kill();
            transform.localPosition = _startLocalPos;

            Vector3 target = _startLocalPos;
            switch (_moveDirection)
            {
                case direction.dirX:
                    target += new Vector3(amplitude, 0f, 0f);
                    break;
                case direction.dirY:
                    target += new Vector3(0f, amplitude, 0f);
                    break;
                case direction.dirZ:
                    target += new Vector3(0f, 0f, amplitude);
                    break;
            }

            _tween = transform
                .DOLocalMove(target, duration)
                .SetEase(ease)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        public void Stop()
        {
            _tween?.Kill();
            transform.position = _startLocalPos;
        }
    }
}
