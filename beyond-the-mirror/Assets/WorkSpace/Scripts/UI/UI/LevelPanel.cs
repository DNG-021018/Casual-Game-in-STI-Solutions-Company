using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class LevelPanel : BaseUI
{
    [Header("Level Panel Specific")]
    [SerializeField] private Button backButton;
    [SerializeField] private LevelButton level1;
    [SerializeField] private LevelButton level2;
    [SerializeField] private LevelButton level3;
    [SerializeField] private Transform levelButtonContainer;
    [SerializeField] private LevelGame[] levelGames;

    private readonly List<LevelButton> levelButtons = new();
    private int levelPlayer = 0;
    private const string LEVEL_PROGRESS_KEY = "LevelProgress";

    public static event Action<LevelGame> OnStartGame;

    protected override void Start()
    {
        base.Start();

        backButton?.onClick.AddListener(OnBackClicked);

        InGame.OnWin += OnWinCheckUpgradeLevel; // Sửa lại ở đây
        InGame.OnNextLevel += OnNextLevelClicked;

        LoadPlayerProgress();
        GenerateLevelButtons();
    }

    private void OnWinCheckUpgradeLevel()
    {
        // Lấy level hiện tại từ InGame (giả sử chỉ có 1 instance)
        var inGame = FindAnyObjectByType<InGame>();
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

        for (int i = 0; i < levelGames.Length; i++)
        {
            var levelGame = levelGames[i];
            bool isUnlocked = i <= levelPlayer;

            LevelButton prefab = GetPrefabForIndex(i);
            if (prefab == null)
            {
                Debug.LogWarning($"⚠️ Không có prefab nào cho level index {i}");
                continue;
            }

            var button = Instantiate(prefab, levelButtonContainer);
            button.InitializeButton(levelGame, (_, __) => OnLevelSelected(levelGame));
            button.SetUnlockState(isUnlocked);

            levelButtons.Add(button);
        }
    }

    private LevelButton GetPrefabForIndex(int index)
    {
        return index switch
        {
            0 => level1,
            1 => level2,
            2 => level3,
            _ => null // Nếu có nhiều hơn 3 level thì bạn nên thêm prefab khác hoặc tái sử dụng prefab 1
        };
    }

    private void ClearLevelButtons()
    {
        foreach (var button in levelButtons)
        {
            if (button != null) Destroy(button.gameObject);
        }
        levelButtons.Clear();
    }

    private void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            bool isUnlocked = i <= levelPlayer;
            levelButtons[i].SetUnlockState(isUnlocked);
        }
    }

    private void OnLevelSelected(LevelGame levelGame)
    {
        if (levelGame.LevelIndex <= levelPlayer)
        {
            UIManager.Instance?.ShowInGame(levelGame.LevelIndex);
            SceneManager.LoadScene(levelGame.SceneName);
            OnStartGame?.Invoke(levelGame);
        }
        else
        {
            Debug.LogWarning($"Level {levelGame.LevelIndex + 1} is locked.");
        }
    }

    public void OnNextLevelClicked(int levelIndex)
    {
        if (levelIndex < levelGames.Length)
        {
            OnLevelSelected(levelGames[levelIndex]);
        }
    }

    public void UpgradeLevelPlayer()
    {
        if (levelPlayer < levelGames.Length - 1)
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
