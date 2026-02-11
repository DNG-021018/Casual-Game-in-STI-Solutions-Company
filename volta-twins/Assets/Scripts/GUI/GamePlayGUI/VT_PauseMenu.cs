using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoltaTwins
{
    public class VT_PauseMenu : VT_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels MenuPanel;

        [Header("Button")]
        [SerializeField] VT_UIButton resumeButton;
        [SerializeField] VT_UIButton homeButton;
        [SerializeField] VT_UIButton playAgainBtn;

        [Header("TMP")]
        [SerializeField] TextMeshProUGUI musicValueText;
        [SerializeField] TextMeshProUGUI vfxValueText;

        [Header("Sliders")]
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;

        [Header("Optional targets")]
        [SerializeField] AudioSource bgmAudio;

        Vector2 _menuStart;
        bool _initializedPos;

        private VT_GameManager _gameManager;
        VT_AudioManager audioManager;

        public override void Init(VT_BaseUI parent)
        {
            _gameManager = VT_GameManager.Instance;

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

            if (resumeButton != null)
            {
                resumeButton.Bind(() =>
                {
                    VT_LevelManager.Instance.GameStart();
                    _gameManager.SetState(GameState.Play);
                });
            }
            if (homeButton != null)
            {
                homeButton.Bind(() =>
                {
                    _gameManager.SetState(GameState.Initialize);
                    VT_LoadingScreenRoot.Instance.LoadScene("StartGame");
                });
            }
            if (playAgainBtn != null)
            {
                playAgainBtn.Bind(() =>
                {
                    _gameManager.SetState(GameState.Play);
                    VT_LoadingScreenRoot.Instance.LoadScene("GamePlay");
                });
            }

            if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }

        void OnDestroy()
        {
            if (resumeButton != null)
            {
                resumeButton.UnBind();
            }
            if (homeButton != null)
            {
                homeButton.UnBind();
            }
            if (playAgainBtn != null)
            {
                playAgainBtn.UnBind();
            }

            if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
        }

        protected override void CacheStartPositions()
        {
            if (_initializedPos) return;
            if (MenuPanel.panel) _menuStart = MenuPanel.panel.anchoredPosition;
            _initializedPos = true;
        }

        public override IEnumerator Show(object ctx = null)
        {
            _gameManager.SetState(GameState.Paused);
            CacheStartPositions();
            Vector2 from = GetOffscreenPos(MenuPanel.panel, MenuPanel.slideDir, _menuStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (MenuPanel.panel, from, _menuStart)
            );
        }

        public override IEnumerator Hide()
        {
            CacheStartPositions();

            _gameManager.SetState(GameState.Play);

            Vector2 to = GetOffscreenPos(MenuPanel.panel, MenuPanel.slideDir, _menuStart, offscreenPadding);

            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (MenuPanel.panel, _menuStart, to)
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
