using System.Collections;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace CataFury
{
    public class CF_EnemyEffect : MonoBehaviour, IPoolableWithInit<CF_EnemyEffect>
    {
        [SerializeField] ParticleSystem[] particleSystems;
        [SerializeField] private AudioClip hitSfx;

        private CF_AudioManager audioManager;
        private Pooler<CF_EnemyEffect> pool;
        private Coroutine _playCoroutine;

        private void Awake()
        {
            if (particleSystems == null || particleSystems.Length == 0)
                particleSystems = GetComponentsInChildren<ParticleSystem>();

            audioManager = ServiceLocator.Get<CF_AudioManager>();
        }

        public void InitPool(Pooler<CF_EnemyEffect> pool)
        {
            this.pool = pool;
        }

        public void OnGetFromPool()
        {
            gameObject.SetActive(true);
        }

        public void OnReturnToPool()
        {
            if (_playCoroutine != null)
            {
                StopCoroutine(_playCoroutine);
                _playCoroutine = null;
            }

            StopAndClearAll();
            gameObject.SetActive(false);
        }

        public void PlayParticleEffectsAt(Vector3 position)
        {
            if (_playCoroutine != null)
                StopCoroutine(_playCoroutine);

            _playCoroutine = StartCoroutine(PlayRoutine(position));
        }

        private IEnumerator PlayRoutine(Vector3 position)
        {
            StopAndClearAll();

            float maxDuration = 0f;

            foreach (var ps in particleSystems)
            {
                if (ps == null) continue;
                ps.transform.position = position;
                ps.Play(true);
                maxDuration = Mathf.Max(maxDuration, ps.main.duration + ps.main.startLifetime.constantMax);
            }

            audioManager?.PlaySfx(hitSfx);

            yield return new WaitForSeconds(maxDuration);

            _playCoroutine = null;
            pool?.ReturnToPool(this);
        }

        private void StopAndClearAll()
        {
            foreach (var ps in particleSystems)
            {
                if (ps != null)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}