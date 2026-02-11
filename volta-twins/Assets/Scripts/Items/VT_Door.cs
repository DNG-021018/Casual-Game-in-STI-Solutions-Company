using UnityEngine;

namespace VoltaTwins
{
    public class VT_Door : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator animator;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;

        private int _openHash;

        private VT_AudioManager audioManager;

        void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            audioManager = VT_AudioManager.Instance;
            _openHash = Animator.StringToHash(VT_SafetyKey.ANIM_DOOR);
        }

        public void SetOpen(bool open)
        {
            if (animator == null) return;

            if (audioManager != null)
            {
                audioManager.PlayerSfxAtTime(open ? openClip : closeClip, 1f);
            }

            animator.SetBool(_openHash, open);
        }
    }
}
