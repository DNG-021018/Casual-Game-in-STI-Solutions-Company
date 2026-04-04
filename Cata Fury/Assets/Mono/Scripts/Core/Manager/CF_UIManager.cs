using System;
using UnityEngine;

namespace CataFury
{
    public class CF_UIManager : MonoBehaviour
    {
        public event Action<GameState> OnGameStateChanged;

        public void NotifyGameStateChanged(GameState newState)
        {
            OnGameStateChanged?.Invoke(newState);
        }
    }
}
