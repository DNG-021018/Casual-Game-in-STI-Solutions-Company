using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_Star : MonoBehaviour
    {
        [SerializeField] GameObject winStar;
        [SerializeField] TextMeshProUGUI winStarRequire;

        [SerializeField] GameObject loseStar;
        [SerializeField] TextMeshProUGUI loseStarRequire;

        public RectTransform TargetAnchor => (RectTransform)transform;

        public bool TryGetWinStarVisual(out Sprite sprite, out RectTransform rect)
        {
            sprite = null; rect = null;
            if (!winStar) return false;

            var img = winStar.GetComponentInChildren<Image>(true);
            if (!img) return false;

            sprite = img.sprite;
            rect = (RectTransform)img.transform;
            return sprite != null;
        }

        public void OnText(int req)
        {
            if (winStarRequire) winStarRequire.text = req.ToString();
            if (loseStarRequire) loseStarRequire.text = req.ToString();
        }

        public void OnStarPop()
        {
            winStar.SetActive(true);
            loseStar.SetActive(false);

            var rt = (RectTransform)winStar.transform;
            rt.DOKill();
            rt.localScale = Vector3.one * 0.8f;
            rt.DOScale(1f, 0.18f).SetEase(Ease.OutBack, 2f).SetUpdate(true);
        }
    }
}
