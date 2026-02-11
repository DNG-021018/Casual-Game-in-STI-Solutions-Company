using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace NightEscape
{
    public class NE_EndGameMenu : NE_UIPage
    {
        [Header("Panel Root")]
        [SerializeField] Panels Panel;

        [Header("Status")]
        [SerializeField] Image statusImage;
        [SerializeField] Sprite winSprite;
        [SerializeField] Sprite loseSprite;

        [Header("UI Animation")]
        [SerializeField] RectTransform container;

        [Space(10)]

        [SerializeField] RectTransform winPanel;
        [SerializeField] float panelDuration = 0.5f;
        [SerializeField] Ease panelEase = Ease.OutBack;

        [Space(10)]
        [SerializeField] RectTransform star1;
        [SerializeField] Vector2 star1_Pos;

        [SerializeField] RectTransform star2;
        [SerializeField] Vector2 star2_Pos;

        [SerializeField] RectTransform star3;
        [SerializeField] Vector2 star3_Pos;

        [SerializeField] float starDuration = 0.5f;
        [SerializeField] Ease starEase = Ease.OutBack;

        [Space(10)]
        [SerializeField] RectTransform buttonGroup;
        [SerializeField] float buttonGroupDuration = 0.5f;
        [SerializeField] Ease buttonGroupEase = Ease.OutBack;

        [Header("Button")]
        [SerializeField] NE_UIButton nextLevelBtn;
        [SerializeField] NE_UIButton replayBtn;
        [SerializeField] NE_UIButton exitBtn;

        NE_GameManager gameManager => NE_GameManager.Instance;

        Vector2 _winPanel_OriginPos;
        Vector3 _winPanel_OriginScale;

        Vector2 _star1_OriginPos;
        Vector3 _star1_OriginScale;

        Vector2 _star2_OriginPos;
        Vector3 _star2_OriginScale;

        Vector2 _star3_OriginPos;
        Vector3 _star3_OriginScale;

        Vector2 _buttonsOriginPos;
        Vector3 _buttonsOriginScale;

        Vector2 _panelStart;

        bool _cached;
        Sequence _showSeq;

        public override void Init(NE_BaseUI parent)
        {
            base.Init(parent);
        }

        void Start()
        {
            if (nextLevelBtn != null) nextLevelBtn.Bind(() =>
            {
                NE_AudioManager.Instance.SetBgmVolume(1f);
                int nextLevel = NE_GameManager.Instance.CurrentLevel + 1;
                NE_GameManager.Instance.LoadLevelScene(nextLevel);
            });

            if (replayBtn != null) replayBtn.Bind(() =>
            {
                NE_AudioManager.Instance.SetBgmVolume(1f);
                NE_GameManager.Instance.LoadLevelScene(NE_GameManager.Instance.CurrentLevel);
            });

            if (exitBtn != null) exitBtn.Bind(() =>
            {
                NE_AudioManager.Instance.SetBgmVolume(1f);
                NE_LoadingScreenRoot.Instance.LoadScene("MainMenu");
            });

            bool isWin = gameManager.GetState() == GameState.Win;
            bool isLose = gameManager.GetState() == GameState.Lose;
            bool hasNextLevel = !gameManager.CheckNextLevelInvalid();

            if (nextLevelBtn != null)
                nextLevelBtn.gameObject.SetActive(isWin && hasNextLevel);

            if (replayBtn != null)
                replayBtn.gameObject.SetActive(isLose);

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
            if (Panel.panel) _panelStart = Panel.panel.anchoredPosition;
            if (winPanel != null)
            {
                _winPanel_OriginPos = winPanel.anchoredPosition;
                _winPanel_OriginScale = winPanel.localScale;
            }

            if (star1 != null)
            {
                var starCtrl = star1.GetComponentInParent<NE_Star>();
                if (starCtrl != null && starCtrl.CollectRect != null)
                {
                    _star1_OriginPos = Vector2.zero;
                    _star1_OriginScale = starCtrl.CollectOriginScale;
                }
                else
                {
                    _star1_OriginPos = star1.anchoredPosition;
                    _star1_OriginScale = star1.localScale;
                }
            }

            if (star2 != null)
            {
                var starCtrl = star2.GetComponentInParent<NE_Star>();
                if (starCtrl != null && starCtrl.CollectRect != null)
                {
                    _star2_OriginPos = Vector2.zero;
                    _star2_OriginScale = starCtrl.CollectOriginScale;
                }
                else
                {
                    _star2_OriginPos = star2.anchoredPosition;
                    _star2_OriginScale = star2.localScale;
                }
            }

            if (star3 != null)
            {
                var starCtrl = star3.GetComponentInParent<NE_Star>();
                if (starCtrl != null && starCtrl.CollectRect != null)
                {
                    _star3_OriginPos = Vector2.zero;
                    _star3_OriginScale = starCtrl.CollectOriginScale;
                }
                else
                {
                    _star3_OriginPos = star3.anchoredPosition;
                    _star3_OriginScale = star3.localScale;
                }
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
            CacheStartPositions();

            statusImage.sprite = gameManager.GetState() == GameState.Win ? winSprite : loseSprite;

            if (NE_GameManager.Instance.GetState() == GameState.Win)
            {
                base.Show(ctx);
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

                if (container != null)
                {
                    container.localScale = Vector3.one;
                }

                if (winPanel != null)
                {
                    winPanel.anchoredPosition = _winPanel_OriginPos;
                    winPanel.localScale = Vector3.zero;
                }

                ResetStar(star1);
                ResetStar(star2);
                ResetStar(star3);

                if (buttonGroup != null)
                {
                    buttonGroup.localScale = _buttonsOriginScale;
                    buttonGroup.anchoredPosition = new Vector2(_buttonsOriginPos.x, -Screen.height);
                }

                _showSeq = DOTween.Sequence();

                if (winPanel != null)
                {
                    _showSeq.Append(
                        winPanel.DOScale(_winPanel_OriginScale, panelDuration)
                                .SetEase(panelEase)
                    );
                }

                AppendStarSequence(_showSeq, star1, Vector2.zero, _star1_OriginScale, star1_Pos);
                AppendStarSequence(_showSeq, star2, Vector2.zero, _star2_OriginScale, star2_Pos);
                AppendStarSequence(_showSeq, star3, Vector2.zero, _star3_OriginScale, star3_Pos);

                if (buttonGroup != null)
                {
                    _showSeq.AppendInterval(0.25f);
                    _showSeq.Append(
                        buttonGroup.DOAnchorPos(_buttonsOriginPos, buttonGroupDuration)
                                   .SetEase(buttonGroupEase)
                    );
                }

                yield return _showSeq.WaitForCompletion();
            }
            else if (NE_GameManager.Instance.GetState() == GameState.Lose)
            {
                base.Show(ctx);
                HideStar(star1);
                HideStar(star2);
                HideStar(star3);

                Vector2 rFrom = GetOffscreenPos(Panel.panel, Panel.slideDir, _panelStart, offscreenPadding);

                yield return ShowMovePanels(
                    duration, showEase, 0f, 1f,
                    (Panel.panel, rFrom, _panelStart)
                );
            }
        }

        void HideStar(RectTransform star)
        {
            if (star == null) return;

            NE_Star starCtrl = star.GetComponentInParent<NE_Star>();
            if (starCtrl != null)
            {
                starCtrl.Hide();
            }
        }

        public override IEnumerator Hide()
        {
            _showSeq?.Kill();

            if (NE_GameManager.Instance.GetState() == GameState.Win)
            {
                base.Hide();
                yield return HideScalePanels(
                    duration, hideEase, 1f, 0f,
                    (Panel.panel, Panel.panel.localScale, Vector3.zero)
                );
            }
            else if (NE_GameManager.Instance.GetState() == GameState.Lose)
            {
                base.Hide();
                Vector2 rTo = GetOffscreenPos(Panel.panel, Panel.slideDir, _panelStart, offscreenPadding);

                yield return HideMovePanels(
                    duration, hideEase, 1f, 0f,
                    (Panel.panel, _panelStart, rTo)
                );
            }
        }

        void ResetStar(RectTransform star)
        {
            if (star == null) return;

            NE_Star starCtrl = star.GetComponentInParent<NE_Star>();
            if (starCtrl != null)
            {
                starCtrl.ResetVisual();
            }
        }

        void AppendStarSequence(
            Sequence parentSeq,
            RectTransform star,
            Vector2 targetPos,
            Vector3 targetScale,
            Vector2 startPos)
        {
            if (star == null) return;

            NE_Star starCtrl = star.GetComponentInParent<NE_Star>();
            if (starCtrl == null || starCtrl.CollectRect == null) return;

            RectTransform collectRect = starCtrl.CollectRect;

            parentSeq.AppendCallback(() =>
            {
                starCtrl.PrepareForAnimation(startPos);
            });

            Sequence starSeq = DOTween.Sequence();
            starSeq.Join(
                collectRect.DOAnchorPos(targetPos, starDuration)
                           .SetEase(starEase)
            );
            starSeq.Join(
                collectRect.DOScale(targetScale, starDuration)
                           .SetEase(starEase)
            );

            starSeq.OnComplete(() =>
            {
                starCtrl.SetCollected(true);

                if (container == null) return;

                container.DOKill();
                container.localScale = Vector3.one;

                container
                    .DOScale(0.9f, 0.07f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        container
                            .DOScale(1f, 0.12f)
                            .SetEase(Ease.OutBack);
                    });
            });

            parentSeq.Append(starSeq);
        }
    }
}
