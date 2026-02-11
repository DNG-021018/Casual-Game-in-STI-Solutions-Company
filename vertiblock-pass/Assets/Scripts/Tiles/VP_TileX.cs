using UnityEngine;

namespace VertiblockPass
{
    public class VP_TileX : VP_TilesBase
    {
        [SerializeField] private VP_Bridge[] targets;
        [SerializeField] private bool isClose;
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
            if (VP_SplitManager.Instance != null && VP_SplitManager.Instance.IsSplit)
            {
                return;
            }

            if (!state.IsStanding)
            {
                return;
            }

            if (audioManager != null && hitButton != null)
            {
                audioManager.PlaySfx(hitButton);
            }

            SetTargets(isClose ? !isClose : true);
        }

        private void SetTargets(bool isOpen)
        {
            if (targets == null || targets.Length == 0) return;

            foreach (var bridge in targets)
            {
                if (bridge != null)
                {
                    bridge.ToggleBridge(isOpen);
                }
            }
        }
    }
}
