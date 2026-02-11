using System.Collections;
using TMPro;
using UnityEngine;


namespace CB_CubeRunner
{
    public class CB_MainMenuPage : CB_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels TopPanel;
        [SerializeField] Panels BottomPanel;

        [Header("Bind buttons")]
        [SerializeField] CB_UIButton startBtn;
        [SerializeField] CB_UIButton tutorialBtn;
        [SerializeField] CB_UIButton settingsBtn;
        [SerializeField] CB_UIButton shoppingBtn;

        [Header("TMP")]
        [SerializeField] TextMeshProUGUI coinText;

        Vector2 _topStart;
        Vector2 _bottomStart;

        CB_BaseUI _parent;

        public override void Init(CB_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
            startBtn.Bind(() =>
            {
                _parent.Open(UIPageId.GamePlay);
                CB_GameManager.Instance?.SetState(GameState.Play);
            });

            shoppingBtn.Bind(() =>
            {
                _parent.Open(UIPageId.Shopping);
            });

            tutorialBtn.Bind(() => _parent.Open(UIPageId.Tutorial));
            settingsBtn.Bind(() => _parent.Open(UIPageId.Settings));
        }

        private void OnDestroy()
        {
            startBtn.UnBind();
            shoppingBtn.UnBind();
            tutorialBtn.UnBind();
            settingsBtn.UnBind();
        }

        public override IEnumerator Show(object ctx = null)
        {
            if (coinText && CB_GameManager.Instance != null)
                coinText.text = CB_GameManager.Instance.TotalCoin + "/" + CB_GameManager.MAX_COIN.ToString();

            Vector2 tfrom = GetOffscreenPos(TopPanel.panel, TopPanel.slideDir, _topStart, offscreenPadding);
            Vector2 bfrom = GetOffscreenPos(BottomPanel.panel, BottomPanel.slideDir, _bottomStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (TopPanel.panel, tfrom, _topStart),
                (BottomPanel.panel, bfrom, _bottomStart)
            );
        }

        public override IEnumerator Hide()
        {
            Vector2 tTo = GetOffscreenPos(TopPanel.panel, TopPanel.slideDir, _topStart, offscreenPadding);
            Vector2 bTo = GetOffscreenPos(BottomPanel.panel, BottomPanel.slideDir, _bottomStart, offscreenPadding);

            yield return HideMovePanels(
                duration, showEase, 1f, 0f,
                (TopPanel.panel, _topStart, tTo),
                (BottomPanel.panel, _bottomStart, bTo)
            );
        }
    }
}