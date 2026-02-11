using UnityEngine;

namespace Bowmancer
{
    public class B_Trap : MonoBehaviour
    {
        [SerializeField] float Damage = 10f;
        [SerializeField] float ContDownDamageInterval = 1f;
        private float damageCountdown = 0f;

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(B_SafetyKey.TAG_PLAYER))
            {
                damageCountdown -= Time.deltaTime;

                if (damageCountdown <= 0f)
                {
                    B_PlayerController player = other.GetComponent<B_PlayerController>();
                    player.TakeDamage(Damage);
                    damageCountdown = ContDownDamageInterval;
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(B_SafetyKey.TAG_PLAYER))
            {
                damageCountdown = 0f;
            }
        }
    }
}