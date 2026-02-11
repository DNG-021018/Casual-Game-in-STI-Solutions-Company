using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public enum UIState
    {
        MainMenu,
        InGame,
        Level,
        TutorialPanel
    }

    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private MainGame mainMenuUI;
    [SerializeField] private EditGameManager editGameUI;
    [SerializeField] private EditGameManager editGameUIInGame;
    [SerializeField] private LevelPanel levelPanel;
    [SerializeField] private InGame inGameUI;
    [SerializeField] private Loading loadingUI;
    [SerializeField] private TutorialPanel tutorialPanel;
    public static event Action OnLoadingComplete;
    // public static event Action<bool> OnEditModeToggle;
    private UIState currentUIState;
    private UIState targetUIState;
    private bool isTransitioning = false;
    private bool IsEditMode = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to events
        Loading.OnLoadingComplete += HandleLoadingComplete;
    }

    private void Start()
    {
        ShowLoading();
        targetUIState = UIState.MainMenu;
    }
    private void OnDestroy()
    {
        Loading.OnLoadingComplete -= HandleLoadingComplete;
    }

    private void HandleLoadingComplete()
    {
        OnLoadingComplete?.Invoke();

        if (isTransitioning)
        {
            StartCoroutine(CompleteUITransitionSmoothly());
        }
        else
        {
            ShowMainMenuDirect();
        }
    }

    private IEnumerator CompleteUITransitionSmoothly()
    {
        switch (targetUIState)
        {
            case UIState.MainMenu:
                mainMenuUI?.SetUIActive(true);
                break;
            case UIState.InGame:
                inGameUI?.SetUIActive(true);
                break;
            case UIState.Level:
                levelPanel?.SetUIActive(true);
                break;
            case UIState.TutorialPanel:
                tutorialPanel?.SetUIActive(true);
                break;
            default:
                Debug.LogWarning("Unknown UIState: " + targetUIState);
                yield break;
        }
        yield return null;
        currentUIState = targetUIState;
        isTransitioning = false;
    }

    public void ShowLoading()
    {
        if (loadingUI != null)
        {
            loadingUI.gameObject.SetActive(true);
        }
    }

    private void SetUIStateWithLoading(UIState newState)
    {
        if (currentUIState == newState || isTransitioning) return;

        targetUIState = newState;
        isTransitioning = true;

        // Hide all current UIs
        HideAllUIs();

        // Show loading
        ShowLoading();
    }

    private void ShowMainMenuDirect()
    {
        currentUIState = UIState.MainMenu;
        mainMenuUI?.SetUIActive(true);
    }

    private void HideAllUIs()
    {
        mainMenuUI?.SetUIActive(false);
        inGameUI?.SetUIActive(false);
        levelPanel?.SetUIActive(false);
        tutorialPanel?.SetUIActive(false);
    }

    public void ShowMainMenu()
    {
        SetUIStateWithLoading(UIState.MainMenu);
    }

    public void ShowInGame(int levelIndex)
    {
        if (inGameUI != null)
        {
            inGameUI.SetUIActive(true);
            inGameUI.ShowInGame(levelIndex);
        }
        SetUIStateWithLoading(UIState.InGame);
    }

    public void ShowLevelPanel()
    {
        SetUIStateWithLoading(UIState.Level);
    }
    public void ShowTutorialPanel()
    {
        SetUIStateWithLoading(UIState.TutorialPanel);
    }

    public void ShowEditMode(bool show)
    {
        editGameUI?.SetUIActive(show);
        // OnEditModeToggle?.Invoke(show);
        IsEditMode = show;
    }
    public void ShowEditModeInGame(bool show)
    {
        editGameUIInGame?.SetUIActive(show);
        IsEditMode = show;
        // OnEditModeToggle?.Invoke(show);
    }
    public void OnPlayAgainButtonClicked()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
        // editGameUIInGame?.SetUIActive(false);
    }
    public bool IsEditModeActive()
    {
        return IsEditMode;
    }
}