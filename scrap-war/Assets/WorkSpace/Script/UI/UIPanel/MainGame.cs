using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        StartCoroutine(LoadSceneAndShowUI());
    }

    private IEnumerator LoadSceneAndShowUI()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("DragonTest");
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        UIManager.Instance?.ShowInGame();
    }

    private void OnTutorialButtonClicked()
    {
        UIManager.Instance?.ShowTutorialPanel();
    }
}