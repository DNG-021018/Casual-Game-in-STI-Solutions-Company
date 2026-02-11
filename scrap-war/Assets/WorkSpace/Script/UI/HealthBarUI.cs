using UnityEngine;
using DG.Tweening;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private float tweenDuration = 0.3f;

    public void SetHealth(float current, float max)
    {
        if (healthBar != null && max > 0)
        {
            float ratio = Mathf.Clamp01(current / max);
            healthBar.DOScaleX(ratio, tweenDuration).SetEase(Ease.OutQuad);
        }
    }
}
