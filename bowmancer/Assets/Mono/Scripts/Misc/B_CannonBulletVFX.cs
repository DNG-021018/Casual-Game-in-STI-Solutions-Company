using System.Collections;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace Bowmancer
{
    public class B_CannonBulletVFX : MonoBehaviour, IPoolableWithInit<B_CannonBulletVFX>
    {
        ParticleSystem particleSystems;
        Pooler<B_CannonBulletVFX> pool;
        float maxDuration = 0f;

        void Awake()
        {
            particleSystems = GetComponent<ParticleSystem>();
            particleSystems.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (particleSystems.main.duration > maxDuration)
            {
                maxDuration = particleSystems.main.duration;
            }
        }

        public void InitPool(Pooler<B_CannonBulletVFX> pool)
        {
            this.pool = pool;
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
            yield return new WaitForSeconds(maxDuration);
            OnReturnToPool();
        }
    }
}
