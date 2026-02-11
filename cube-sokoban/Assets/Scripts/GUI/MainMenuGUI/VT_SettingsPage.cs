using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CubeSokoban
{
    public class CS_SettingsPage : CS_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels SettingPanel;

        [Header("Sliders")]
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;

        [SerializeField] CS_UIButton exitBtn;

        [Header("Optional targets")]
        [SerializeField] AudioSource bgmAudio;

        CS_UIToggleSlider _musicToggle;
        CS_UIToggleSlider _sfxToggle;

        bool _initializedPos;

        CS_BaseUI _parent;
        Vector2 _menuStart;

        CS_AudioManager audioManager;

        public override void Init(CS_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
            exitBtn.Bind(() => _parent.Back());

            if (musicSlider)
            {
                _musicToggle = musicSlider.GetComponent<CS_UIToggleSlider>();
                _musicToggle.onToggleOn.AddListener(OnMusicOn);
                _musicToggle.onToggleOff.AddListener(OnMusicOff);
            }

            if (sfxSlider)
            {
                _sfxToggle = sfxSlider.GetComponent<CS_UIToggleSlider>();
                _sfxToggle.onToggleOn.AddListener(OnSfxOn);
                _sfxToggle.onToggleOff.AddListener(OnSfxOff);
            }

            bool musicOn = PlayerPrefs.GetInt(CS_SafetyKey.KEY_MUSIC_ON, 1) == 1;
            bool sfxOn = PlayerPrefs.GetInt(CS_SafetyKey.KEY_SFX_ON, 1) == 1;

            if (_musicToggle)
            {
                _musicToggle.Initialize();
                _musicToggle.ToggleByGroupManager(musicOn);
            }
            if (_sfxToggle)
            {
                _sfxToggle.Initialize();
                _sfxToggle.ToggleByGroupManager(sfxOn);
            }

            base.Init(parent);
        }

        void Start()
        {
            audioManager = CS_AudioManager.Instance;
        }

        void OnDestroy()
        {
            exitBtn.UnBind();

            if (musicSlider)
            {
                _musicToggle.onToggleOn.RemoveListener(OnMusicOn);
                _musicToggle.onToggleOff.RemoveListener(OnMusicOff);
            }

            if (sfxSlider)
            {
                _sfxToggle.onToggleOn.RemoveListener(OnSfxOn);
                _sfxToggle.onToggleOff.RemoveListener(OnSfxOff);
            }
        }

        protected override void CacheStartPositions()
        {
            if (_initializedPos) return;
            if (SettingPanel.panel) _menuStart = SettingPanel.panel.anchoredPosition;
            _initializedPos = true;
        }

        public override IEnumerator Show(object ctx = null)
        {
            CacheStartPositions();

            Vector2 from = GetOffscreenPos(SettingPanel.panel, SettingPanel.slideDir, _menuStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (SettingPanel.panel, from, _menuStart)
            );
        }

        public override IEnumerator Hide()
        {
            CacheStartPositions();

            Vector2 to = GetOffscreenPos(SettingPanel.panel, SettingPanel.slideDir, _menuStart, offscreenPadding);

            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (SettingPanel.panel, _menuStart, to)
            );
        }

        public void OnMusicOn()
        {
            if (audioManager) audioManager.SetMusicEnabled(true);
        }

        public void OnMusicOff()
        {
            if (audioManager) audioManager.SetMusicEnabled(false);
        }

        public void OnSfxOn()
        {
            if (audioManager) audioManager.SetSfxEnabled(true);
        }

        public void OnSfxOff()
        {
            if (audioManager) audioManager.SetSfxEnabled(false);
        }
    }
}
