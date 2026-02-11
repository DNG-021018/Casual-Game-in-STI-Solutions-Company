using System.Collections;
using UnityEngine;

namespace NightEscape
{
    public class NE_UIGamePlayRoot : NE_BaseUI
    {
        void Start()
        {
            NE_GameManager.Instance.GameStart();
        }

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
                    CloseAll();
                    StartCoroutine(OpenUIWithDelay(UIPageId.WinGame, null, 3.2f));
                    break;
                case GameState.Lose:
                    CloseAll();
                    StartCoroutine(OpenUIWithDelay(UIPageId.LoseGame, null, 4.2f));
                    break;
                default:
                    CloseAll();
                    break;
            }
        }

        IEnumerator OpenUIWithDelay(UIPageId id, object ctx, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            Open(id, ctx);
        }
    }
}
