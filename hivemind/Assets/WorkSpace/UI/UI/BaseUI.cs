using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using System.Collections.Generic;

public abstract class BaseUI : MonoBehaviour
{
    [Header("Base UI Settings")]
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected GameObject GraphicsHolder;
    [SerializeField] private List<TweenSliderFlex> tweenSliderFlexes;

    [SerializeField] protected float transitionDuration = 0.5f;
    [SerializeField] protected bool setActiveOnStart = false;

    protected virtual void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    protected virtual void Start()
    {
        SetUIActive(setActiveOnStart, false);
    }

    public virtual void SetUIActive(bool isActive, bool animate = true)
    {
        // Luôn SetActive(true) trước khi hiển thị để tránh delay
        if (isActive)
        {
            GraphicsHolder.SetActive(true);
            Debug.Log($"Activating UI: {gameObject.name}");
        }

        if (animate && canvasGroup != null)
        {
            if (isActive)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                canvasGroup.DOFade(1, transitionDuration).OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    OnEnable();
                });
            }
            else
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                OnDisable();

                canvasGroup.DOFade(0, transitionDuration).OnComplete(() =>
                {
                    GraphicsHolder.SetActive(false);
                });
            }
        }
        else
        {
            GraphicsHolder.SetActive(isActive);
            if (isActive)
            {
                OnEnable();
            }
            else
            {
                OnDisable();
            }
        }
    }

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
    protected virtual void OnChangeScene()
    {
        // Cleanup if needed
    }

    public void OnEnd()
    {
        // Cleanup if needed
        foreach (var tweenSlider in tweenSliderFlexes)
        {
            tweenSlider?.SlideToEnd();
        }
    }
    public void OnStart()
    {
        // Reset UI state if needed
        foreach (var tweenSlider in tweenSliderFlexes)
        {
            tweenSlider?.SlideToStart();
        }
    }
}