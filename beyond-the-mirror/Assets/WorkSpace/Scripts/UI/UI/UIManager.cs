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
    LevelManager levelManager;
    [SerializeField] private TutorialPanel tutorialPanel;

    [Header("Level Configuration")]
    [SerializeField] private LevelGame[] levelGames;


    // private GamePlayController gamePlayController;

    // Events for better decoupling
    public static event Action OnLoadingComplete;
    public static event Action<bool> OnEditModeToggle;
    private UIState currentUIState;
    private UIState targetUIState;
    private bool isTransitioning = false;

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
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (IsGameplayScene(arg0.name))
        {
            // if (gamePlayController != null)
            // {
            //     gamePlayController.OnScoreChanged += inGameUI.PlusEgg;
            //     gamePlayController.OnLevelCompleted += inGameUI.OnLevelCompleted;
            // }
            // levelManager = FindAnyObjectByType<LevelManager>();
            // LevelManager.Instance.OnEndGame += inGameUI.OnLevelCompleted;
        }
    }
    private bool IsGameplayScene(string sceneName)
    {
        if (levelGames != null)
        {
            foreach (var levelGame in levelGames)
            {
                if (levelGame != null && levelGame.SceneName.Equals(sceneName))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void OnSceneUnloaded(Scene arg0)
    {
        // if (gamePlayController)
        // {
        //     // Unsubscribe from events to prevent memory leaks
        //     gamePlayController.OnScoreChanged -= inGameUI.PlusEgg;
        //     gamePlayController.OnLevelCompleted -= inGameUI.OnLevelCompleted;
        //     gamePlayController = null;
        // }
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
            // Complete the UI transition after loading
            StartCoroutine(CompleteUITransitionSmoothly());
        }
        else
        {
            // Initial loading complete, show main menu
            ShowMainMenuDirect();
        }
    }

    private IEnumerator CompleteUITransitionSmoothly()
    {
        // Show the target UI first
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

        // Wait one frame to ensure UI is rendered
        yield return null;

        // Now hide loading and complete transition
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

    public void ShowInGame(int levelIndex = -1)
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
        OnEditModeToggle?.Invoke(show);
    }
    public void ShowEditModeInGame(bool show)
    {
        editGameUIInGame?.SetUIActive(show);
        OnEditModeToggle?.Invoke(show);
    }
    public void OnPlayAgainButtonClicked()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
        editGameUIInGame?.SetUIActive(false);
    }

    public void GetAvailableLevels(List<LevelGame> levelGames)
    {
        List<LevelGame> availableLevels = new List<LevelGame>();
        if (levelGames != null)
        {
            foreach (var level in levelGames)
            {
                if (level != null)
                {
                    availableLevels.Add(level);
                }
            }
        }
    }
}