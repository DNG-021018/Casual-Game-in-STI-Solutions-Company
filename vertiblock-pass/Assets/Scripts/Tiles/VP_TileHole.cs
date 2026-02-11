using UnityEngine;

namespace VertiblockPass
{
    public class VP_TileHole : VP_TilesBase
    {
        [SerializeField] GameObject holeMask;
        [SerializeField] private AudioClip hitButton;

        private VP_AudioManager audioManager;
        void Start()
        {
            holeMask.SetActive(false);
            if (audioManager == null)
            {
                audioManager = VP_AudioManager.Instance;
            }
        }

        public override void HandleCubeEnter(VP_PlayerController player, VP_PlayerState state)
        {
            // If currently split, don't interact with hole tiles
            if (!state.IsStanding) return;
            if (VP_SplitManager.Instance != null && VP_SplitManager.Instance.IsSplit) return;

            if (audioManager != null && hitButton != null)
            {
                audioManager.PlaySfx(hitButton);
            }
            holeMask.SetActive(true);
            player.FallStraightDown();
        }
    }
}
