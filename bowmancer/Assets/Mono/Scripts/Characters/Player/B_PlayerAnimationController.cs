using UnityEngine;

namespace Bowmancer
{
    public class B_PlayerAnimationController : MonoBehaviour
    {
        private B_PlayerRef _playerRef;
        private Animator _animator;

        private static readonly int IsMovingHash = Animator.StringToHash(B_SafetyKey.ANIM_PLAYER_BLEND_MOVING);
        private static readonly int IsShootingHash = Animator.StringToHash(B_SafetyKey.ANIM_PLAYER_BOOL_SHOOT);
        private static readonly int VelocityXHash = Animator.StringToHash(B_SafetyKey.ANIM_PLAYER_BLEND_SHOOTING_VELOCITY_X);
        private static readonly int VelocityYHash = Animator.StringToHash(B_SafetyKey.ANIM_PLAYER_BLEND_SHOOTING_VELOCITY_Y);
        private static readonly int IsDeadHash = Animator.StringToHash(B_SafetyKey.ANIM_PLAYER_TRIGGER_DEAD);

        private void Start()
        {
            _playerRef = GetComponent<B_PlayerRef>();
            _animator = _playerRef.Animator;
        }

        public void SetMovingBlend(float blend)
        {
            _animator.SetFloat(IsMovingHash, blend);
        }

        public void SetShootingVelocity(float velocityX, float velocityY)
        {
            _animator.SetFloat(VelocityXHash, velocityX);
            _animator.SetFloat(VelocityYHash, velocityY);
        }

        public void PlayShootingAnimation(bool value)
        {
            _animator.SetBool(IsShootingHash, value);
        }

        public bool IsShooting()
        {
            return _animator.GetBool(IsShootingHash);
        }

        public void PlayDeadAnimation()
        {
            _animator.SetTrigger(IsDeadHash);
        }
    }
}
