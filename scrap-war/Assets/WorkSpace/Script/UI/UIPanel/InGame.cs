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

    [Header("Gameplay State")]
    [SerializeField] private DragonController dragonController;
    [SerializeField] private PlayerController playerController;

    private bool isGameActive = false;

    public event Action OnWin;
    public event Action OnTryAgain;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;

    private InputHandler inputHandler;


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
        dragonController.OnDragonDeath += OnLevelCompleted;
        playerController.OnPlayerDeath += OnLevelCompleted;
        inputHandler = FindFirstObjectByType<InputHandler>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        tweenWin.BackButtonClicked -= ReturnMenu;
        tweenWin.TryAgainClicked -= PlayAgainGame;

        tweenWin.BackButtonClicked += ReturnMenu;
        tweenWin.TryAgainClicked += PlayAgainGame;
        OnEnd();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        tweenWin.BackButtonClicked -= ReturnMenu;
        tweenWin.TryAgainClicked -= PlayAgainGame;
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

    public void ShowInGame()
    {
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
        // HideLosePanel();
        // HideWinPanel();
        inputHandler.enabled = true;
        OnTryAgain?.Invoke();
    }

    public void ReturnMenu()
    {
        HideWinPanel();
        HideLosePanel();
        SceneManager.LoadScene("MakeUI");
        UIManager.Instance?.ShowMainMenu();
    }

    #endregion

    #region End Game Handling

    public void OnLevelCompleted(bool isWin)
    {
        isGameActive = false;
        if (inputHandler != null)
        {
            inputHandler.ResetInput();
            inputHandler.enabled = false;
        }
        if (SoundManager.Instance != null)
            SoundManager.Instance.PauseBGM();

        if (isWin)
        {
            if (winClip != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(winClip);
            // Debug.Log("Win sound played");
            ShowPanelWin();
        }

        else
        {
            if (loseClip != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(loseClip);
            // Debug.Log("Lose sound played");
            ShowPanelLose();
        }
    }

    [ContextMenu("Show Win Panel")]
    public void ShowPanelWin()
    {
        isGameActive = false;
        // OnWin?.Invoke();
        tweenWin.ShowWinPanel();
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




