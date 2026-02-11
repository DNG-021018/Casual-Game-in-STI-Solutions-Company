using System.Collections;
using UnityEngine;

namespace VoltaTwins
{
    public class VT_TutorialPage : VT_UIPage
    {
        [Header("Panel")]
        [SerializeField] Panels TutorialPanel;

        [Header("Buttons")]
        [SerializeField] VT_UIButton nextBtn;
        [SerializeField] VT_UIButton prevBtn;
        [SerializeField] VT_UIButton exitBtn;
        [SerializeField] VT_UIHorizontalPager pager;

        VT_BaseUI parent;

        Vector2 _menuStart;

        public override void Init(VT_BaseUI p)
        {
            base.Init(p);
            parent = p;

            nextBtn.Bind(() => pager.Next());
            prevBtn.Bind(() => pager.Prev());
            exitBtn.Bind(() => parent.Back());

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
