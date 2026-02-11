using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_MainMenuPage : Wja8YNiR_UIPage
    {
        [Header("Bind buttons")]
        [SerializeField] Wja8YNiR_UIButton startBtn;
        [SerializeField] Wja8YNiR_UIButton tutorialBtn;
        [SerializeField] Wja8YNiR_UIButton settingsBtn;

        Wja8YNiR_BaseUI _parent;

        public override void Init(Wja8YNiR_BaseUI parent)
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
            yield return base.Hide();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = true;
            canvasGroup.alpha = 0f;
            yield break;
        }
    }
}
