using System;

namespace Bowmancer
{
    public class B_UIManager : Singleton<B_UIManager>
    {
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnCoinChanged;

        public void NotifyGameStateChanged(GameState newState)
        {
            OnGameStateChanged?.Invoke(newState);
        }

        public void NotifyCoinChanged(int newCoin)
        {
            OnCoinChanged?.Invoke(newCoin);
        }
    }
}
