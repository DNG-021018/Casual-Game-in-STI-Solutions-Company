using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class LevelPanel : BaseUI
{
    [Header("Level Panel Specific")]
    [SerializeField] private Button backButton;
    [SerializeField] private LevelButton levelButtonCanNotPlayPrefab;
    [SerializeField] private LevelButton levelButtonCanPlayPrefab;
    [SerializeField] private Transform levelButtonContainer;
    [SerializeField] private LevelGame[] levelGames;

    private List<LevelButton> levelButtons = new List<LevelButton>();
    public int levelPlayer = 0;

    private const string LEVEL_PROGRESS_KEY = "LevelProgress";

    public static event Action<LevelGame> OnStartGame;

    protected override void Start()
    {
        base.Start();

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        // Load progress first before creating buttons
        LoadPlayerProgress();
                ClearLevelButtons();

        for (int i = 0; i < levelGames.Length; i++)
        {
            CreateLevelButton(i);
        }
        InGame.OnWin += UpgradeLevelPlayer;
        InGame.OnNextLevel += OnNextLevelClicked;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadPlayerProgress();
        UpdateLevelButtons();
    }

    private int LoadPlayerProgress()
    {
        levelPlayer = PlayerPrefs.GetInt(LEVEL_PROGRESS_KEY, 0);
        return levelPlayer;
    }

    private void SavePlayerProgress()
    {
        PlayerPrefs.SetInt(LEVEL_PROGRESS_KEY, levelPlayer);
        PlayerPrefs.Save();
    }

    private void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            if (levelButtons[i] != null)
            {
                bool isUnlocked = i <= levelPlayer;
                levelButtons[i].SetUnlockState(isUnlocked);
            }
        }
    }

    private void OnBackClicked()
    {
        UIManager.Instance?.ShowMainMenu();
    }

    private void OnStartGameClicked(LevelGame levelGame, int levelIndex = 0)
    {
        UIManager.Instance?.ShowInGame(levelIndex);
        Debug.Log($"Starting game for level {levelGame.LevelIndex + 1} with scene: {levelGame.SceneName}");
        SceneManager.LoadScene(levelGame.SceneName);
    }

    private void OnLevelSelected(LevelGame levelGame)
    {
        if (levelGame.LevelIndex <= levelPlayer)
        {
            OnStartGameClicked(levelGame, levelGame.LevelIndex);
            OnStartGame?.Invoke(levelGame);
        }
        else
        {
            Debug.LogWarning($"Level {levelGame.LevelIndex + 1} is locked. Please unlock it first.");
        }
    }

    public void OnNextLevelClicked(int levelIndex)
    {
        OnLevelSelected(levelGames[levelIndex]);
    }

    private void CreateLevelButton(int levelIndex)
    {
        bool isUnlocked = levelIndex <= levelPlayer;

        LevelButton newButton;
        if (isUnlocked)
        {
            newButton = Instantiate(levelButtonCanPlayPrefab, levelButtonContainer);
        }
        else
        {
            newButton = Instantiate(levelButtonCanNotPlayPrefab, levelButtonContainer);
        }

        levelButtons.Add(newButton);

        // Get scene name for this level
        LevelGame levelGame = levelIndex < levelGames.Length ? levelGames[levelIndex] : null;

        newButton.InitializeButton(levelGame, (index, scene) => OnLevelSelected(levelGame));
        newButton.SetUnlockState(isUnlocked);
    }

    public void UpgradeLevelPlayer()
    {
        InGame ingame = FindObjectOfType<InGame>();
        // Chỉ nâng level nếu người chơi vừa thắng level cao nhất họ từng mở
        if (ingame.currentLevelIndex - 1 == levelPlayer)
        {
            levelPlayer++;
            SavePlayerProgress();
            ClearLevelButtons();

            for (int i = 0; i < levelGames.Length; i++)
            {
                CreateLevelButton(i);
            }
        }
    }


    private void ClearLevelButtons()
    {
        foreach (var button in levelButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }

        levelButtons.Clear();
    }

    // Method to reset progress (useful for testing or reset functionality)
    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        levelPlayer = 0;
        SavePlayerProgress();
        ClearLevelButtons();

        for (int i = 0; i < levelGames.Length; i++)
        {
            CreateLevelButton(i);
        }
    }
}