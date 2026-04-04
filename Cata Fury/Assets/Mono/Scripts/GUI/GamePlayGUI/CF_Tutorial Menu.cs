using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CataFury
{
    public class CF_TutorialMenu : CF_UIPage
    {
        [Header("Cutoff Mask")]
        [SerializeField] private RectTransform cutoffMask;

        [Header("Animation")]
        [SerializeField] private Animator tutorialAnimator;
        [SerializeField] private string animationStateName = "Tutorial";

        [Header("Pulse Settings")]
        [SerializeField] private float pulseScaleUp = 1.15f;
        [SerializeField] private float pulseScaleDown = 0.92f;
        [SerializeField] private float pulseDuration = 0.35f;
        [SerializeField] private float pulseInterval = 0.3f;

        private Sequence _pulseSeq;
        private Vector3 _maskBaseScale;
        private bool _dismissed;

        public override void Init(CF_BaseUI parent)
        {
            base.Init(parent);
        }

        public override IEnumerator Show()
        {
            _dismissed = false;
            canvasGroup.alpha = 1f;
            yield return base.Show();

            // Reset animation về frame đầu mỗi lần Show
            if (tutorialAnimator != null)
            {
                tutorialAnimator.Play(animationStateName, 0, 0f);
                tutorialAnimator.Update(0f);
            }

            if (cutoffMask != null)
            {
                _maskBaseScale = cutoffMask.localScale;
                StartPulse();
            }
        }

        public override IEnumerator Hide()
        {
            StopPulse();

            if (cutoffMask != null)
                cutoffMask.localScale = _maskBaseScale;

            canvasGroup.alpha = 0f;
            yield return base.Hide();
        }

        private void Update()
        {
            if (_dismissed || !IsShown) return;

            bool tapped = false;

            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                tapped = true;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                tapped = true;

            if (tapped) Dismiss();
        }

        private void Dismiss()
        {
            _dismissed = true;
            CF_GameManager.Instance.TutorialComplete();
        }

        private void StartPulse()
        {
            StopPulse();

            _pulseSeq = DOTween.Sequence()
                .SetUpdate(true)
                .SetLink(gameObject)
                .SetLoops(-1);

            _pulseSeq.Append(
                cutoffMask.DOScale(_maskBaseScale * pulseScaleUp, pulseDuration)
                    .SetEase(Ease.OutSine));
            _pulseSeq.Append(
                cutoffMask.DOScale(_maskBaseScale * pulseScaleDown, pulseDuration)
                    .SetEase(Ease.InOutSine));
            _pulseSeq.Append(
                cutoffMask.DOScale(_maskBaseScale, pulseDuration * 0.8f)
                    .SetEase(Ease.OutCubic));
            _pulseSeq.AppendInterval(pulseInterval);
        }

        private void StopPulse()
        {
            if (_pulseSeq != null && _pulseSeq.IsActive())
                _pulseSeq.Kill();
            _pulseSeq = null;
        }
    }
}