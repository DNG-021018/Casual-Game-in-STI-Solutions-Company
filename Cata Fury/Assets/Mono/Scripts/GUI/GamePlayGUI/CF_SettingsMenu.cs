using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CataFury
{
    public class CF_SettingsMenu : CF_UIPage
    {
        [Header("Tween")]
        [SerializeField] Panels settingsPanel;
        private Vector2 _originalPanelPos;

        [Header("Button")]
        [SerializeField] CF_UIButton closeButton;

        [SerializeField] CF_AudioButton musicButton;
        [SerializeField] Slider musicSlider;

        [SerializeField] CF_AudioButton sfxButton;
        [SerializeField] Slider sfxSlider;

        [SerializeField] bool isPauseMenu = true;

        private float _lastMusicVolume = 1f;
        private float _lastSfxVolume = 1f;
        private bool _musicOn = true;
        private bool _sfxOn = true;
        private bool _isUpdatingSlider = false;

        private CF_AudioManager _audioManager;
        private CF_BaseUI parent;

        void Awake()
        {
            _audioManager = ServiceLocator.Get<CF_AudioManager>();
        }

        public override void Init(CF_BaseUI parent)
        {
            base.Init(parent);
            this.parent = parent;
            CacheStartPositions();
        }

        private void RefreshAudioState()
        {
            if (_audioManager != null)
            {
                _musicOn = _audioManager.MusicEnabled;
                _sfxOn = _audioManager.SfxEnabled;
                _lastMusicVolume = _audioManager.MusicVolume > 0f ? _audioManager.MusicVolume : 1f;
                _lastSfxVolume = _audioManager.SfxVolume > 0f ? _audioManager.SfxVolume : 1f;
            }
            else
            {
                _musicOn = PlayerPrefs.GetInt(CF_SafetyKey.Data.KEY_PLAYPREF_MUSIC_ON, 1) == 1;
                _sfxOn = PlayerPrefs.GetInt(CF_SafetyKey.Data.KEY_PLAYPREF_SFX_ON, 1) == 1;
            }

            SyncSliders();
            UpdateAudioButtonUI();
        }

        private void SyncSliders()
        {
            _isUpdatingSlider = true;

            if (musicSlider != null)
            {
                musicSlider.minValue = 0f;
                musicSlider.maxValue = 1f;
                musicSlider.value = _audioManager != null
                    ? _audioManager.MusicVolume
                    : PlayerPrefs.GetFloat(CF_SafetyKey.Data.KEY_PLAYPREF_MUSIC_VOLUME, 1f);
            }

            if (sfxSlider != null)
            {
                sfxSlider.minValue = 0f;
                sfxSlider.maxValue = 1f;
                sfxSlider.value = _audioManager != null
                    ? _audioManager.SfxVolume
                    : PlayerPrefs.GetFloat(CF_SafetyKey.Data.KEY_PLAYPREF_SFX_VOLUME, 1f);
            }

            _isUpdatingSlider = false;
        }

        private void OnEnable()
        {
            if (closeButton != null)
                closeButton.Bind(() =>
                {
                    if (isPauseMenu)
                        CF_GameManager.Instance.ResumeGame();
                    else
                        parent.Back();
                });

            if (musicButton) musicButton.Bind(OnMusicToggle);
            if (sfxButton) sfxButton.Bind(OnSfxToggle);
            if (musicSlider) musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.UnBind();
            if (musicButton) musicButton.UnBind();
            if (sfxButton) sfxButton.UnBind();
            if (musicSlider) musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        }

        protected override void CacheStartPositions()
        {
            if (settingsPanel.panel != null)
                _originalPanelPos = settingsPanel.panel.anchoredPosition;
        }

        public override IEnumerator Show()
        {
            RefreshAudioState();

            yield return ShowScalePanels(
                settingsPanel.duration, settingsPanel.showEase,
                0f, 1f,
                (settingsPanel.panel, Vector3.zero, Vector3.one)
            );
        }

        public override IEnumerator Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.gameObject.SetActive(false);

            if (settingsPanel.panel != null)
                settingsPanel.panel.localScale = Vector3.zero;

            yield break;
        }

        void OnMusicToggle()
        {
            _musicOn = !_musicOn;
            _audioManager?.SetMusicEnabled(_musicOn);

            _isUpdatingSlider = true;
            if (!_musicOn)
            {
                if (musicSlider && musicSlider.value > 0f) _lastMusicVolume = musicSlider.value;
                if (musicSlider) musicSlider.value = 0f;
                _audioManager?.SetMusicVolume(0f);
            }
            else
            {
                float restore = _lastMusicVolume > 0f ? _lastMusicVolume : 1f;
                if (musicSlider) musicSlider.value = restore;
                _audioManager?.SetMusicVolume(restore);
            }
            _isUpdatingSlider = false;
            UpdateAudioButtonUI();
        }

        void OnSfxToggle()
        {
            _sfxOn = !_sfxOn;
            _audioManager?.SetSfxEnabled(_sfxOn);

            _isUpdatingSlider = true;
            if (!_sfxOn)
            {
                if (sfxSlider && sfxSlider.value > 0f) _lastSfxVolume = sfxSlider.value;
                if (sfxSlider) sfxSlider.value = 0f;
                _audioManager?.SetSfxVolume(0f);
            }
            else
            {
                float restore = _lastSfxVolume > 0f ? _lastSfxVolume : 1f;
                if (sfxSlider) sfxSlider.value = restore;
                _audioManager?.SetSfxVolume(restore);
            }
            _isUpdatingSlider = false;
            UpdateAudioButtonUI();
        }

        void OnMusicVolumeChanged(float value)
        {
            if (_isUpdatingSlider) return;
            if (value > 0f)
            {
                _lastMusicVolume = value;
                if (!_musicOn)
                {
                    _musicOn = true;
                    _audioManager?.SetMusicEnabled(true);
                    UpdateAudioButtonUI();
                }
                _audioManager?.SetMusicVolume(value);
            }
            else
            {
                if (_musicOn)
                {
                    _musicOn = false;
                    _audioManager?.SetMusicEnabled(false);
                    UpdateAudioButtonUI();
                }
                _audioManager?.SetMusicVolume(0f);
            }
        }

        void OnSfxVolumeChanged(float value)
        {
            if (_isUpdatingSlider) return;
            if (value > 0f)
            {
                _lastSfxVolume = value;
                if (!_sfxOn)
                {
                    _sfxOn = true;
                    _audioManager?.SetSfxEnabled(true);
                    UpdateAudioButtonUI();
                }
                _audioManager?.SetSfxVolume(value);
            }
            else
            {
                if (_sfxOn)
                {
                    _sfxOn = false;
                    _audioManager?.SetSfxEnabled(false);
                    UpdateAudioButtonUI();
                }
                _audioManager?.SetSfxVolume(0f);
            }
        }

        void UpdateAudioButtonUI()
        {
            if (musicButton) musicButton.SetAudioState(_musicOn);
            if (sfxButton) sfxButton.SetAudioState(_sfxOn);
        }
    }
}
