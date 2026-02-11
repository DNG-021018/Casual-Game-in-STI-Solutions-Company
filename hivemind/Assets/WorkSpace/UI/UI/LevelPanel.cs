using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class LevelPanel : BaseUI
{
    [Header("Level Panel Specific")]
    [SerializeField] private Button backButton;
    [SerializeField] private List<Transform> levelButtonContainerColumns; // Thay thế các column riêng lẻ
    [SerializeField] private LevelButton levelButtonPrefab;

    [SerializeField] private List<Sprite> levelSprites; // Danh sách sprite cho các level
    private List<LevelButton> levelButtons = new();
    public InGame inGame;
    public int levelPlayer = 0;
    private const string LEVEL_PROGRESS_KEY = "LevelProgress";
    public int TotalLevel;// Tổng số level trong game

    public event Action<int> OnStartGame;

    protected override void Start()
    {
        base.Start();

        backButton?.onClick.AddListener(OnBackClicked);

        inGame = FindAnyObjectByType<InGame>();

        inGame.OnWin += OnWinCheckUpgradeLevel; // Sửa lại ở đây
        inGame.OnNextLevel += OnNextLevelClicked;

        LoadPlayerProgress();
        GenerateLevelButtons();
    }

    private void OnWinCheckUpgradeLevel()
    {
        if (inGame != null && inGame.currentLevelIndex - 1 == levelPlayer)
        {
            UpgradeLevelPlayer();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadPlayerProgress();
        UpdateLevelButtons();
    }

    private void OnBackClicked()
    {
        UIManager.Instance?.ShowMainMenu();
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

    private void GenerateLevelButtons()
    {
        ClearLevelButtons();

        for (int i = 0; i < TotalLevel; i++)
        {
            Transform targetContainer = GetContainerForLevel(i);
            LevelButton newButton = Instantiate(levelButtonPrefab, targetContainer);
            newButton.InitializeButton(i, OnLevelSelected, i <= levelPlayer, GetSpriteForLevel(i));
            levelButtons.Add(newButton);
        }
    }

    private Transform GetContainerForLevel(int levelIndex)
    {
        int buttonsPerColumn = 3;
        int columnIndex = levelIndex / buttonsPerColumn;
        if (columnIndex >= levelButtonContainerColumns.Count)
        {
            columnIndex = levelButtonContainerColumns.Count - 1;
        }
        return levelButtonContainerColumns[columnIndex];
    }

    private Sprite GetSpriteForLevel(int level)
    {
        if (level >= 0 && level <= 2)
        {
            return levelSprites[0];
        }
        else if (level > 2 && level <= 5)
        {
            return levelSprites[1];
        }
        else if (level > 5 && level <= 8)
        {
            return levelSprites[2];
        }
        else if (level > 8 && level <= 11)
        {
            return levelSprites[3];
        }
        return null;
    }

    private void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            bool isUnlocked = i <= levelPlayer;
            levelButtons[i].SetUnlockState(isUnlocked);
        }
    }
    private void ClearLevelButtons()
    {
        foreach (var button in levelButtons)
        {
            if (button != null) Destroy(button.gameObject);
        }
        levelButtons.Clear();
    }

    private void OnLevelSelected(int levelIndex)
    {
        if (levelIndex <= levelPlayer)
        {
            UIManager.Instance?.ShowInGame(levelIndex);
            SceneManager.LoadScene("MakeGame");
            OnStartGame?.Invoke(levelIndex + 1);
        }
        else
        {
            Debug.LogWarning($"Level {levelIndex + 1} is locked.");
        }
    }

    public void OnNextLevelClicked(int levelIndex)
    {
        int nextLevelIndex = levelIndex;
        if (nextLevelIndex < TotalLevel)
        {
            OnLevelSelected(nextLevelIndex); // Gọi với next level
        }
    }

    public void UpgradeLevelPlayer()
    {
        if (levelPlayer < TotalLevel - 1)
        {
            levelPlayer++;
            SavePlayerProgress();
            GenerateLevelButtons();
        }
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        levelPlayer = 0;
        SavePlayerProgress();
        GenerateLevelButtons();
    }
}
