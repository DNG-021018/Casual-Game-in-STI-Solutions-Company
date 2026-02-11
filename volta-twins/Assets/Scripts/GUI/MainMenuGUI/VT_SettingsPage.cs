using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoltaTwins
{
    public class VT_SettingsPage : VT_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels SettingPanel;

        [Header("TMP")]
        [SerializeField] TextMeshProUGUI musicValueText;
        [SerializeField] TextMeshProUGUI vfxValueText;

        [Header("Sliders")]
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;

        [SerializeField] VT_UIButton exitBtn;

        [Header("Optional targets")]
        [SerializeField] AudioSource bgmAudio;

        VT_BaseUI _parent;

        Vector2 _menuStart;

        VT_AudioManager audioManager;

        public override void Init(VT_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;
            exitBtn.Bind(() => _parent.Back());

            float savedMusic = PlayerPrefs.GetFloat(VT_SafetyKey.KEY_MUSIC_VOLUME, 1f);
            float savedSfx = PlayerPrefs.GetFloat(VT_SafetyKey.KEY_SFX_VOLUME, 1f);

            if (musicSlider != null)
                musicSlider.value = savedMusic;

            if (sfxSlider != null)
                sfxSlider.value = savedSfx;

            if (musicValueText != null)
                musicValueText.text = Mathf.RoundToInt(savedMusic * 100).ToString() + "%";

            if (vfxValueText != null)
                vfxValueText.text = Mathf.RoundToInt(savedSfx * 100).ToString() + "%";

            base.Init(parent);
        }

        void Start()
        {
            audioManager = VT_AudioManager.Instance;

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
            if (musicValueText != null)
                musicValueText.text = Mathf.RoundToInt(value * 100).ToString() + "%";
        }

        void OnSfxSliderChanged(float value)
        {
            if (audioManager) audioManager.SetSfxVolume(value);
            if (vfxValueText != null)
                vfxValueText.text = Mathf.RoundToInt(value * 100).ToString() + "%";
        }
    }
}
