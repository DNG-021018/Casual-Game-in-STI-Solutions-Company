using UnityEngine;

namespace VertiblockPass
{
    public class VP_TileO : VP_TilesBase
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
            if (audioManager != null && hitButton != null)
            {
                audioManager.PlaySfx(hitButton);
            }
            SetTargets(isClose ? !isClose : true);
        }

        private void SetTargets(bool value)
        {
            if (targets == null) return;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                {
                    targets[i].ToggleBridge(value);
                }
            }
        }
    }
}
