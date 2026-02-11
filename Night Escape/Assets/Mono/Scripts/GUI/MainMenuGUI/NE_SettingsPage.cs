using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NightEscape
{
    public class NE_SettingsPage : NE_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels SettingPanel;

        [Header("Audio Buttons")]
        [SerializeField] NE_AudioButton musicButton;
        [SerializeField] Button musicToggleBtn;
        [SerializeField] NE_AudioButton sfxButton;
        [SerializeField] Button sfxToggleBtn;

        [SerializeField] NE_UIButton exitBtn;

        bool _musicOn = true;
        bool _sfxOn = true;
        bool _initializedPos;

        NE_BaseUI _parent;
        Vector2 _menuStart;

        NE_AudioManager audioManager;

        public override void Init(NE_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
            exitBtn.Bind(() => _parent.Back());

            if (musicToggleBtn)
            {
                musicToggleBtn.onClick.AddListener(OnMusicToggle);
            }

            if (sfxToggleBtn)
            {
                sfxToggleBtn.onClick.AddListener(OnSfxToggle);
            }

            bool musicOn = PlayerPrefs.GetInt(NE_SafetyKey.KEY_MUSIC_ON, 1) == 1;
            bool sfxOn = PlayerPrefs.GetInt(NE_SafetyKey.KEY_SFX_ON, 1) == 1;

            _musicOn = musicOn;
            _sfxOn = sfxOn;
            UpdateAudioButtonUI();

            base.Init(parent);
        }

        void Start()
        {
            audioManager = NE_AudioManager.Instance;
        }

        void OnDestroy()
        {
            exitBtn.UnBind();

            if (musicToggleBtn)
            {
                musicToggleBtn.onClick.RemoveListener(OnMusicToggle);
            }

            if (sfxToggleBtn)
            {
                sfxToggleBtn.onClick.RemoveListener(OnSfxToggle);
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

        void OnMusicToggle()
        {
            _musicOn = !_musicOn;

            if (audioManager)
            {
                audioManager.SetMusicEnabled(_musicOn);
            }

            UpdateAudioButtonUI();
        }

        void OnSfxToggle()
        {
            _sfxOn = !_sfxOn;

            if (audioManager)
            {
                audioManager.SetSfxEnabled(_sfxOn);
            }

            UpdateAudioButtonUI();
        }

        void UpdateAudioButtonUI()
        {
            if (musicButton)
            {
                musicButton.SetAudioState(_musicOn);
            }

            if (sfxButton)
            {
                sfxButton.SetAudioState(_sfxOn);
            }
        }
    }
}
