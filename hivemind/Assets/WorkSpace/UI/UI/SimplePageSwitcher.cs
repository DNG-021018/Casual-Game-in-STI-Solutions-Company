using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Numerics;

public class SimplePageSwitcher : MonoBehaviour
{
    public int maxPage = 2;
    int currentPage;
    public UnityEngine.Vector3 targetPosition;
    public UnityEngine.Vector3 PageStep;
    public RectTransform levelPageRectTransform;
    public float tweenTime;

    public Button buttonNext;
    public Button buttonPrevious;

    private void Start()
    {
        buttonNext.onClick.AddListener(OnNextButtonClicked);
        buttonPrevious.onClick.AddListener(OnPreviousButtonClicked);
    }

    void OnEnable()
    {
        currentPage = 1;
        targetPosition = levelPageRectTransform.localPosition;
    }

    public void MovePage()
    {
        levelPageRectTransform.DOLocalMove(targetPosition, tweenTime)
            .SetEase(Ease.OutCubic);
    }

    public void OnNextButtonClicked()
    {
        if (currentPage < maxPage)
        {
            currentPage++;
            targetPosition += PageStep;
            MovePage();
        }
    }

    public void OnPreviousButtonClicked()
    {
        if (currentPage > 1)
        {
            currentPage--;
            targetPosition -= PageStep;
            MovePage();
        }
    }

}