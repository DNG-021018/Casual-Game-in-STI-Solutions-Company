namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_UIMainMenuRoot : bJakGZQ3_BaseUI
    {
        void Start() => Open(UIPageId.MainMenu, null, true);

        protected override void HandleGameState(GameState s)
        {
            if (s == GameState.Initialize) Open(UIPageId.MainMenu, null, true);
            else CloseAll();
        }
    }
}
