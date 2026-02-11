namespace VertiblockPass
{
    public class VP_UIGamePlayRoot : VP_BaseUI
    {
        protected override void HandleGameState(GameState s)
        {
            if (VP_LevelManager.Instance)
            {
                if (VP_LevelManager.Instance.isGameFinish)
                {
                    return;
                }
            }

            switch (s)
            {
                case GameState.InitializeLevel:
                    CloseAll();
                    break;
                case GameState.Play:
                    Open(UIPageId.GamePlay);
                    break;
                case GameState.Paused:
                    Open(UIPageId.Pause);
                    break;
                case GameState.Win:
                case GameState.Lose:
                    Open(UIPageId.EndGameMenu);
                    break;
                default:
                    CloseAll();
                    break;
            }
        }
    }
}
