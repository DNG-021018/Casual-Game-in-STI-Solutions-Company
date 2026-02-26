using UnityEngine;

namespace DoublesideZ
{
    public class DZ_PlayerHitbox : MonoBehaviour
    {
        private DZ_PlayerController player;
        private DZ_EffectPool effectPool;

        void Awake()
        {
            player = GetComponentInParent<DZ_PlayerController>();
            effectPool = ServiceLocator.Get<DZ_PoolManager>().EffectPool;
        }

        void OnTriggerEnter(Collider other)
        {
            if (player == null || player.IsDead) return;
            if (other.CompareTag(DZ_SafetyKey.TAG_ENEMY))
            {
                effectPool.Get("enemyHit", transform.position, Quaternion.identity).PlayEffect(other.transform);
                player.Death();
            }
        }
    }
}
