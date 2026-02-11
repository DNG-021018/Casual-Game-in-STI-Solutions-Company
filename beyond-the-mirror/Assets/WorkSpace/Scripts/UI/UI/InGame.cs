using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class InGame : BaseUI
{
    [Header("UI References")]
    [SerializeField] private Button SettingsButton;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Tween Panels")]
    [SerializeField] private TweenWin tweenWin;
    [SerializeField] private TweenLose tweenLose;

    private int timeLimit = 0;
    private float currentTime = 0f;
    private bool isGameActive = false;
    private string currentSceneName;

    public int currentLevelIndex = 1;

    private LevelManager levelManager;
    private GameController gameController;
    private SegmentManager segmentManager;

    public static event Action OnWin;
    public static event Action<int> OnNextLevel;
    public static event Action OnMaxLevelReached;
    public static event Action OnTimeUp;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;


    #region Unity Lifecycle

    protected override void Start()
    {
        base.Start();
        SettingsButton?.onClick.AddListener(OnSettingsClicked);

        timerText.text = "00:00";
        SceneManager.sceneLoaded += HandleSceneLoaded;

        if (SceneManager.GetActiveScene().name != "MakeUI")
        {
            SetupDependencies();
            StartGameplay();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        OnEnd(); // đảm bảo các tween không bị kẹt
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        OnEnd();
    }

    private void Update()
    {
        if (!isGameActive) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(0f, currentTime); // không để âm

        UpdateTimerDisplay();

        if (timeLimit > 0 && currentTime <= 0f)
        {
            HandleTimeUp();
        }
    }

    #endregion

    #region Gameplay Setup

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MakeUI") return;

        HideWinPanel();
        HideLosePanel();
        SetupDependencies();
        StartGameplay();
    }

    private void SetupDependencies()
    {
        levelManager = FindAnyObjectByType<LevelManager>();
        gameController = FindAnyObjectByType<GameController>();
        segmentManager = FindAnyObjectByType<SegmentManager>();
    }

    private void StartGameplay()
    {
        if (gameController == null)
        {
            Debug.LogError("❌ GameController not found.");
            return;
        }

        gameController.StartGame();
        levelManager.OnEndGame += OnLevelCompleted;

        timeLimit = segmentManager?.segmentData.timeLimit ?? 0;
        currentTime = timeLimit;
        isGameActive = true;
    }

    public void ShowInGame(int levelIndex = -1)
    {
        currentLevelIndex = (levelIndex != -1) ? levelIndex + 1 : 1;
        gameObject.SetActive(true);
    }

    #endregion

    #region Timer Logic

    private void UpdateTimerDisplay()
    {
        int secondsLeft = Mathf.CeilToInt(currentTime);
        int minutes = secondsLeft / 60;
        int seconds = secondsLeft % 60;
        timerText.text = $"{minutes:00}:{seconds:00}";

        timerText.color = (currentTime <= 10f && currentTime > 0f) ? Color.yellow : Color.white;
    }

    private void HandleTimeUp()
    {
        isGameActive = false;
        OnTimeUp?.Invoke();
        // Có thể trigger lose tự động nếu cần
    }

    #endregion

    #region Game Control

    private void OnSettingsClicked()
    {
        UIManager.Instance?.ShowEditModeInGame(true);
    }

    public void PlayAgainGame()
    {
        HideLosePanel();
        ReloadCurrentScene();
    }

    public void NextLevel()
    {
        isGameActive = false;
        currentTime = 0f;
        HideWinPanel();
        OnStart();
        OnNextLevel?.Invoke(currentLevelIndex);
    }

    public void ReturnMenu()
    {
        HideWinPanel();
        HideLosePanel();
        SceneManager.LoadScene("MakeUI");
        UIManager.Instance?.ShowMainMenu();
    }

    private void ReloadCurrentScene()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    #endregion

    #region End Game Handling

    public void OnLevelCompleted(bool isWin)
    {
        isGameActive = false;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PauseBGM();

        if (isWin)
        {
            if (winClip != null && SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(winClip);
            ShowPanelWin();
        }
        else
        {
            if (loseClip != null && SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(loseClip);
            ShowPanelLose();
        }
    }

    public void ShowPanelWin()
    {
        isGameActive = false;
        tweenWin.ShowWinPanel(currentLevelIndex);
        if (currentLevelIndex >= 3)
        {
            OnMaxLevelReached?.Invoke();
        }
        OnWin?.Invoke();
    }

    public void ShowPanelLose()
    {
        isGameActive = false;
        tweenLose.ShowLosePanel();
    }

    public void HideWinPanel()
    {
        isGameActive = false;
        tweenWin.HideWinPanel();
        if (SoundManager.Instance != null)
            SoundManager.Instance.ResumeBGM();
    }

    public void HideLosePanel()
    {
        isGameActive = false;
        tweenLose.HideLosePanel();
        if (SoundManager.Instance != null)
            SoundManager.Instance.ResumeBGM();
    }

    #endregion
}




