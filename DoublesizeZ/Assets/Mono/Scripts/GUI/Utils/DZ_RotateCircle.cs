using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace DoublesideZ
{
    public class DZ_RotateCircle : MonoBehaviour
    {
        [SerializeField] private float rotateDuration = 2f;
        [SerializeField] private RotateMode rotateMode = RotateMode.FastBeyond360;
        [SerializeField] private Ease easeType = Ease.Linear;
        [SerializeField] private bool rotateOnEnable = true;

        private Image circleImage;
        private Tweener rotateTween;

        void Awake()
        {
            circleImage = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (rotateOnEnable)
            {
                Rotate();
            }
        }

        void Rotate()
        {
            if (rotateTween != null && rotateTween.IsActive())
            {
                rotateTween.Kill();
            }

            transform.rotation = Quaternion.identity;

            rotateTween = transform.DORotate(new Vector3(0, 0, -360f), rotateDuration, rotateMode)
                .SetEase(easeType)
                .SetLoops(-1, LoopType.Restart);
        }

        public void StopRotate()
        {
            if (rotateTween != null && rotateTween.IsActive())
            {
                rotateTween.Kill();
            }
        }

        public void PauseRotate()
        {
            if (rotateTween != null && rotateTween.IsActive())
            {
                rotateTween.Pause();
            }
        }

        public void ResumeRotate()
        {
            if (rotateTween != null && rotateTween.IsActive())
            {
                rotateTween.Play();
            }
        }

        void OnDisable()
        {
            StopRotate();
        }

        void OnDestroy()
        {
            StopRotate();
        }
    }
}