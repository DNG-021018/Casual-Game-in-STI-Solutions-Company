namespace DoublesideZ
{
    public class DZ_UIGamePlayRoot : DZ_BaseUI
    {
        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            Open(UIPageId.Mainmenu);
        }

        protected override void HandleGameState(GameState s)
        {
            switch (s)
            {
                case GameState.None:
                    break;

                case GameState.Initialize:
                    CloseAll();
                    break;

                case GameState.Ready:
                    Open(UIPageId.Mainmenu);
                    break;

                case GameState.Play:
                    Open(UIPageId.GamePlay);
                    break;

                // case GameState.Paused:
                //     Open(UIPageId.PauseMenu);
                //     break;

                case GameState.Lose:
                    CloseAll();
                    Open(UIPageId.LoseGame);
                    break;

                case GameState.Cleanup:
                    CloseAll();
                    break;
            }
        }
    }
}
