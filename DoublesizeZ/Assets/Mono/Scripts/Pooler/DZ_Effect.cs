using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_Effect : MonoBehaviour, IPoolableWithInit<DZ_Effect>
    {
        ParticleSystem ps;
        ParticleSystem[] child_ps;
        Pooler<DZ_Effect> pool;
        [SerializeField] AudioClip audioClip;

        DZ_AudioManager _audioManager;

        void Awake()
        {
            ps = GetComponent<ParticleSystem>();
            child_ps = GetComponentsInChildren<ParticleSystem>();
            _audioManager = ServiceLocator.Get<DZ_AudioManager>();
            ClearEffect();
        }

        public void InitPool(Pooler<DZ_Effect> pool)
        {
            this.pool = pool;
        }

        public void OnGetFromPool()
        {
            ClearEffect();
        }

        public void OnReturnToPool()
        {
            pool.ReturnToPool(this);
        }

        public void PlayEffect(Transform pos)
        {
            transform.position = pos.position;
            ps.Play();
            if (audioClip != null)
            {
                _audioManager.PlaySfx(audioClip, 4f);
            }
            Invoke(nameof(OnReturnToPool), ps.main.duration);
        }

        private void ClearEffect()
        {
            ClearParticle(ps);
            foreach (var child in child_ps)
            {
                ClearParticle(child);
            }
        }

        private void ClearParticle(ParticleSystem ps)
        {
            ps.Stop();
            ps.Clear();
        }
    }
}
