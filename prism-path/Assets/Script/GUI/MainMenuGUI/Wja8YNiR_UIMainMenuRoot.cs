namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_UIMainMenuRoot : Wja8YNiR_BaseUI
    {
        void Start() => Open(UIPageId.MainMenu, null, true);

        protected override void HandleGameState(GameState s)
        {
            if (s == GameState.Initialize) Open(UIPageId.MainMenu, null, true);
            else CloseAll();
        }
    }
}
