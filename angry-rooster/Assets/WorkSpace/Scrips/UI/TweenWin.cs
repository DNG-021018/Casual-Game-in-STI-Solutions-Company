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
    [SerializeField] private GameObject[] Particle;
    [SerializeField] private AudioClip winSoundClip;


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
        foreach (var particle in Particle)
        {
            particle.SetActive(false);
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
        TitleText.text = $"Stage {levelIndex} Complete!";

        if (SoundManager.Instance != null)
        {
            originalBGMVolume = SoundManager.Instance.GetBGMVolume();
            SoundManager.Instance.SetBGMVolume(0f);
        }

        winPanelCanvasGroup.DOFade(1, 0.5f).SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                SoundManager.Instance?.PlaySFX(winSoundClip);
                StartCoroutine(RestoreBGMAfterWinSound());
                Title_Panel_Win.transform.DOScaleX(1.2f, 0.8f).SetEase(Ease.OutBack).SetDelay(0.2f).
                    OnComplete(() =>
                    {
                        ShowStarsAnimated(Star.Length);
                        foreach (var particle in Particle)
                        {
                            particle.SetActive(true);
                        }
                    });
            });
        winPanelCanvasGroup.interactable = true;
        winPanelCanvasGroup.blocksRaycasts = true;
    }
    private IEnumerator RestoreBGMAfterWinSound()
    {
        if (winSoundClip != null)
        {
            yield return new WaitForSeconds(winSoundClip.length);
        }
        SoundManager.Instance?.SetBGMVolume(originalBGMVolume);
    }


    public void HideWinPanel()
    {
        SoundManager.Instance?.SetBGMVolume(originalBGMVolume);
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

                foreach (var particle in Particle)
                {
                    particle.SetActive(false);
                }
            });
    }

    public void ShowStarsAnimated(int starCount)
    {
        StartCoroutine(PlayStarSequence(starCount));
    }

    private IEnumerator PlayStarSequence(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Star[i].transform.localScale = Vector3.zero;
            Star[i].SetActive(true);
            Star[i].transform.DOScale(new Vector3(2, 2, 1), 0.4f).SetEase(Ease.OutBack);
            if (i == 2)
            {
                Star[i].transform.DOScale(new Vector3(3, 3, 1), 0.4f).SetEase(Ease.OutBack);
            }
            yield return new WaitForSeconds(0.3f);
        }

        Panel_Button.transform.DOLocalMove(originalButtonPosition, 0.6f).SetEase(Ease.OutBack);
    }

    public void HideButtonNextLevel()
    {
        if (NextLevelButton != null)
        {
            NextLevelButton.gameObject.SetActive(false);
        }
    }
}