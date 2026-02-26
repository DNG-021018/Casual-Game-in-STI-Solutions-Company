using System;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_UIClickSfx : MonoBehaviour
    {
        [Header("Clip")]
        [SerializeField] AudioClip clickClip;
        [Range(0f, 2f)][SerializeField] float volumeScale = 1f;

        public void Play(Action after = null)
        {
            if (clickClip == null)
            {
                after?.Invoke();
                return;
            }

            DZ_AudioManager audioManager = ServiceLocator.Get<DZ_AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySfx(clickClip, volumeScale, after);
                return;
            }

            after?.Invoke();
        }
    }
}
