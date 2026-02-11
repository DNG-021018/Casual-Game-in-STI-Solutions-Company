using System;
using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    [DefaultExecutionOrder(-2)]
    public class bJakGZQ3_AudioManager : MonoBehaviour
    {
        public static bJakGZQ3_AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] AudioSource bgmSource;
        [SerializeField] bJakGZQ3_UISfxPool sfxPool;

        const string KEY_MUSIC_VOLUME = "MUSIC_VOLUME";
        const string KEY_SFX_VOLUME = "SFX_VOLUME";

        private float _musicVolume;
        private float _sfxVolume;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSettings();
            ApplySettings();
        }

        void LoadSettings()
        {
            _musicVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 1f);
            _sfxVolume = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);
        }

        void ApplySettings()
        {
            ApplyMusicSettings();
        }

        #region MUSIC 
        // === MUSIC (BGM) ===
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, _musicVolume);
            PlayerPrefs.Save();
            ApplyMusicSettings();
        }

        void ApplyMusicSettings()
        {
            if (bgmSource)
            {
                bgmSource.volume = _musicVolume;

                if (!bgmSource.isPlaying)
                {
                    bgmSource.Play();
                }
            }
        }
        #endregion

        #region SOUND
        // === SFX ===
        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(KEY_SFX_VOLUME, _sfxVolume);
            PlayerPrefs.Save();
        }

        public void PlaySfx(AudioClip clip, float volumeScale = 1f, System.Action onDone = null)
        {
            if (!sfxPool || !clip)
            {
                onDone?.Invoke();
                return;
            }
            float finalVolume = _sfxVolume * volumeScale;
            sfxPool.PlayOneShot(clip, finalVolume, onDone);
        }

        internal void PlaySfx(object clip)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}