namespace NightEscape
{
    public class NE_UIMainMenuRoot : NE_BaseUI
    {
        void Start()
        {
            Open(UIPageId.MainMenu);
        }

        protected override void HandleGameState(GameState s)
        {
            switch (s)
            {
                case GameState.Initialize:
                    Open(UIPageId.MainMenu);
                    break;
                default:
                    CloseAll();
                    break;
            }
        }
    }
}
