using System;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_UIManager : MonoBehaviour
    {
        public event Action<GameState> OnGameStateChanged;

        public void NotifyGameStateChanged(GameState newState)
        {
            OnGameStateChanged?.Invoke(newState);
        }
    }
}
