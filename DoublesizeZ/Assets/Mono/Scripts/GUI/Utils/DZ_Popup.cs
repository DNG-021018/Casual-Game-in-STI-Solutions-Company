using DG.Tweening;
using TMPro;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_Popup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Anim")]
        [SerializeField] private float moveUp = 80f;
        [SerializeField] private float duration = 1f;

        public void Play(string message)
        {
            text.text = message;

            canvasGroup.alpha = 1f;

            RectTransform rect = transform as RectTransform;
            Vector2 startPos = rect.anchoredPosition;

            Sequence seq = DOTween.Sequence();
            seq.Join(rect.DOAnchorPosY(startPos.y + moveUp, duration))
               .Join(canvasGroup.DOFade(0f, duration))
               .OnComplete(() =>
               {
                   Destroy(gameObject);
               });
        }

        public void PlayTopNotification(string message, Color color)
        {
            text.text = message;
            text.color = color;

            RectTransform rect = transform as RectTransform;
            Vector2 basePos = rect.anchoredPosition;

            canvasGroup.alpha = 0f;
            rect.localScale = Vector3.zero;
            rect.anchoredPosition = basePos + Vector2.down * 20f;

            Sequence seq = DOTween.Sequence();

            seq.Append(canvasGroup.DOFade(1f, 0.2f))
               .Join(rect.DOScale(1.1f, 0.3f).SetEase(Ease.OutBack))
               .Join(rect.DOAnchorPos(basePos, 0.3f).SetEase(Ease.OutBack));

            seq.Append(rect.DOScale(1f, 0.15f).SetEase(Ease.InOutSine));

            seq.AppendInterval(0.8f);

            seq.Append(rect.DOAnchorPosY(basePos.y + 40f, 0.5f).SetEase(Ease.InQuad))
               .Join(canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InQuad))
               .OnComplete(() =>
               {
                   Destroy(gameObject);
               });
        }
    }
}