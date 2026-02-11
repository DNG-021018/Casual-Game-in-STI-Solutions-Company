using DG.Tweening;
using UnityEngine;

namespace CubeSokoban
{
    public class CS_UIMainMenuRoot : CS_BaseUI
    {
        [SerializeField] RectTransform Logo;
        [SerializeField] RectTransform Chichken;
        [SerializeField] RectTransform Wolf;
        [SerializeField] RectTransform sunlight;
        [SerializeField, Range(10f, 360f)] float rotationSpeed = 90f;

        [Header("Intro Settings")]
        [SerializeField] float introDuration = 0.6f;
        [SerializeField] Ease introEase = Ease.OutBack;

        private Vector3 _logoDefaultScale;
        private Vector3 _chickenDefaultScale;
        private Vector3 _wolfDefaultScale;
        private Vector3 _sunlightDefaultScale;

        private Vector2 _logoDefaultPos;
        private Vector2 _chickenDefaultPos;
        private Vector2 _wolfDefaultPos;
        private Vector2 _sunlightDefaultPos;

        private Sequence _introSeq;

        void Start()
        {
            if (Logo != null)
            {
                _logoDefaultScale = Logo.localScale;
                _logoDefaultPos = Logo.anchoredPosition;
            }

            if (Chichken != null)
            {
                _chickenDefaultScale = Chichken.localScale;
                _chickenDefaultPos = Chichken.anchoredPosition;
            }

            if (Wolf != null)
            {
                _wolfDefaultScale = Wolf.localScale;
                _wolfDefaultPos = Wolf.anchoredPosition;
            }

            if (sunlight != null)
            {
                _sunlightDefaultScale = sunlight.localScale;
                _sunlightDefaultPos = sunlight.anchoredPosition;
            }

            PlayIntro();
        }

        protected override void HandleGameState(GameState s)
        {
            if (s == GameState.Initialize)
            {
                PlayIntro();
            }
            else
            {
                CloseAll();
            }
        }

        private void PlayIntro()
        {
            CloseAll();

            _introSeq?.Kill();
            StopSunlightRotation();

            Vector2 centerPos = _sunlightDefaultPos;

            if (Logo != null)
            {
                Logo.localScale = Vector3.zero;
                Logo.anchoredPosition = centerPos;
            }

            if (Chichken != null)
            {
                Chichken.localScale = Vector3.zero;
                Chichken.anchoredPosition = centerPos;
            }

            if (Wolf != null)
            {
                Wolf.localScale = Vector3.zero;
                Wolf.anchoredPosition = centerPos;
            }

            if (sunlight != null)
            {
                sunlight.localScale = Vector3.zero;
                sunlight.anchoredPosition = _sunlightDefaultPos;
                sunlight.localRotation = Quaternion.identity;
            }

            _introSeq = DOTween.Sequence();

            if (sunlight != null)
            {
                _introSeq.Join(
                    sunlight.DOScale(_sunlightDefaultScale, introDuration)
                            .SetEase(introEase)
                );
            }

            if (Logo != null)
            {
                _introSeq.Join(
                    Logo.DOScale(_logoDefaultScale, introDuration)
                        .SetEase(introEase)
                );
                _introSeq.Join(
                    Logo.DOAnchorPos(_logoDefaultPos, introDuration)
                        .SetEase(introEase)
                );
            }

            StartSunlightRotation();

            if (Chichken != null)
            {
                _introSeq.Join(
                    Chichken.DOScale(_chickenDefaultScale, introDuration)
                            .SetEase(introEase)
                );
                _introSeq.Join(
                    Chichken.DOAnchorPos(_chickenDefaultPos, introDuration)
                            .SetEase(introEase)
                );
            }

            if (Wolf != null)
            {
                _introSeq.Join(
                    Wolf.DOScale(_wolfDefaultScale, introDuration)
                        .SetEase(introEase)
                );
                _introSeq.Join(
                    Wolf.DOAnchorPos(_wolfDefaultPos, introDuration)
                        .SetEase(introEase)
                );
            }

            _introSeq
                .SetUpdate(true)
                .SetDelay(0.5f)
                .OnComplete(() =>
                {
                    Open(UIPageId.MainMenu, null, true);
                });
        }

        void StartSunlightRotation()
        {
            if (sunlight == null) return;

            sunlight.DOKill();
            float duration = 360f / rotationSpeed;

            sunlight
                .DORotate(new Vector3(0f, 0f, 360f), duration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(true);
        }

        void StopSunlightRotation()
        {
            if (sunlight != null)
                sunlight.DOKill();
        }

        void OnDestroy()
        {
            _introSeq?.Kill();
            StopSunlightRotation();
        }
    }
}
