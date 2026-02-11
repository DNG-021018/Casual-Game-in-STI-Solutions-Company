namespace Bowmancer
{
    public class B_UIGamePlayRoot : B_BaseUI
    {
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
                    Open(UIPageId.GamePlay);
                    break;

                case GameState.Play:
                    if (_stack.Count == 0 || _stack.Peek() != UIPageId.GamePlay)
                    {
                        Open(UIPageId.GamePlay);
                    }
                    break;

                case GameState.Paused:
                    break;

                case GameState.PickupUpgrade:
                    Open(UIPageId.UpgradeMenu);
                    break;

                case GameState.Win:
                    CloseAll();
                    Open(UIPageId.WinGame);
                    break;

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