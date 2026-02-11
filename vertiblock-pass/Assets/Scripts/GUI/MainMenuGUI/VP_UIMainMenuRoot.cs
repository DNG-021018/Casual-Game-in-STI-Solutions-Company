using DG.Tweening;
using UnityEngine;

namespace VertiblockPass
{
    public class VP_UIMainMenuRoot : VP_BaseUI
    {
        [SerializeField] RectTransform Logo;

        [SerializeField, Range(10f, 360f)] float rotationSpeed = 90f;

        [Header("Intro Settings")]
        [SerializeField] float introDuration = 0.6f;
        [SerializeField] Ease introEase = Ease.OutBack;

        private Vector3 _logoDefaultScale;

        private Vector2 _logoDefaultPos;
        private Vector2 _sunlightDefaultPos;

        private Sequence _introSeq;

        void Start()
        {
            if (Logo != null)
            {
                _logoDefaultScale = Logo.localScale;
                _logoDefaultPos = Logo.anchoredPosition;
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

            Vector2 centerPos = _sunlightDefaultPos;

            if (Logo != null)
            {
                Logo.localScale = Vector3.zero;
                Logo.anchoredPosition = centerPos;
            }
            _introSeq = DOTween.Sequence();


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

            _introSeq
                .SetUpdate(true)
                .SetDelay(0.5f)
                .OnComplete(() =>
                {
                    Open(UIPageId.MainMenu, null, true);
                });
        }

        void OnDestroy()
        {
            _introSeq?.Kill();
        }
    }
}
