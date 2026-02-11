using UnityEngine;

namespace HexaStack
{
    [DefaultExecutionOrder(-99)]
    public class HS_AudioManager : MonoBehaviour
    {
        public static HS_AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] AudioSource bgmSource;
        [SerializeField] HS_UISfxPool sfxPool;

        private bool _musicEnabled;
        private bool _sfxEnabled;
        private float _musicVolume;
        private float _sfxVolume;

        public bool MusicEnabled => _musicEnabled;
        public bool SfxEnabled => _sfxEnabled;
        public float MusicVolume => _musicVolume;
        public float SfxVolume => _sfxVolume;

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
            _musicEnabled = PlayerPrefs.GetInt(HS_SafetyKey.KEY_MUSIC_ON, 1) == 1;
            _sfxEnabled = PlayerPrefs.GetInt(HS_SafetyKey.KEY_SFX_ON, 1) == 1;
            _musicVolume = PlayerPrefs.GetFloat(HS_SafetyKey.KEY_MUSIC_VOLUME, 1f);
            _sfxVolume = PlayerPrefs.GetFloat(HS_SafetyKey.KEY_SFX_VOLUME, 1f);
        }

        void ApplySettings()
        {
            ApplyMusicSettings();
        }

        // === MUSIC (BGM) ===
        public void SetMusicEnabled(bool enabled)
        {
            _musicEnabled = enabled;
            PlayerPrefs.SetInt(HS_SafetyKey.KEY_MUSIC_ON, enabled ? 1 : 0);
            PlayerPrefs.Save();
            ApplyMusicSettings();
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(HS_SafetyKey.KEY_MUSIC_VOLUME, _musicVolume);
            PlayerPrefs.Save();
            ApplyMusicSettings();
        }

        void ApplyMusicSettings()
        {
            if (bgmSource)
            {
                bgmSource.mute = !_musicEnabled;
                bgmSource.volume = _musicVolume;

                if (_musicEnabled && !bgmSource.isPlaying)
                {
                    bgmSource.Play();
                }
            }
        }

        public void PlayBgm(AudioClip clip = null, bool loop = true)
        {
            if (!bgmSource) return;

            if (clip != null)
            {
                bgmSource.clip = clip;
            }

            bgmSource.loop = loop;

            if (_musicEnabled)
            {
                bgmSource.Play();
            }
        }

        public void StopBgm()
        {
            if (bgmSource) bgmSource.Stop();
        }

        public void PauseBgm()
        {
            if (bgmSource) bgmSource.Pause();
        }

        public void ResumeBgm()
        {
            if (bgmSource && _musicEnabled) bgmSource.UnPause();
        }

        // === SFX ===
        public void SetSfxEnabled(bool enabled)
        {
            _sfxEnabled = enabled;
            PlayerPrefs.SetInt(HS_SafetyKey.KEY_SFX_ON, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(HS_SafetyKey.KEY_SFX_VOLUME, _sfxVolume);
            PlayerPrefs.Save();
        }

        public void PlaySfx(AudioClip clip, float volumeScale = 1f, System.Action onDone = null)
        {
            if (!_sfxEnabled || !sfxPool || !clip)
            {
                onDone?.Invoke();
                return;
            }
            float finalVolume = _sfxVolume * volumeScale;
            sfxPool.PlayOneShot(clip, finalVolume, onDone);
        }

        public void PlaySfxAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            if (!_sfxEnabled || !clip) return;

            float finalVolume = _sfxVolume * volumeScale;
            AudioSource.PlayClipAtPoint(clip, position, finalVolume);
        }
    }
}