using System;
using UnityEngine;

namespace CB_CubeRunner
{
    public class CB_UIClickSfx : MonoBehaviour
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

            if (CB_AudioManager.Instance != null)
            {
                CB_AudioManager.Instance.PlaySfx(clickClip, volumeScale, after);
                return;
            }

            after?.Invoke();
        }
    }
}
