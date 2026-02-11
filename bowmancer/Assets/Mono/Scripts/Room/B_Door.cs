using UnityEngine;

namespace Bowmancer
{
    public class B_Door : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        [Header("Door Colliders")]
        [SerializeField] private Collider[] doorColliders;

        private bool isOpen;

        private readonly int openHash = Animator.StringToHash(B_SafetyKey.ANIM_DOOR_TRIGGER_OPEN);
        private readonly int closeHash = Animator.StringToHash(B_SafetyKey.ANIM_DOOR_TRIGGER_CLOSE);

        void Awake()
        {
            if (!animator) animator = GetComponent<Animator>();
        }

        public void OpenDoor()
        {
            if (isOpen) return;

            isOpen = true;
            animator.SetTrigger(openHash);

            SetDoorColliders(false);
        }

        public void CloseDoor()
        {
            if (!isOpen) return;

            isOpen = false;
            animator.SetTrigger(closeHash);

            SetDoorColliders(true);
        }

        private void SetDoorColliders(bool enabled)
        {
            if (doorColliders == null) return;

            foreach (var col in doorColliders)
            {
                if (col != null)
                    col.enabled = enabled;
            }
        }

        public bool IsOpen() => isOpen;
    }
}
