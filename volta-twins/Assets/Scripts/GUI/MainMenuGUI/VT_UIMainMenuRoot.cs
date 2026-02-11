namespace VoltaTwins
{
    public class VT_UIMainMenuRoot : VT_BaseUI
    {
        void Start() => Open(UIPageId.MainMenu, null, true);

        protected override void HandleGameState(GameState s)
        {
            if (s == GameState.Initialize) Open(UIPageId.MainMenu, null, true);
            else CloseAll();
        }
    }
}
