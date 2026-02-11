using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace CB_CubeRunner
{
    public class CB_Paused : CB_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels SettingPanel;

        [Header("Toggle Sliders")]
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;

        [SerializeField] CB_UIButton exitBtn;

        CB_BaseUI _parent;

        CB_UIToggleSlider _musicToggle;
        CB_UIToggleSlider _sfxToggle;

        const string KEY_MUSIC_ON = "MUSIC_ON";
        const string KEY_SFX_ON = "SFX_ON";

        Vector2 _menuStart;

        CB_AudioManager audioManager;

        public override void Init(CB_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;

            exitBtn.Bind(() =>
            {
                CB_GameManager.Instance?.SetState(GameState.Play);
                _parent.Back();
            });

            if (musicSlider)
            {
                _musicToggle = musicSlider.GetComponent<CB_UIToggleSlider>();
            }

            if (sfxSlider)
            {
                _sfxToggle = sfxSlider.GetComponent<CB_UIToggleSlider>();
            }

            bool musicOn = PlayerPrefs.GetInt(KEY_MUSIC_ON, 1) == 1;
            bool sfxOn = PlayerPrefs.GetInt(KEY_SFX_ON, 1) == 1;

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
            audioManager = CB_AudioManager.Instance;
        }

        void OnDestroy()
        {
            exitBtn.UnBind();
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
