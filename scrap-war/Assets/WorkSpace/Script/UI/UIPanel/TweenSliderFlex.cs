using UnityEngine;
using DG.Tweening;
using System;

public class TweenSliderFlex : MonoBehaviour
{
    [Header("Slide Settings")]
    public float slideDuration = 0.5f;
    public Ease slideEase = Ease.OutQuart;

    [Header("Positions")]
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private Vector2 endPosition;

    [Header("CanvasGroup Settings")]
    [SerializeField] private bool useCanvasGroup = true;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private bool setStartPositionOnAwake = true;

    // Events
    public event Action OnSlideToEndComplete;
    public event Action OnSlideToStartComplete;

    private RectTransform rectTransform;
    private bool isAtEnd = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (useCanvasGroup && canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    void Start()
    {
        if (setStartPositionOnAwake)
        {
            rectTransform.anchoredPosition = startPosition;
            isAtEnd = false;

            if (useCanvasGroup && canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
    [ContextMenu("Slide To End")]
    public void SlideToEnd()
    {
        if (isAtEnd) return;

        rectTransform.DOAnchorPos(endPosition, slideDuration)
            .SetEase(slideEase)
            .OnComplete(() =>
            {
                isAtEnd = true;
                if (useCanvasGroup && canvasGroup != null)
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }

                OnSlideToEndComplete?.Invoke();
            });
        if (useCanvasGroup && canvasGroup != null)
        {
            canvasGroup.DOFade(1f, slideDuration);
        }
    }

    [ContextMenu("Slide To Start")]
    public void SlideToStart()
    {
        if (!isAtEnd) return;

        if (useCanvasGroup && canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        rectTransform.DOAnchorPos(startPosition, slideDuration)
            .SetEase(slideEase)
            .OnComplete(() =>
            {
                isAtEnd = false;
                OnSlideToStartComplete?.Invoke();
            });
        if (useCanvasGroup && canvasGroup != null)
        {
            canvasGroup.DOFade(0f, slideDuration);
        }
    }
    [ContextMenu("Toggle Slide")]
    public void ToggleSlide()
    {
        if (isAtEnd)
            SlideToStart();
        else
            SlideToEnd();
    }

    public void ToggleSlide(bool slideToEnd)
    {
        if (slideToEnd)
            SlideToEnd();
        else
            SlideToStart();
    }

    private void OnDisable()
    {
        // Reset the position when the component is disabled
        rectTransform.anchoredPosition = startPosition;
        isAtEnd = false;

        if (useCanvasGroup && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}