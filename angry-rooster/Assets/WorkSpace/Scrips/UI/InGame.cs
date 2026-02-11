using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGame : BaseUI
{
    [Header("In Game Specific")]
    [SerializeField] private Button SettingsButton;
    [SerializeField] private TextMeshProUGUI eggText;
    [SerializeField] private TextMeshProUGUI timerText;

    public int currentLevelIndex = 1;
    private string currentSceneName;
    private int totalEggCount = 0; // This should be set based on the level requirements
    private int timeLimit = 0; // Time limit in seconds
    private float currentTime = 0f; // Current elapsed time
    private bool isGameActive = false; // Track if game is active
    public LevelGame currentLevelGame;

    [SerializeField] private TweenWin tweenWin;
    [SerializeField] private TweenLose tweenLose;

    public static event System.Action OnWin;
    public static event System.Action<int> OnNextLevel;
    public static event System.Action OnMaxLevelReached;
    public static event System.Action OnTimeUp;

    protected override void Start()
    {
        base.Start();

        SettingsButton?.onClick.AddListener(OnSettingsClicked);

        timerText.text = "00:00";
        LevelPanel.OnStartGame += SetUpEgg;


        // Subscribe to game events
        if (GamePlayController.Instance != null)
        {
            GamePlayController.Instance.OnLevelCompleted += OnLevelCompleted;
        }
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scene.name == "MakeUI")
                return;
            if (currentLevelGame == null)
                return;
            eggText.text = $"0/{currentLevelGame.RequiredScore}";
            totalEggCount = currentLevelGame.RequiredScore;
            timeLimit = currentLevelGame.TimeLimit;
            currentTime = 0f;
            isGameActive = true;
        };
    }


    private void SetUpEgg(LevelGame levelGame)
    {
        currentLevelGame = levelGame;
    }

    void Update()
    {
        if (isGameActive)
        {
            currentTime += Time.deltaTime;
            UpdateTimer(currentTime);

            if (timeLimit > 0 && currentTime >= timeLimit)
            {
                OnTimeLimit();
            }
        }
    }

    private void OnTimeLimit()
    {
        isGameActive = false;
        OnTimeUp?.Invoke();

        if (GamePlayController.Instance != null && !GamePlayController.Instance.IsLevelCompleted)
        {
            OnLevelCompleted(false);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        OnEnd();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        OnEnd();
    }

    private void OnSettingsClicked()
    {
        UIManager.Instance?.ShowEditModeInGame(true);
    }

    public void ShowInGame(int levelIndex = -1)
    {
        currentLevelIndex = levelIndex != -1 ? levelIndex + 1 : 1; // Store level (add 1 because levelIndex is 0-based)
        gameObject.SetActive(true);
    }

    public void PlayAgainGame()
    {
        if (currentLevelGame != null)
        {
            SetUpEgg(currentLevelGame);
        }
        
        HideLosePanel();
        currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void UpdateTimer(float time)
    {
        if (timeLimit > 0)
        {
            float remainingTime = Mathf.Max(0, timeLimit - time);
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";

            if (remainingTime <= 10f && remainingTime > 0)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
        else
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }



    public void PlusEgg(int eggCollect)
    {
        eggText.text = $"{eggCollect}/{totalEggCount}";
        // Tạo sequence tween
        Sequence eggTween = DOTween.Sequence();
        eggTween.Append(eggText.transform.DOScale(1.3f, 0.2f))
                .Append(eggText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBounce))
                .Join(eggText.DOColor(Color.yellow, 0.1f))
                .Append(eggText.DOColor(Color.white, 0.4f));
    }


    [ContextMenu("Panel Lose")]
    public void ShowPanelLose()
    {
        isGameActive = false; // Stop the timer when game is lost
        tweenLose.ShowLosePanel();
    }

    [ContextMenu("Hide Lose Panel")]
    public void HideLosePanel()
    {
        isGameActive = false; // Ensure the game is not active when hiding the panel
        tweenLose.HideLosePanel();
    }

    [ContextMenu("Panel Win")]
    public void ShowPanelWin()
    {
        isGameActive = false;
        tweenWin.ShowWinPanel(currentLevelIndex);
        if (currentLevelIndex == 5) OnMaxLevelReached?.Invoke();
        OnWin?.Invoke();
    }

    [ContextMenu("Hide Win Panel")]
    public void HideWinPanel()
    {
        isGameActive = false; // Ensure the game is not active when hiding the panel
        tweenWin.HideWinPanel();
    }

    public void ReturnMenu()
    {
        HideWinPanel();
        HideLosePanel();
        SceneManager.LoadScene("MakeUI");
        UIManager.Instance?.ShowMainMenu();
    }

    public void NextLevel()
    {
        // Reset timer state khi chuyển level
        isGameActive = false;
        currentTime = 0f;
        OnStart();
        HideWinPanel();
        OnNextLevel?.Invoke(currentLevelIndex);
    }
    public void OnLevelCompleted(bool isWining)
    {
        isGameActive = false; // Stop the timer when level is completed

        if (isWining)
            ShowPanelWin();
        else
            ShowPanelLose();
    }
}