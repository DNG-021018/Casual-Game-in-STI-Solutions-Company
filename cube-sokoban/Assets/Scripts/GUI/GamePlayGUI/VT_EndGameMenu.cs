using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace CubeSokoban
{
    public class CS_EndGameMenu : CS_UIPage
    {
        [Header("Panel Root")]
        [SerializeField] Panels Panel;

        [Header("Image")]
        [SerializeField] Image levelImage;
        [SerializeField] Sprite[] levelSprites;

        [Header("Logo Graphics")]
        [SerializeField] RectTransform logo;
        [SerializeField] float logoDuration = 0.5f;
        [SerializeField] float logoMoveDuration = 0.35f;
        [SerializeField] float logoMoveDelay = 0.1f;
        [SerializeField] Ease logoEase = Ease.OutBack;

        [Header("Panel Graphics")]
        [SerializeField] RectTransform winTextPanel;
        [SerializeField] float panelDuration = 0.5f;
        [SerializeField] Ease panelEase = Ease.OutBack;

        [Header("Button Groups Graphics")]
        [SerializeField] RectTransform buttonGroup;
        [SerializeField] float buttonGroupDuration = 0.5f;
        [SerializeField] Ease buttonGroupEase = Ease.OutBack;

        [Header("Button")]
        [SerializeField] CS_UIButton nextLevelBtn;
        [SerializeField] CS_UIButton replayBtn;
        [SerializeField] CS_UIButton exitBtn;

        [Header("Clip")]
        [SerializeField] AudioClip AwakeClip;

        CS_LevelManager levelManager;

        Vector2 _logoOriginPos;
        Vector2 _panelOriginPos;
        Vector2 _buttonsOriginPos;

        Vector3 _logoOriginScale;
        Vector3 _panelOriginScale;
        Vector3 _buttonsOriginScale;

        bool _cached;
        Sequence _showSeq;

        public override void Init(CS_BaseUI parent)
        {
            base.Init(parent);
        }

        void Start()
        {
            levelManager = CS_LevelManager.Instance;

            if (nextLevelBtn != null) nextLevelBtn.Bind(() =>
            {
                CS_GameManager.Instance.currentLevel++;
                CS_LoadingScreenRoot.Instance.LoadScene("GamePlay");
            });

            if (replayBtn != null) replayBtn.Bind(() =>
            {
                CS_LoadingScreenRoot.Instance.LoadScene("GamePlay");
            });

            if (exitBtn != null) exitBtn.Bind(() =>
            {
                CS_LoadingScreenRoot.Instance.LoadScene("StartGame");
            });

            if (nextLevelBtn != null)
                nextLevelBtn.gameObject.SetActive(!levelManager.CheckNextLevelInvalid());

            CacheStartPositions();
        }

        void OnDestroy()
        {
            _showSeq?.Kill();

            if (nextLevelBtn != null) nextLevelBtn.UnBind();
            if (replayBtn != null) replayBtn.UnBind();
            if (exitBtn != null) exitBtn.UnBind();
        }

        protected override void CacheStartPositions()
        {
            if (_cached) return;

            if (logo != null)
            {
                _logoOriginPos = logo.anchoredPosition;
                _logoOriginScale = logo.localScale;
            }

            if (winTextPanel != null)
            {
                _panelOriginPos = winTextPanel.anchoredPosition;
                _panelOriginScale = winTextPanel.localScale;
            }

            if (buttonGroup != null)
            {
                _buttonsOriginPos = buttonGroup.anchoredPosition;
                _buttonsOriginScale = buttonGroup.localScale;
            }

            _cached = true;
        }

        public override IEnumerator Show(object ctx = null)
        {
            base.Show(ctx);
            CacheStartPositions();

            int idx = CS_GameManager.Instance.currentLevel - 1;
            if (levelImage != null && levelSprites != null && levelSprites.Length > 0)
            {
                idx = Mathf.Clamp(idx, 0, levelSprites.Length - 1);
                levelImage.sprite = levelSprites[idx];
            }

            if (CS_AudioManager.Instance && AwakeClip)
            {
                CS_AudioManager.Instance.PlaySfx(AwakeClip);
            }

            yield return new WaitForSecondsRealtime(1f);

            gameObject.SetActive(true);
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            _showSeq?.Kill();

            if (Panel.panel != null)
            {
                Panel.panel.gameObject.SetActive(true);
                Panel.panel.localScale = Vector3.one;
                Panel.panel.anchoredPosition = Vector2.zero;
            }

            if (logo != null)
            {
                logo.localScale = Vector3.zero;
                logo.anchoredPosition = Vector2.zero;
            }

            if (winTextPanel != null)
            {
                winTextPanel.localScale = _panelOriginScale;
                winTextPanel.anchoredPosition =
                    new Vector2(-Screen.width, _panelOriginPos.y);
            }

            if (buttonGroup != null)
            {
                buttonGroup.localScale = _buttonsOriginScale;
                buttonGroup.anchoredPosition =
                    new Vector2(_buttonsOriginPos.x, -Screen.height);
            }

            _showSeq = DOTween.Sequence();

            if (logo != null)
            {
                _showSeq.Append(
                    logo.DOScale(_logoOriginScale, logoDuration)
                        .SetEase(logoEase)
                );

                _showSeq.AppendInterval(logoMoveDelay);

                _showSeq.Append(
                    logo.DOAnchorPos(_logoOriginPos, logoMoveDuration)
                        .SetEase(logoEase)
                );
            }

            if (winTextPanel != null)
            {
                _showSeq.Append(
                    winTextPanel.DOAnchorPos(_panelOriginPos, panelDuration)
                                .SetEase(panelEase)
                );
            }

            if (buttonGroup != null)
            {
                _showSeq.AppendInterval(1f);
                _showSeq.Append(
                    buttonGroup.DOAnchorPos(_buttonsOriginPos, buttonGroupDuration)
                               .SetEase(buttonGroupEase)
                );
            }

            yield return _showSeq.WaitForCompletion();
        }

        public override IEnumerator Hide()
        {
            base.Hide();

            _showSeq?.Kill();

            if (Panel.panel != null)
            {
                yield return HideScalePanels(
                    duration, hideEase, 1f, 0f,
                    (Panel.panel, Panel.panel.localScale, Vector3.zero)
                );
            }
            else
            {
                yield break;
            }
        }
    }
}
