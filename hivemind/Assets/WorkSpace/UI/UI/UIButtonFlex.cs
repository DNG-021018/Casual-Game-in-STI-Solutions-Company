using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonFlex : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Tween Settings")]
    [SerializeField] private float scaleAmount = 0.9f;
    [SerializeField] private float tweenDuration = 0.1f;
    [SerializeField] private Ease tweenEase = Ease.OutQuad;
    
    [Header("Button Settings")]
    [SerializeField] private bool enableHapticFeedback = true;
    
    private Vector3 originalScale;
    private Button button;
    private Tween currentTween;
    
    void Start()
    {
        // Lưu scale gốc
        originalScale = transform.localScale;
        
        // Lấy component Button nếu có
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Kiểm tra nếu button bị disable
        if (button != null && !button.interactable) return;
        
        // Dừng tween hiện tại nếu có
        currentTween?.Kill();
        
        // Tween scale xuống khi nhấn
        currentTween = transform.DOScale(originalScale * scaleAmount, tweenDuration)
            .SetEase(tweenEase);
        
        // Haptic feedback cho mobile
        if (enableHapticFeedback && Application.isMobilePlatform)
        {
            Handheld.Vibrate();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Kiểm tra nếu button bị disable
        if (button != null && !button.interactable) return;
        
        // Dừng tween hiện tại nếu có
        currentTween?.Kill();
        
        // Tween scale về kích thước gốc khi thả
        currentTween = transform.DOScale(originalScale, tweenDuration)
            .SetEase(tweenEase);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Method này có thể được override để xử lý click event
        // Hoặc sử dụng UnityEvent trong Inspector
    }

    private void OnDestroy()
    {
        // Cleanup tween khi object bị destroy
        currentTween?.Kill();
    }

    // Method public để có thể gọi từ bên ngoài
    public void PlayClickAnimation()
    {
        currentTween?.Kill();
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(originalScale * scaleAmount, tweenDuration * 0.5f))
                .Append(transform.DOScale(originalScale, tweenDuration * 0.5f))
                .SetEase(tweenEase);
    }
    
    // Reset scale về ban đầu
    public void ResetScale()
    {
        currentTween?.Kill();
        transform.localScale = originalScale;
    }
}