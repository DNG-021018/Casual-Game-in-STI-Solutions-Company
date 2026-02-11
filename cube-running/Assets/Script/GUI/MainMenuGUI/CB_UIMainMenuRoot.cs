
namespace CB_CubeRunner
{
    public class CB_UIMainMenuRoot : CB_BaseUI
    {
        void Start() => Open(UIPageId.MainMenu, null, true);

        protected override void HandleGameState(GameState s)
        {
            if (s == GameState.Initialize)
                Open(UIPageId.MainMenu, null, true);
            else if (s == GameState.Play)
                Open(UIPageId.GamePlay, null, true);
            else if (s == GameState.Paused)
                Open(UIPageId.Pause, null, false);
            else if (s == GameState.FinishGame)
                Open(UIPageId.FinishGame, null, true);
            else
                CloseAll();
        }
    }
}