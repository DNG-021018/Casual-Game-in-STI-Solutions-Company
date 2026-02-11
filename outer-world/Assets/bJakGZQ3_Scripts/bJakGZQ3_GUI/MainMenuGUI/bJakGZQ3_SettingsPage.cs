using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_SettingsPage : bJakGZQ3_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels SettingPanel;

        [Header("Toggle Sliders")]
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;

        [SerializeField] bJakGZQ3_UIButton exitBtn;

        [Header("Optional targets")]
        [SerializeField] AudioSource bgmAudio;

        bJakGZQ3_BaseUI _parent;

        const string KEY_MUSIC_VOLUME = "MUSIC_VOLUME";
        const string KEY_SFX_VOLUME = "SFX_VOLUME";

        Vector2 _menuStart;

        bJakGZQ3_AudioManager audioManager;

        public override void Init(bJakGZQ3_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;

            exitBtn.Bind(() => _parent.Back());

            float savedMusic = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 1f);
            float savedSfx = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);

            if (musicSlider != null)
                musicSlider.value = savedMusic;

            if (sfxSlider != null)
                sfxSlider.value = savedSfx;
        }

        void Start()
        {
            audioManager = bJakGZQ3_AudioManager.Instance;

            if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }


        void OnDestroy()
        {
            exitBtn.UnBind();

            if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
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

        void OnMusicSliderChanged(float value)
        {
            if (audioManager) audioManager.SetMusicVolume(value);
        }

        void OnSfxSliderChanged(float value)
        {
            if (audioManager) audioManager.SetSfxVolume(value);
        }
    }
}
