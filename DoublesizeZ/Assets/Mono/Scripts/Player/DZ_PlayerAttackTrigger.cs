using UnityEngine;

namespace DoublesideZ
{
    public class DZ_PlayerAttackTrigger : MonoBehaviour
    {
        private DZ_PlayerController player;
        private DZ_EffectPool effectPool;
        private DZ_FloatingTextPool floatingTextPool;

        private DZ_ScoreManager _scoreManager;
        private DZ_CurrencyManager _currencyManager;

        void Awake()
        {
            player = GetComponentInParent<DZ_PlayerController>();
            effectPool = ServiceLocator.Get<DZ_PoolManager>().EffectPool;
            floatingTextPool = ServiceLocator.Get<DZ_PoolManager>().FloatingTextPool;
        }

        void Start()
        {
            _scoreManager = ServiceLocator.Get<DZ_ScoreManager>();
            _currencyManager = ServiceLocator.Get<DZ_CurrencyManager>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (player == null || player.IsDead) return;
            if (other.CompareTag(DZ_SafetyKey.TAG_ENEMY))
            {
                player.Attack();

                float random = Random.Range(0f, 1f);
                string effectName = random > 0.5f ? "playerHit_1" : "playerHit_2";
                effectPool.Get(effectName, other.transform.position, Quaternion.identity).PlayEffect(other.transform);

                floatingTextPool.GetRandom(other.transform.position, Quaternion.identity).ShowFloatingText("+1", other.transform);

                other.GetComponent<DZ_EnemyController>()?.Death();

                _scoreManager?.AddScore(1);
                _currencyManager?.AddCoins(1);
            }
        }
    }
}
