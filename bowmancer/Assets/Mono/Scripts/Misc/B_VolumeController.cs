using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Bowmancer
{
    public class B_VolumeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Volume volume;
        [SerializeField] private Animator animator;

        [Header("Health Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float lowHealthThreshold = 0.15f;

        readonly int isHitHash = Animator.StringToHash(B_SafetyKey.ANIM_TRIGGER_HIT);
        readonly int isLowHealthHash = Animator.StringToHash(B_SafetyKey.ANIM_BOOL_LOW_HEALTH);

        public static Action OnHitEvent;
        public static Action<float, float> OnLowHealthEnterEvent;

        private bool _isLowHealth;

        void OnEnable()
        {
            OnHitEvent += OnHit;
            OnLowHealthEnterEvent += WarningLowHealth;
        }

        void OnDisable()
        {
            OnHitEvent -= OnHit;
            OnLowHealthEnterEvent -= WarningLowHealth;
        }

        public void OnHit()
        {
            if (animator == null) return;
            animator.SetTrigger(isHitHash);
        }

        public void WarningLowHealth(float currentHp, float maxHp)
        {
            float percent = currentHp / maxHp;

            if (percent <= lowHealthThreshold && currentHp > 0)
            {
                EnterLowHealth();
            }
            else
            {
                ExitLowHealth();
            }
        }

        private void EnterLowHealth()
        {
            if (_isLowHealth) return;
            _isLowHealth = true;

            animator.SetBool(isLowHealthHash, true);
        }

        private void ExitLowHealth()
        {
            if (!_isLowHealth) return;
            _isLowHealth = false;

            animator.SetBool(isLowHealthHash, false);
        }
    }
}
