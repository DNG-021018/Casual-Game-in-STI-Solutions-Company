using System.Collections;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace Bowmancer
{
    public class B_CoinVFX : MonoBehaviour, IPoolableWithInit<B_CoinVFX>
    {
        ParticleSystem particleSystems;
        Pooler<B_CoinVFX> pool;
        Pooler<B_Coin> coinPool;
        float maxDuration = 0f;

        [Header("Coin Spawn Settings")]
        [SerializeField] private int minCoinCount = 2;
        [SerializeField] private int maxCoinCount = 3;
        [SerializeField] private float spawnRadius = 2f;

        void Awake()
        {
            particleSystems = GetComponent<ParticleSystem>();
            particleSystems.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (particleSystems.main.duration > maxDuration)
            {
                maxDuration = particleSystems.main.duration;
            }
        }

        public void InitPool(Pooler<B_CoinVFX> pool)
        {
            this.pool = pool;
        }

        public void InitCoinPool(Pooler<B_Coin> pool)
        {
            this.coinPool = pool;
        }

        public void OnGetFromPool()
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            particleSystems.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystems.Play();
            StartCoroutine(ReturnToPoolAfterEffect());
        }

        public void OnReturnToPool()
        {
            gameObject.SetActive(false);
            pool.ReturnToPool(this);
        }

        IEnumerator ReturnToPoolAfterEffect()
        {
            yield return new WaitForSeconds(maxDuration / 2);

            if (coinPool != null)
            {
                SpawnCoins();
            }

            yield return new WaitForSeconds(maxDuration / 2);
            OnReturnToPool();
        }

        private void SpawnCoins()
        {
            int coinCount = Random.Range(minCoinCount, maxCoinCount);

            for (int i = 0; i < coinCount; i++)
            {
                Vector3 randomPos = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = transform.position + new Vector3(randomPos.x, 0, randomPos.y);

                B_Coin coin = coinPool.GetRandom(spawnPos, Quaternion.identity);
                if (coin == null) continue;
                coin.InitPool(coinPool);
                coin.OnGetFromPool();
            }
        }
    }
}
