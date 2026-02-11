using UnityEngine;

namespace VoltaTwins
{
    public class VT_PlayerAnimationController : MonoBehaviour
    {
        private Animator animator;

        void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        public void SetMoveSpeed(float speed)
        {
            animator.SetFloat(VT_SafetyKey.ANIM_SPEED, speed);
        }

        public void TriggerShoot()
        {
            animator.SetTrigger(VT_SafetyKey.ANIM_SHOOT);
        }

        public void SetDeadState(bool state)
        {
            animator.SetBool(VT_SafetyKey.ANIM_DEAD, state);
        }
    }
}
