using System.Collections;
using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_TutorialPage : bJakGZQ3_UIPage
    {
        [Header("Panel")]
        [SerializeField] Panels TutorialPanel;

        [Header("Buttons")]
        [SerializeField] bJakGZQ3_UIButton nextBtn;
        [SerializeField] bJakGZQ3_UIButton prevBtn;
        [SerializeField] bJakGZQ3_UIButton exitBtn;
        [SerializeField] bJakGZQ3_UIHorizontalPager pager;

        bJakGZQ3_BaseUI parent;

        Vector2 _menuStart;

        public override void Init(bJakGZQ3_BaseUI p)
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
