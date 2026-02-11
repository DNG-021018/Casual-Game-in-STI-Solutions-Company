using UnityEngine;

namespace NightEscape
{
    public abstract class NE_AEnemy : MonoBehaviour
    {
        public virtual void CaughtPlayer()
        {
            NE_GameManager.Instance.SetState(GameState.Lose);
        }

        public virtual void CaughtPlayer(NE_PlayerController player)
        {
            NE_GameManager.Instance.SetState(GameState.Lose);
        }
    }
}
