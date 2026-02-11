using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_PauseMenu : bJakGZQ3_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels MenuPanel;

        [Header("Button")]
        [SerializeField] bJakGZQ3_UIButton resumeButton;
        [SerializeField] bJakGZQ3_UIButton homeButton;
        [SerializeField] bJakGZQ3_UIButton playAgainBtn;

        [Header("Sliders")]
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;

        const string KEY_MUSIC_VOLUME = "MUSIC_VOLUME";
        const string KEY_SFX_VOLUME = "SFX_VOLUME";

        Vector2 _menuStart;
        bool _initializedPos;

        private bJakGZQ3_GameManager _gameManager;
        bJakGZQ3_AudioManager audioManager;

        public override void Init(bJakGZQ3_BaseUI parent)
        {
            _gameManager = bJakGZQ3_GameManager.Instance;

            float savedMusic = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 1f);
            float savedSfx = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);

            if (musicSlider != null)
                musicSlider.value = savedMusic;

            if (sfxSlider != null)
                sfxSlider.value = savedSfx;

            base.Init(parent);
        }

        void Start()
        {
            audioManager = bJakGZQ3_AudioManager.Instance;

            if (resumeButton != null)
            {
                resumeButton.Bind(() =>
                {
                    _gameManager.SetState(GameState.Play);
                });
            }
            if (homeButton != null)
            {
                homeButton.Bind(() =>
                {
                    bJakGZQ3_LoadingScreenRoot.Instance.LoadScene("StartGame");
                });
            }
            if (playAgainBtn != null)
            {
                playAgainBtn.Bind(() =>
                {
                    bJakGZQ3_LoadingScreenRoot.Instance.LoadScene("GamePlay");
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
        }

        void OnSfxSliderChanged(float value)
        {
            if (audioManager) audioManager.SetSfxVolume(value);
        }
    }
}
