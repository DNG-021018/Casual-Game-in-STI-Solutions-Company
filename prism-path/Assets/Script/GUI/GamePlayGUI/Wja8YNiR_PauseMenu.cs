using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_PauseMenu : Wja8YNiR_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels MenuPanel;

        [Header("Button")]
        [SerializeField] Wja8YNiR_UIButton resumeButton;
        [SerializeField] Wja8YNiR_UIButton homeButton;
        [SerializeField] Wja8YNiR_UIButton playAgainBtn;

        [Header("Toggle Sliders")]
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;

        [Header("Optional targets")]
        [SerializeField] AudioSource bgmAudio;

        Wja8YNiR_UIToggleSlider _musicToggle;
        Wja8YNiR_UIToggleSlider _sfxToggle;

        const string KEY_MUSIC_ON = "MUSIC_ON";
        const string KEY_SFX_ON = "SFX_ON";

        Vector2 _menuStart;
        bool _initializedPos;

        private Wja8YNiR_GameManager _gameManager;
        Wja8YNiR_AudioManager audioManager;

        public override void Init(Wja8YNiR_BaseUI parent)
        {
            _gameManager = Wja8YNiR_GameManager.Instance;
            if (musicSlider)
            {
                _musicToggle = musicSlider.GetComponent<Wja8YNiR_UIToggleSlider>();
            }

            if (sfxSlider)
            {
                _sfxToggle = sfxSlider.GetComponent<Wja8YNiR_UIToggleSlider>();
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
            audioManager = Wja8YNiR_AudioManager.Instance;

            if (resumeButton != null)
            {
                resumeButton.Bind(() =>
                {
                    Wja8YNiR_LevelManager.Instance.ContinueCountdown();
                    Wja8YNiR_LevelManager.Instance.GameStart();
                    _gameManager.SetState(GameState.Playing);
                });
            }
            if (homeButton != null)
            {
                homeButton.Bind(() =>
                {
                    // SceneManager.LoadScene("StartGame");
                    Wja8YNiR_LoadingScreenRoot.Instance.LoadScene("StartGame");
                });
            }
            if (playAgainBtn != null)
            {
                playAgainBtn.Bind(() =>
                {
                    // SceneManager.LoadScene("StartGame");
                    Wja8YNiR_LoadingScreenRoot.Instance.LoadScene("GamePlay");
                });
            }
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
            Wja8YNiR_LevelManager.Instance.PauseCountdown();
            Vector2 from = GetOffscreenPos(MenuPanel.panel, MenuPanel.slideDir, _menuStart, offscreenPadding);

            yield return ShowMovePanels(
                duration, showEase, 0f, 1f,
                (MenuPanel.panel, from, _menuStart)
            );
        }

        public override IEnumerator Hide()
        {
            CacheStartPositions();

            _gameManager.SetState(GameState.Playing);

            Vector2 to = GetOffscreenPos(MenuPanel.panel, MenuPanel.slideDir, _menuStart, offscreenPadding);

            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (MenuPanel.panel, _menuStart, to)
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
