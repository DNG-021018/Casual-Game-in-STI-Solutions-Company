using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_UIGamePlayRoot : Wja8YNiR_BaseUI
    {
        [SerializeField] Wja8YNiR_UIButton pauseButton;
        Wja8YNiR_LevelManager levelmanager;

        void Start()
        {
            Open(UIPageId.GamePlay, null, true);

            if (pauseButton != null)
            {
                pauseButton.Bind(() => Wja8YNiR_GameManager.Instance?.SetState(GameState.Paused));
            }
            levelmanager = Wja8YNiR_LevelManager.Instance;
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
                case GameState.Playing:
                    Open(UIPageId.GamePlay);
                    break;
                case GameState.Paused:
                    Open(UIPageId.Pause);
                    break;
                case GameState.Win:
                    Open(UIPageId.WinGame);
                    break;
                case GameState.Lose:
                    Open(UIPageId.LoseGame);
                    break;
                case GameState.Setup:
                    Open(UIPageId.MirrorControlPanel);
                    break;
                default:
                    CloseAll();
                    break;
            }
        }
    }
}
