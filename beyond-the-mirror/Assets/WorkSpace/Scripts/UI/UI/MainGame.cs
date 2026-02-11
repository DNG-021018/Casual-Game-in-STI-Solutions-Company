using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGame : BaseUI
{
    [Header("Main Game Specific")]
    [SerializeField] private Button editGameButton;
    [SerializeField] private Button ChangeSceneLevelButton;
    [SerializeField] private Button TutorialButton;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        OnEnd();
        editGameButton?.onClick.AddListener(OnEditGameButtonClicked);
        ChangeSceneLevelButton?.onClick.AddListener(OnChangeScene);
        TutorialButton?.onClick.AddListener(OnTutorialButtonClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        OnStart();
        editGameButton?.onClick.RemoveListener(OnEditGameButtonClicked);
        ChangeSceneLevelButton?.onClick.RemoveListener(OnChangeScene);
        TutorialButton?.onClick.RemoveListener(OnTutorialButtonClicked);
    }

    private void OnEditGameButtonClicked()
    {
        UIManager.Instance?.ShowEditMode(true);
    }


    protected override void OnChangeScene()
    {
        UIManager.Instance?.ShowLevelPanel();
    }

    private void OnTutorialButtonClicked()
    {
        UIManager.Instance?.ShowTutorialPanel();
    }
}