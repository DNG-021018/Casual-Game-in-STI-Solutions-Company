using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NightEscape
{
    [RequireComponent(typeof(ScrollRect))]
    public class NE_UIHorizontalPager : MonoBehaviour
    {
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] bool canScroll;
        [SerializeField] float duration = 0.25f;
        [SerializeField] AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] bool useNormalized = true;

        public int Index { get; private set; }
        public int Count => scrollRect && scrollRect.content ? scrollRect.content.childCount : 0;

        Coroutine anim;
        RectTransform Content => scrollRect.content;

        void Reset()
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        void Awake()
        {
            if (!scrollRect) scrollRect = GetComponent<ScrollRect>();
            scrollRect.horizontal = canScroll;
            scrollRect.vertical = false;
        }

        public void JumpTo(int index, bool instant = false)
        {
            if (Count == 0) return;
            Index = Mathf.Clamp(index, 0, Count - 1);

            if (useNormalized)
            {
                float target = (Count <= 1) ? 0f : (float)Index / (Count - 1);
                TweenNorm(target, instant);
            }
            else
            {
                float targetX = CalcAnchoredX(Index);
                TweenAnchored(targetX, instant);
            }
        }

        public void Next() => JumpTo(Index + 1);
        public void Prev() => JumpTo(Index - 1);

        float CalcAnchoredX(int i)
        {
            Canvas.ForceUpdateCanvases();
            var vp = scrollRect.viewport != null ? scrollRect.viewport.rect : ((RectTransform)scrollRect.transform).rect;
            float vpW = vp.width;

            var c = Content;
            float total = Mathf.Max(0f, c.rect.width - vpW);

            var child = (RectTransform)c.GetChild(Mathf.Clamp(i, 0, Count - 1));

            var centerLocal = (Vector2)child.localPosition + new Vector2(child.rect.width * (0.5f - child.pivot.x), 0f);
            float desired = centerLocal.x - vpW * 0.5f;
            return Mathf.Clamp(desired, 0f, total);
        }

        void TweenNorm(float target, bool instant)
        {
            if (anim != null) StopCoroutine(anim);
            if (instant) { scrollRect.horizontalNormalizedPosition = target; return; }
            anim = StartCoroutine(CoNorm(target));
        }
        IEnumerator CoNorm(float target)
        {
            float t = 0f, start = scrollRect.horizontalNormalizedPosition;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = ease.Evaluate(Mathf.Clamp01(t / duration));
                scrollRect.horizontalNormalizedPosition = Mathf.LerpUnclamped(start, target, k);
                yield return null;
            }
            scrollRect.horizontalNormalizedPosition = target;
            anim = null;
        }

        void TweenAnchored(float targetX, bool instant)
        {
            if (anim != null) StopCoroutine(anim);
            if (instant) { var p = Content.anchoredPosition; p.x = targetX; Content.anchoredPosition = p; return; }
            anim = StartCoroutine(CoAnchored(targetX));
        }
        IEnumerator CoAnchored(float targetX)
        {
            float t = 0f, start = Content.anchoredPosition.x;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = ease.Evaluate(Mathf.Clamp01(t / duration));
                var p = Content.anchoredPosition;
                p.x = Mathf.LerpUnclamped(start, targetX, k);
                Content.anchoredPosition = p;
                yield return null;
            }
            var fin = Content.anchoredPosition; fin.x = targetX; Content.anchoredPosition = fin;
            anim = null;
        }
    }
}
