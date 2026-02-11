using System.Collections;
using UnityEngine;

namespace NightEscape
{
    public class NE_TutorialPage : NE_UIPage
    {
        [Header("Panel")]
        [SerializeField] Panels TutorialPanel;

        [Header("Buttons")]
        [SerializeField] NE_UIButton nextBtn;
        [SerializeField] NE_UIButton prevBtn;
        [SerializeField] NE_UIButton exitBtn;
        [SerializeField] NE_UIHorizontalPager pager;

        NE_BaseUI parent;

        Vector2 _menuStart;

        public override void Init(NE_BaseUI p)
        {
            base.Init(p);
            parent = p;

            nextBtn.Bind(() => pager.Next());
            prevBtn.Bind(() => pager.Prev());
            exitBtn.Bind(() =>
            {
                StartCoroutine(Hide());
                parent.Open(UIPageId.MainMenu);
            });

            pager.JumpTo(0, true);
        }

        void OnDestroy()
        {
            nextBtn.UnBind();
            prevBtn.UnBind();
            exitBtn.UnBind();
        }

        public override IEnumerator Show(object ctx = null)
        {
            parent.CloseAll();

            pager.JumpTo(0, true);

            Vector2 from = GetOffscreenPos(TutorialPanel.panel, TutorialPanel.slideDir, _menuStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (TutorialPanel.panel, from, _menuStart)
            );
        }

        public override IEnumerator Hide()
        {
            Vector2 to = GetOffscreenPos(TutorialPanel.panel, TutorialPanel.slideDir, _menuStart, offscreenPadding);

            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (TutorialPanel.panel, _menuStart, to)
            );
        }

        public override void ApplyContext(object ctx)
        {
            if (ctx is int i) pager.JumpTo(i, true);
        }
    }
}
