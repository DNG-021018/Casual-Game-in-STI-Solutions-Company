using System;
using UnityEngine;

namespace CataFury
{
    public class CF_UIClickSfx : MonoBehaviour
    {
        [Header("Clip")]
        [SerializeField] AudioClip clickClip;
        [Range(0f, 2f)][SerializeField] float volumeScale = 1f;

        public void SetClip(AudioClip clip)
        {
            clickClip = clip;
        }

        public void Play(Action after = null)
        {
            if (clickClip == null)
            {
                after?.Invoke();
                return;
            }

            CF_AudioManager audioManager = ServiceLocator.Get<CF_AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySfx(clickClip, volumeScale, after);
                return;
            }

            after?.Invoke();
        }
    }
}