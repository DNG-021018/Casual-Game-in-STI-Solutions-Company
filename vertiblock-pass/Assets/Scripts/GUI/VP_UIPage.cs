using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace VertiblockPass
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class VP_UIPage : MonoBehaviour
    {
        [Header("Base")]
        [SerializeField] protected CanvasGroup canvasGroup;

        [System.Serializable]
        protected struct Panels
        {
            [Header("Panel")]
            public RectTransform panel;
            [Header("Tween Direction")]
            public SlideDir slideDir;
        }

        [Header("Tween")]
        public float duration = 0.5f;
        public Ease showEase = Ease.OutCubic;
        public Ease hideEase = Ease.InCubic;
        public float offscreenPadding = 64f;

        public bool IsShown { get; private set; }
        Sequence _pageTween;

        public virtual void Init(VP_BaseUI parent)
        {
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
            HideInstant();
        }

        public virtual IEnumerator Show(object ctx = null)
        {
            IsShown = true;
            gameObject.SetActive(true);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            yield break;
        }

        public virtual IEnumerator Hide()
        {
            IsShown = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            gameObject.SetActive(false);
            yield break;
        }

        public void HideInstant()
        {
            IsShown = false;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            gameObject.SetActive(false);
        }

        public virtual void ApplyContext(object ctx) { }

        #region Helpers

        protected void KillPageTween()
        {
            if (_pageTween != null && _pageTween.IsActive()) _pageTween.Kill();
            _pageTween = null;
        }

        protected virtual void CacheStartPositions() { }

        protected IEnumerator ShowMovePanels(
            float duration, Ease ease,
            float alphaFrom, float alphaTo,
            params (RectTransform rt, Vector2 from, Vector2 to)[] panels)
        {
            KillPageTween();

            gameObject.SetActive(true);
            canvasGroup.alpha = alphaFrom;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = true;

            _pageTween = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            foreach (var panel in panels)
            {
                if (panel.rt == null) continue;
                panel.rt.anchoredPosition = panel.from;
                _pageTween.Join(panel.rt.DOAnchorPos(panel.to, duration).SetEase(ease));
            }
            _pageTween.Join(canvasGroup.DOFade(alphaTo, duration).SetEase(ease));

            bool done = false;
            _pageTween.OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                done = true;
            });

            while (!done) yield return null;
        }

        protected IEnumerator HideMovePanels(
            float duration, Ease ease,
            float alphaFrom, float alphaTo,
            params (RectTransform rt, Vector2 from, Vector2 to)[] panels)
        {
            KillPageTween();

            canvasGroup.blocksRaycasts = false;

            canvasGroup.alpha = alphaFrom;

            _pageTween = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            foreach (var panel in panels)
            {
                if (panel.rt == null) continue;
                panel.rt.anchoredPosition = panel.from;
                _pageTween.Join(panel.rt.DOAnchorPos(panel.to, duration).SetEase(ease));
            }
            _pageTween.Join(canvasGroup.DOFade(alphaTo, duration).SetEase(ease));

            bool done = false;
            _pageTween.OnComplete(() =>
            {
                gameObject.SetActive(false);
                done = true;
            });

            while (!done) yield return null;
        }

        protected IEnumerator ShowScalePanels(
            float duration, Ease ease,
            float alphaFrom, float alphaTo,
            params (RectTransform rt, Vector3 from, Vector3 to)[] panels)
        {
            KillPageTween();

            gameObject.SetActive(true);
            canvasGroup.alpha = alphaFrom;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = true;

            _pageTween = DOTween.Sequence()
                .SetUpdate(true)
                .SetRecyclable(true)
                .SetLink(gameObject);

            foreach (var panel in panels)
            {
                if (panel.rt == null) continue;
                panel.rt.localScale = panel.from;
                _pageTween.Join(panel.rt.DOScale(panel.to, duration).SetEase(ease));
            }
            _pageTween.Join(canvasGroup.DOFade(alphaTo, duration).SetEase(ease));

            bool done = false;
            _pageTween.OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                done = true;
            });

            while (!done) yield return null;
        }

        protected IEnumerator HideScalePanels(
            float duration, Ease ease,
            float alphaFrom, float alphaTo,
            params (RectTransform rt, Vector3 from, Vector3 to)[] panels)
        {
            KillPageTween();

            canvasGroup.blocksRaycasts = false;
            // canvasGroup.interactable = false;
            canvasGroup.alpha = alphaFrom;

            _pageTween = DOTween.Sequence()
                .SetUpdate(true)
                .SetRecyclable(true)
                .SetLink(gameObject);

            foreach (var panel in panels)
            {
                if (panel.rt == null) continue;
                panel.rt.localScale = panel.from;
                _pageTween.Join(panel.rt.DOScale(panel.to, duration).SetEase(ease));
            }
            _pageTween.Join(canvasGroup.DOFade(alphaTo, duration).SetEase(ease));

            bool done = false;
            _pageTween.OnComplete(() =>
            {
                gameObject.SetActive(false);
                done = true;
            });

            while (!done) yield return null;
        }

        protected Vector2 GetOffscreenPos(RectTransform rt, SlideDir dir, Vector2 start, float offscreenPadding)
        {
            if (rt == null) return start;

            var parent = rt.parent as RectTransform;
            if (parent == null) return start;

            float px = (dir == SlideDir.Left || dir == SlideDir.Right)
                     ? (parent.rect.width + rt.rect.width)
                     : (parent.rect.height + rt.rect.height);

            px += Mathf.Max(0f, offscreenPadding);

            Vector2 delta = dir switch
            {
                SlideDir.Left => new Vector2(-px, 0f),
                SlideDir.Right => new Vector2(px, 0f),
                SlideDir.Up => new Vector2(0f, px),
                SlideDir.Down => new Vector2(0f, -px),
                SlideDir.None => Vector2.zero,
                _ => Vector2.zero
            };
            return start + delta;
        }
        #endregion
    }
}
