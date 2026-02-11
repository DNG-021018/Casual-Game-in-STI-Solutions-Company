using System.Collections;
using UnityEngine;

namespace Bowmancer
{
    public class B_EnemyAnimationController : MonoBehaviour
    {
        private static readonly int IsMovingHash = Animator.StringToHash(B_SafetyKey.ANIM_ENEMY_BLEND_MOVING_BLEND);
        private static readonly int IsDeadHash = Animator.StringToHash(B_SafetyKey.ANIM_ENEMY_TRIGGER_DEAD);
        private static readonly int getHitHash = Animator.StringToHash(B_SafetyKey.ANIM_ENEMY_TRIGGER_GETHIT);
        private static readonly int IsAttackHash = Animator.StringToHash(B_SafetyKey.ANIM_ENEMY_TRIGGER_ATTACK);

        private B_EnemyRef _enemyRef;
        private Animator _animator;
        private bool _isPlayingGetHitAnimation = false;
        private float _getHitAnimationDuration = 0.5f;

        private void Awake()
        {
            _enemyRef = GetComponent<B_EnemyRef>();
            _animator = _enemyRef.Animator;
        }

        public void SetMovingBlend(float blend)
        {
            _animator.SetFloat(IsMovingHash, blend);
        }

        public void PlayDeadAnimation()
        {
            _animator.SetTrigger(IsDeadHash);
        }

        public void PlayGetHitAnimation()
        {
            if (_isPlayingGetHitAnimation) return;

            _animator.SetTrigger(getHitHash);
            _isPlayingGetHitAnimation = true;
            StartCoroutine(ResetGetHitAnimation());
        }

        public void PlayAttackAnimation()
        {
            _animator.SetTrigger(IsAttackHash);
        }

        private IEnumerator ResetGetHitAnimation()
        {
            yield return new WaitForSeconds(_getHitAnimationDuration);
            _isPlayingGetHitAnimation = false;
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
