using System.Collections;
using UnityEngine;

namespace NightEscape
{
    public class NE_DogController : NE_AEnemy
    {
        [Header("Bark Settings")]
        [SerializeField] private ParticleSystem barkParticles;
        [SerializeField] private float barkDuration = 2f;
        [SerializeField] private AudioClip barkClip;

        private ParticleSystem[] _childParticleSystems;
        private Coroutine _barkCoroutine;
        private NE_AudioManager _audioManager => NE_AudioManager.Instance;

        private void Start()
        {
            InitializeParticles();
        }

        private void InitializeParticles()
        {
            if (barkParticles == null)
            {
                barkParticles = GetComponentInChildren<ParticleSystem>();
            }

            if (barkParticles != null)
            {
                _childParticleSystems = barkParticles.GetComponentsInChildren<ParticleSystem>();
            }
            else
            {
                _childParticleSystems = new ParticleSystem[0];
            }
        }

        public void Bark()
        {
            if (barkParticles == null) return;

            if (_barkCoroutine != null)
            {
                StopCoroutine(_barkCoroutine);
            }

            PlayBarkParticles();
            PlayBarkSound();

            _barkCoroutine = StartCoroutine(StopBarkingAfterDelay(barkDuration));
        }

        private void PlayBarkParticles()
        {
            if (barkParticles == null || _childParticleSystems == null || _childParticleSystems.Length == 0)
            {
                return;
            }

            barkParticles.Stop();
            foreach (ParticleSystem ps in _childParticleSystems)
            {
                ps.Stop();
            }

            barkParticles.Play();
            foreach (ParticleSystem ps in _childParticleSystems)
            {
                ps.Play();
            }
        }

        private void PlayBarkSound()
        {
            if (barkClip != null)
            {
                _audioManager.SetBgmVolume();
                _audioManager.PlaySfxWithDuration(barkClip, 2.5f, 1);
            }
        }

        private IEnumerator StopBarkingAfterDelay(float barkDuration)
        {
            yield return new WaitForSeconds(barkDuration);
            StopBarkParticles();
        }

        private void StopBarkParticles()
        {
            if (barkParticles == null || _childParticleSystems == null || _childParticleSystems.Length == 0)
            {
                return;
            }

            foreach (ParticleSystem ps in _childParticleSystems)
            {
                if (ps != null)
                {
                    ParticleSystem.MainModule main = ps.main;
                    main.loop = false;
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            ParticleSystem.MainModule mainBark = barkParticles.main;
            mainBark.loop = false;
            barkParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public override void CaughtPlayer()
        {
            base.CaughtPlayer();
            Bark();
        }

        public override void CaughtPlayer(NE_PlayerController player)
        {
            base.CaughtPlayer(player);
            Bark();
        }
    }
}
