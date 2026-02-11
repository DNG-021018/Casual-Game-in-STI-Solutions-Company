using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditGameManager : BaseUI
{
    [Header("Edit Game Specific")]
    [SerializeField] private TweenSliderFlex tweenSliderFlex;
    [SerializeField] private Button closeButton;

    [SerializeField] private Button againButton;
    [SerializeField] private Button homeButton;


    public static event Action OnEditOpen;
    public static event Action OnEditClose;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        closeButton?.onClick.AddListener(() => UIManager.Instance?.ShowEditMode(false));
        closeButton?.onClick.AddListener(() => UIManager.Instance?.ShowEditModeInGame(false));
        againButton?.onClick.AddListener(OnAgainButtonClicked);
        homeButton?.onClick.AddListener(OnHomeButtonClicked);
        if (tweenSliderFlex != null)
        {
            tweenSliderFlex.OnSlideToEndComplete += () => StopGame();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        closeButton?.onClick.RemoveAllListeners();
        againButton?.onClick.RemoveAllListeners();
        homeButton?.onClick.RemoveAllListeners();
    }

    public override void SetUIActive(bool isActive, bool animate = true)
    {
        base.SetUIActive(isActive, animate);

        if (tweenSliderFlex != null)
        {
            if (isActive)
            {
                tweenSliderFlex.SlideToEnd();
            }
            else
            {
                ResumeGame();
                tweenSliderFlex.SlideToStart();
            }
        }

        if (IsInGameScene())
        {
            InputHandler inputHandler = FindObjectOfType<InputHandler>();

            if (inputHandler != null)
            {
                inputHandler.enabled = !isActive;

                if (isActive)
                {
                    inputHandler.ResetInput();
                }
            }
        }
    }
    private bool IsInGameScene()
    {
        return SceneManager.GetActiveScene().name == "DragonTest";
    }


    private void OnAgainButtonClicked()
    {
        UIManager.Instance?.ShowEditModeInGame(false);
        UIManager.Instance?.OnPlayAgainButtonClicked();
    }

    private void OnHomeButtonClicked()
    {
        UIManager.Instance?.ShowMainMenu();
        UIManager.Instance?.ShowEditModeInGame(false);
        SceneManager.LoadScene("MakeUI");
        SetUIActive(false);
    }

    public void StopGame()
    {
        Time.timeScale = 0f;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}