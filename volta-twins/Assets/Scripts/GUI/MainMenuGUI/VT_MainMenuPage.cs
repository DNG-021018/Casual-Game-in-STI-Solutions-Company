using System.Collections;
using UnityEngine;

namespace VoltaTwins
{
    public class VT_MainMenuPage : VT_UIPage
    {
        [Header("Bind buttons")]
        [SerializeField] VT_UIButton startBtn;
        [SerializeField] VT_UIButton tutorialBtn;
        [SerializeField] VT_UIButton settingsBtn;

        VT_BaseUI _parent;

        public override void Init(VT_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
            startBtn.Bind(() => _parent.Open(UIPageId.LevelSelect));
            tutorialBtn.Bind(() => _parent.Open(UIPageId.Tutorial));
            settingsBtn.Bind(() => _parent.Open(UIPageId.Settings));
        }

        private void OnDestroy()
        {
            startBtn.UnBind();
            tutorialBtn.UnBind();
            settingsBtn.UnBind();
        }

        public override IEnumerator Show(object ctx = null)
        {
            yield return base.Show(ctx);
            canvasGroup.alpha = 1f;
        }

        public override IEnumerator Hide()
        {
            canvasGroup.blocksRaycasts = false;
            yield break;
            // canvasGroup.alpha = 0f;
        }
    }
}
