using System.Collections;
using UnityEngine;

namespace VoltaTwins
{
    public class VT_UIGamePlayRoot : VT_BaseUI
    {
        void Start()
        {
            Open(UIPageId.GamePlay, null, true);
        }

        protected override void HandleGameState(GameState s)
        {
            switch (s)
            {
                case GameState.Play:
                    Open(UIPageId.GamePlay);
                    break;
                case GameState.Paused:
                    Open(UIPageId.Pause);
                    break;
                case GameState.Win:
                    StartCoroutine(WinScreenCoroutin(1f));
                    break;
                // case GameState.Lose:
                //     Open(UIPageId.LoseGame);
                //     break;
                default:
                    CloseAll();
                    break;
            }
        }

        IEnumerator WinScreenCoroutin(float sec)
        {
            yield return new WaitForSecondsRealtime(sec);
            Open(UIPageId.WinGame);
        }
    }
}
