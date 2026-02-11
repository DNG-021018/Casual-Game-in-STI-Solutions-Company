using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class InGame : BaseUI
{
    [Header("UI References")]
    [SerializeField] private Button SettingsButton;

    [Header("Tween Panels")]
    [SerializeField] private TweenWin tweenWin;
    [SerializeField] private TweenLose tweenLose;


    private bool isGameActive = false;
    private string currentSceneName;

    public int currentLevelIndex = 1;

    public event Action OnWin;
    public event Action<int> OnNextLevel;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;


    #region Unity Lifecycle

    protected override void Start()
    {
        base.Start();
        SettingsButton?.onClick.AddListener(OnSettingsClicked);

        SceneManager.sceneLoaded += HandleSceneLoaded;

        if (SceneManager.GetActiveScene().name != "MakeUI")
        {
            StartGameplay();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        tweenWin.BackButtonClicked -= ReturnMenu;
        tweenWin.NextLevelButtonClicked -= NextLevel;

        tweenWin.BackButtonClicked += ReturnMenu;
        tweenWin.NextLevelButtonClicked += NextLevel;
        OnEnd();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        tweenWin.BackButtonClicked -= ReturnMenu;
        tweenWin.NextLevelButtonClicked -= NextLevel;
        OnEnd();
    }

    private void Update()
    {
        if (!isGameActive) return;
    }

    #endregion

    #region Gameplay Setup

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MakeUI") return;

        HideWinPanel();
        HideLosePanel();
        StartGameplay();
    }

    private void StartGameplay()
    {
        isGameActive = true;
    }

    public void ShowInGame(int levelIndex = -1)
    {
        if (levelIndex != -1)
        {
            currentLevelIndex = levelIndex + 1;
        }
        else
        {
            currentLevelIndex = 1;
        }
        gameObject.SetActive(true);
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

    [ContextMenu("Show Win Panel")]
    public void ShowPanelWin()
    {
        isGameActive = false;
        tweenWin.ShowWinPanel(currentLevelIndex);
        if (currentLevelIndex >= 12)
        {
            tweenWin.HideButtonNextLevel();
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




