using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Button levelButton;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private List<Image> stars;
    
    private int levelIndex;
    private string sceneName;

    public void InitializeButton(LevelGame levelGame, System.Action<int, string> onClickAction = null, bool isUnlocked = true)
    {
        this.levelIndex = levelGame.LevelIndex;
        this.sceneName = levelGame.SceneName;
        SetUnlockState(isUnlocked);
        SetupButton(levelIndex, onClickAction);
    }

    private void SetupButton(int levelIndex, System.Action<int, string> onClickAction)
    {
        if (levelText != null)
            levelText.text = (levelIndex + 1).ToString();

        if (stars?.Count > 0)
        {
            for (int i = 0; i < stars.Count; i++)
            {
                stars[i].gameObject.SetActive(i <= levelIndex);
            }
        }
        levelButton?.onClick.RemoveAllListeners();
        levelButton?.onClick.AddListener(() => onClickAction?.Invoke(levelIndex, sceneName));
    }

    public void SetUnlockState(bool isUnlocked)
    {
        levelButton.interactable = isUnlocked;
        if (levelText != null)
            levelText.color = isUnlocked ? Color.white : Color.gray;
    }
    public int GetLevelIndex() => levelIndex;
    public string GetSceneName() => sceneName;
}