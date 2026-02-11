using System;
using UnityEngine;

namespace Bowmancer
{
    public class B_UIClickSfx : MonoBehaviour
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

            if (B_AudioManager.Instance != null)
            {
                B_AudioManager.Instance.PlaySfx(clickClip, volumeScale, after);
                return;
            }

            after?.Invoke();
        }
    }
}
