using System;
using UnityEngine;

namespace CubeSokoban
{
    public class CS_UIClickSfx : MonoBehaviour
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

            if (CS_AudioManager.Instance != null)
            {
                CS_AudioManager.Instance.PlaySfx(clickClip, volumeScale, after);
                return;
            }

            after?.Invoke();
        }
    }
}
