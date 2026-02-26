using UnityEngine;

namespace DoublesideZ
{
    public class DZ_PlayerAnimationController : MonoBehaviour
    {
        private Animator _animator;

        readonly int key_attack = Animator.StringToHash(DZ_SafetyKey.ANIM_TRIGGER_ATTACK);
        readonly int key_dead = Animator.StringToHash(DZ_SafetyKey.ANIM_TRIGGER_DEAD);

        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        public void PlayAttackAnimation()
        {
            _animator.SetTrigger(key_attack);
        }

        public void PlayDeathAnimation()
        {
            _animator.SetTrigger(key_dead);
        }

        public void ResetAnimation()
        {
            _animator.ResetTrigger(key_attack);
            _animator.ResetTrigger(key_dead);
            _animator.Rebind();
            _animator.Update(0f);
        }
    }
}
