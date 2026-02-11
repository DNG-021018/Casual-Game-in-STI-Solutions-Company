using UnityEngine;

namespace VertiblockPass
{
    public class VP_TileSplit : VP_TilesBase
    {
        [SerializeField] private Transform spawnA;
        [SerializeField] private Transform spawnB;

        [SerializeField] private AudioClip hitButton;

        private VP_AudioManager audioManager;

        void Start()
        {
            if (audioManager == null)
            {
                audioManager = VP_AudioManager.Instance;
            }
        }

        public override void HandleCubeEnter(VP_PlayerController player, VP_PlayerState state)
        {
            if (!state.IsStanding) return;
            if (VP_SplitManager.Instance != null && VP_SplitManager.Instance.IsSplit) return;

            if (audioManager != null && hitButton != null)
            {
                audioManager.PlaySfx(hitButton);
            }

            if (VP_SplitManager.Instance != null)
            {
                VP_SplitManager.Instance.Split(player, spawnA.position, spawnB.position);
            }
        }
    }
}
