using UnityEngine;

namespace CubeSokoban
{
    public class CS_UIGamePlayRoot : CS_BaseUI
    {
        protected override void HandleGameState(GameState s)
        {
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
                    Open(UIPageId.WinGame);
                    break;
                default:
                    CloseAll();
                    break;
            }
        }
    }
}
