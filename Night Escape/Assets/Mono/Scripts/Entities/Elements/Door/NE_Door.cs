using UnityEngine;

namespace NightEscape
{
    public class NE_Door : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator animator;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip openClip;

        [Header("Camera Goal Trigger")]
        [SerializeField] private GameObject goalsTrigger;

        private BoxCollider boxCollider;

        private string openTrigger = NE_SafetyKey.ANIM_DOOR_TRIGGER_OPEN;

        // private VT_AudioManager audioManager;

        void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            // audioManager = VT_AudioManager.Instance;

            boxCollider = GetComponent<BoxCollider>();
            if (goalsTrigger != null) goalsTrigger.SetActive(false);
        }

        public void Open()
        {
            boxCollider.enabled = false;
            if (goalsTrigger != null) goalsTrigger.SetActive(true);

            // if (audioManager != null)
            // {
            //     audioManager.PlaySfx(open ? openClip : closeClip);
            // }

            if (animator == null) return;

            animator.SetTrigger(openTrigger);
        }
    }
}
