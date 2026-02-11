using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TweenWin : MonoBehaviour
{
    [Header("Tween Win Settings")]
    [SerializeField] private CanvasGroup winPanelCanvasGroup;
    [SerializeField] private GameObject Panel_Button;
    [SerializeField] private GameObject Title_Panel_Win;
    [SerializeField] private TextMeshProUGUI TitleText;
    [SerializeField] private GameObject[] Star;


    [Header("UI elements")]
    [SerializeField] private InGame inGame;
    [SerializeField] private Button BackButton;
    [SerializeField] private Button NextLevelButton;

    private Vector3 originalButtonPosition;
    private float originalBGMVolume;


    private void Start()
    {
        winPanelCanvasGroup.alpha = 0;
        winPanelCanvasGroup.interactable = false;
        winPanelCanvasGroup.blocksRaycasts = false;

        Title_Panel_Win.transform.localScale = new Vector3(0, 1, 1);

        // Store original button position and move it down
        originalButtonPosition = Panel_Button.transform.localPosition;
        Panel_Button.transform.localPosition = originalButtonPosition + Vector3.down * 500f;

        foreach (var star in Star)
        {
            star.transform.localScale = Vector3.zero;
        }

    }

    void OnEnable()
    {
        BackButton?.onClick.AddListener(() => inGame.ReturnMenu());
        NextLevelButton?.onClick.AddListener(() => inGame.NextLevel());
        InGame.OnMaxLevelReached += HideButtonNextLevel;
    }
    void OnDisable()
    {
        BackButton?.onClick.RemoveAllListeners();
        NextLevelButton?.onClick.RemoveAllListeners();
        InGame.OnMaxLevelReached -= HideButtonNextLevel;
    }

    public void ShowWinPanel(int levelIndex = 1)
    {
        TitleText.text = $"Stage {levelIndex}";
        winPanelCanvasGroup.DOFade(1, 0.5f).SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                Debug.Log(originalBGMVolume);
                Title_Panel_Win.transform.DOScaleX(1.2f, 0.8f).SetEase(Ease.OutBack).SetDelay(0.2f).
                    OnComplete(() =>
                    {
                        ShowStarsAnimated(Star.Length);
                    });
            });
        winPanelCanvasGroup.interactable = true;
        winPanelCanvasGroup.blocksRaycasts = true;
    }

    public void HideWinPanel()
    {
        Debug.Log(originalBGMVolume);
        winPanelCanvasGroup.DOFade(0, 0.5f).SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                winPanelCanvasGroup.interactable = false;
                winPanelCanvasGroup.blocksRaycasts = false;

                Title_Panel_Win.transform.localScale = new Vector3(0, 1, 1);

                Panel_Button.transform.localPosition = originalButtonPosition + Vector3.down * 500f;

                foreach (var star in Star)
                {
                    star.transform.localScale = Vector3.zero;
                    star.SetActive(false);
                }

                // foreach (var particle in Particle)
                // {
                //     particle.SetActive(false);
                // }
            });
    }

    public void ShowStarsAnimated(int starCount)
    {
        PlayStarSequence(starCount);
    }

    private void PlayStarSequence(int count)
    {
        // Chuỗi tween
        Sequence starSequence = DOTween.Sequence();

        // Danh sách index các sao theo thứ tự xuất hiện
        int[][] starGroups =
        {
        new[] { 2 },       // giữa
        new[] { 1, 3 },    // hai bên giữa
        new[] { 0, 4 }     // ngoài cùng
    };

        // Tương ứng scale cho từng nhóm
        Vector3[] scales =
        {
        new Vector3(1.5f, 1.5f, 1),
        new Vector3(1f, 1f, 1),
        new Vector3(0.7f, 0.7f, 1)
    };

        for (int groupIndex = 0; groupIndex < starGroups.Length; groupIndex++)
        {
            foreach (int i in starGroups[groupIndex])
            {
                if (i < count && i < Star.Length && Star[i] != null)
                {
                    Star[i].SetActive(true);
                    Star[i].transform.localScale = Vector3.zero;

                    // Thêm tween vào sequence, nhưng delay theo nhóm
                    starSequence.Append(
                        Star[i].transform.DOScale(scales[groupIndex], 0.4f).SetEase(Ease.OutBack)
                    );
                }
            }

            // Delay sau mỗi nhóm
            starSequence.AppendInterval(0.2f);
        }

        // Thêm tween di chuyển nút sau cùng
        starSequence.Append(
            Panel_Button.transform.DOLocalMove(originalButtonPosition, 0.6f).SetEase(Ease.OutBack)
        );
    }



    public void HideButtonNextLevel()
    {
        if (NextLevelButton != null)
        {
            NextLevelButton.gameObject.SetActive(false);
        }
    }

    [ContextMenu("TEST_ShowWinPanel")]
    public void TestShowWinPanel()
    {
        ShowWinPanel(1);
    }
}