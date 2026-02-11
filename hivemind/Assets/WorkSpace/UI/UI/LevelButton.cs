using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Button levelButton;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image spriteBaseOnLevel;

    public int levelIndex;
    private Action<int> onClickAction; // Lưu callback

    public void InitializeButton(int levelGame, Action<int> onClickAction = null, bool isUnlocked = true, Sprite sprite = null)
    {
        this.levelIndex = levelGame;
        this.onClickAction = onClickAction;

        SetUnlockState(isUnlocked);
        SetupButton(sprite);
        UpdateLevelDisplay();
    }

    private void UpdateLevelDisplay()
    {
        if (levelText != null)
        {
            levelText.text = (levelIndex + 1).ToString();
        }
    }

    private void SetupButton(Sprite sprite)
    {
        spriteBaseOnLevel.sprite =  sprite;
        if (levelButton != null)
        {
            levelButton.onClick.RemoveAllListeners();
            levelButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        onClickAction?.Invoke(levelIndex);
    }

    public void SetUnlockState(bool isUnlocked)
    {
        if (levelButton != null)
        {
            levelButton.interactable = isUnlocked;
        }
    }
}
