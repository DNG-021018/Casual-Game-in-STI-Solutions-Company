using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_MainMenuPage : bJakGZQ3_UIPage
    {
        [Header("Bind buttons")]
        [SerializeField] bJakGZQ3_UIButton startBtn;
        [SerializeField] bJakGZQ3_UIButton tutorialBtn;
        [SerializeField] bJakGZQ3_UIButton settingsBtn;

        bJakGZQ3_BaseUI _parent;

        public override void Init(bJakGZQ3_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
            startBtn.Bind(() =>
            {
                bJakGZQ3_LoadingScreenRoot.Instance.LoadScene("GamePlay");
                bJakGZQ3_GameManager.Instance?.SetState(GameState.LevelSetup);
            });
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
