using UnityEngine;

namespace VertiblockPass
{
    public abstract class VP_TilesBase : MonoBehaviour
    {
        public abstract void HandleCubeEnter(VP_PlayerController player, VP_PlayerState state);
        // public virtual void HandleCubeExit(VP_PlayerMovement player, VP_PlayerState state) { }
    }
}
