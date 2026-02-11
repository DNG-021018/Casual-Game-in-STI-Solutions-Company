using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Button levelButton;
    
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

        levelButton?.onClick.RemoveAllListeners();
        levelButton?.onClick.AddListener(() => onClickAction?.Invoke(levelIndex, sceneName));
    }

    public void SetUnlockState(bool isUnlocked)
    {
        levelButton.interactable = isUnlocked;
    }
    public int GetLevelIndex() => levelIndex;
    public string GetSceneName() => sceneName;
}
