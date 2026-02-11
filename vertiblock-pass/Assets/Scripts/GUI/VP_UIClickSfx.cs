using System;
using UnityEngine;

namespace VertiblockPass
{
    public class VP_UIClickSfx : MonoBehaviour
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

            if (VP_AudioManager.Instance != null)
            {
                VP_AudioManager.Instance.PlaySfx(clickClip, volumeScale, after);
                return;
            }

            after?.Invoke();
        }
    }
}
