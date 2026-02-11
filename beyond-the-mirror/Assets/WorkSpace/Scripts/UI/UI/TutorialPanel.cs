using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TutorialPanel : BaseUI
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private string[] tutorialTexts;
    [SerializeField] private Sprite[] tutorialImages;
    [SerializeField] private float tweenDuration = 0.5f;

    private int currentSlideIndex = 0;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        backButton?.onClick.RemoveListener(OnBackClicked);
        nextButton?.onClick.RemoveListener(OnNextClicked);
        previousButton?.onClick.RemoveListener(OnPreviousClicked);

        backButton?.onClick.AddListener(OnBackClicked);
        nextButton?.onClick.AddListener(OnNextClicked);
        previousButton?.onClick.AddListener(OnPreviousClicked);

        InitializeTutorial();
    }

    protected override void OnDisable()
    {
        backButton?.onClick.RemoveListener(OnBackClicked);
        nextButton?.onClick.RemoveListener(OnNextClicked);
        previousButton?.onClick.RemoveListener(OnPreviousClicked);

        // Reset to first slide
        currentSlideIndex = 0;
        ShowCurrentSlideWithoutTween();
        UpdateNavigationButtons();
    }

    private void InitializeTutorial()
    {
        currentSlideIndex = 0;
        ShowCurrentSlideWithoutTween();
        UpdateNavigationButtons();
    }

    private void ShowCurrentSlideWithoutTween()
    {
        if (tutorialText != null && tutorialTexts != null && currentSlideIndex < tutorialTexts.Length)
        {
            tutorialText.text = tutorialTexts[currentSlideIndex];
        }

        if (tutorialImage != null && tutorialImages != null && currentSlideIndex < tutorialImages.Length)
        {
            tutorialImage.sprite = tutorialImages[currentSlideIndex];
            tutorialImage.transform.localPosition = new Vector3(0f, tutorialImage.transform.localPosition.y, tutorialImage.transform.localPosition.z);
        }
    }

    private void ShowCurrentSlide()
    {
        if (tutorialText != null && tutorialTexts != null && currentSlideIndex < tutorialTexts.Length)
        {
            // Fade out text, change, then fade in
            tutorialText.DOFade(0f, tweenDuration / 2).OnComplete(() =>
            {
                tutorialText.text = tutorialTexts[currentSlideIndex];
                tutorialText.DOFade(1f, tweenDuration / 2);
            });
        }

        if (tutorialImage != null && tutorialImages != null && currentSlideIndex < tutorialImages.Length)
        {
            // Fade out image, change, then fade in
            tutorialImage.DOFade(0f, tweenDuration / 2).OnComplete(() =>
            {
                tutorialImage.sprite = tutorialImages[currentSlideIndex];
                tutorialImage.DOFade(1f, tweenDuration / 2);
            });
        }
    }

    private void UpdateNavigationButtons()
    {
        if (previousButton != null)
            previousButton.interactable = currentSlideIndex > 0;

        if (nextButton != null)
            nextButton.interactable = currentSlideIndex < Mathf.Min(tutorialTexts?.Length ?? 0, tutorialImages?.Length ?? 0) - 1;
    }

    private void OnNextClicked()
    {
        int maxSlides = Mathf.Min(tutorialTexts?.Length ?? 0, tutorialImages?.Length ?? 0);
        if (currentSlideIndex < maxSlides - 1)
        {
            currentSlideIndex++;
            ShowCurrentSlide();
            UpdateNavigationButtons();
        }
    }

    private void OnPreviousClicked()
    {
        if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            ShowCurrentSlide();
            UpdateNavigationButtons();
        }
    }

    private void OnBackClicked()
    {
        UIManager.Instance?.ShowMainMenu();
    }
}