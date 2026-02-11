using UnityEngine;

namespace NightEscape
{
    public class NE_Trap : NE_AEnemy
    {
        [SerializeField] private AudioClip trapSound;

        private NE_AudioManager _audioManager => NE_AudioManager.Instance;

        public override void CaughtPlayer()
        {
            base.CaughtPlayer();
            if (_audioManager != null && trapSound != null)
            {
                _audioManager.SetBgmVolume();
                _audioManager.PlaySfx(trapSound, 1, () => _audioManager.PlayBgm());
            }
        }

        public override void CaughtPlayer(NE_PlayerController player)
        {
            base.CaughtPlayer(player);
            if (_audioManager != null && trapSound != null)
            {
                _audioManager.SetBgmVolume();
                _audioManager.PlaySfx(trapSound, 1, () => _audioManager.PlayBgm());
            }
        }
    }
}
