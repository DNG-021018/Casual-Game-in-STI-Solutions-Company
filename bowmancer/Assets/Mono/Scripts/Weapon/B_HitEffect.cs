using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace Bowmancer
{
    public class B_HitEffect : MonoBehaviour, IPoolableWithInit<B_HitEffect>
    {
        ParticleSystem[] particleSystems;
        Pooler<B_HitEffect> pool;
        float maxDuration = 0f;

        Coroutine coroutine;

        void Awake()
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            foreach (var ps in particleSystems)
            {
                if (ps.main.duration > maxDuration)
                {
                    maxDuration = ps.main.duration;
                }
            }
        }

        public void InitPool(Pooler<B_HitEffect> pool)
        {
            this.pool = pool;
        }

        public void OnGetFromPool()
        {
            coroutine = null;
            gameObject.SetActive(true);

            foreach (var ps in particleSystems)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            foreach (var ps in particleSystems)
            {
                ps.Play();
            }
        }

        public void OnReturnToPool()
        {
            coroutine = StartCoroutine(ReturnToPoolAfterEffect());
        }

        System.Collections.IEnumerator ReturnToPoolAfterEffect()
        {
            yield return new WaitForSeconds(maxDuration);
            pool.ReturnToPool(this);
            gameObject.SetActive(false);
            coroutine = null;
        }
    }
}
