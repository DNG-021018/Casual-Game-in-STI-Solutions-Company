using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertiblockPass
{
    public class VP_EndGameMenu : VP_UIPage
    {
        [Header("Panel Root")]
        [SerializeField] Panels Panel;

        [Header("Panels")]
        [SerializeField] RectTransform winPanel;
        [SerializeField] RectTransform losePanel;

        [Header("Image")]
        [SerializeField] Image levelImage;
        [SerializeField] Sprite[] levelSprites;

        [Header("TMP")]
        [SerializeField] TextMeshProUGUI stepText;

        [Header("Button")]
        [SerializeField] VP_UIButton nextLevelBtn;
        [SerializeField] VP_UIButton replayBtn;
        [SerializeField] VP_UIButton exitBtn;

        [Header("Clip")]
        [SerializeField] AudioClip winClip;
        [SerializeField] AudioClip loseClip;

        VP_LevelManager levelManager;

        public override void Init(VP_BaseUI parent)
        {
            levelManager = VP_LevelManager.Instance;

            base.Init(parent);
        }

        void Start()
        {
            if (nextLevelBtn != null) nextLevelBtn.Bind(() =>
            {
                VP_GameManager.Instance.currentLevel++;
                VP_LoadingScreenRoot.Instance.LoadScene("GamePlay");
            });

            if (replayBtn != null) replayBtn.Bind(() =>
            {
                VP_LoadingScreenRoot.Instance.LoadScene("GamePlay");
            });

            if (exitBtn != null) exitBtn.Bind(() =>
            {
                VP_LoadingScreenRoot.Instance.LoadScene("StartGame");
            });
        }

        void OnDestroy()
        {
            if (nextLevelBtn != null) nextLevelBtn.UnBind();
            if (replayBtn != null) replayBtn.UnBind();
            if (exitBtn != null) exitBtn.UnBind();
        }

        public override IEnumerator Show(object ctx = null)
        {
            CacheStartPositions();

            bool isWin = VP_GameManager.Instance.GetState() == GameState.Win;
            bool hasNextLevel = levelManager != null && !levelManager.CheckNextLevelInvalid();

            if (isWin)
            {
                winPanel.gameObject.SetActive(true);
                losePanel.gameObject.SetActive(false);
            }
            else
            {
                winPanel.gameObject.SetActive(false);
                losePanel.gameObject.SetActive(true);
            }

            if (nextLevelBtn != null)
                nextLevelBtn.gameObject.SetActive(isWin && hasNextLevel);

            int idx = VP_GameManager.Instance.currentLevel - 1;

            if (levelImage != null && levelSprites != null && levelSprites.Length > 0)
            {
                idx = Mathf.Clamp(idx, 0, levelSprites.Length - 1);
                levelImage.sprite = levelSprites[idx];
            }

            if (VP_AudioManager.Instance)
            {
                if (VP_GameManager.Instance.GetState() == GameState.Win && winClip != null)
                {
                    VP_AudioManager.Instance.PlaySfx(winClip);
                }
                else if (VP_GameManager.Instance.GetState() == GameState.Lose && loseClip != null)
                {
                    VP_AudioManager.Instance.PlaySfx(loseClip);
                }
            }

            if (stepText != null)
            {
                stepText.text = levelManager.GetFinalStepCount().ToString();
            }

            yield return ShowScalePanels(
                duration, showEase, 0f, 1f,
                (Panel.panel, Vector3.zero, Vector3.one)
            );
        }

        public override IEnumerator Hide()
        {
            base.Hide();

            yield return HideScalePanels(
                duration, hideEase, 1f, 0f,
                (Panel.panel, Panel.panel.localScale, Vector3.zero)
            );
        }
    }
}
