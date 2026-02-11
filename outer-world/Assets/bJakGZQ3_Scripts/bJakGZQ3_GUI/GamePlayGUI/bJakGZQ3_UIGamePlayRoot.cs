using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_UIGamePlayRoot : bJakGZQ3_BaseUI
    {
        [SerializeField] bJakGZQ3_UIButton pauseButton;

        void Start()
        {
            if (pauseButton != null)
            {
                pauseButton.Bind(() => bJakGZQ3_GameManager.Instance?.SetState(GameState.Paused));
            }
        }

        void OnDestroy()
        {
            if (pauseButton != null)
            {
                pauseButton.UnBind();
            }
        }

        protected override void HandleGameState(GameState s)
        {
            switch (s)
            {
                case GameState.Play:
                    Open(UIPageId.GamePlay);
                    break;
                case GameState.Paused:
                    Open(UIPageId.Pause);
                    break;
                case GameState.FinishGame:
                    Open(UIPageId.WinGame);
                    break;
                default:
                    CloseAll();
                    break;
            }
        }
    }
}
