using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CubeSokoban
{
    public class CS_PauseMenu : CS_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels MenuPanel;

        [Header("Button")]
        [SerializeField] CS_UIButton resumeButton;
        [SerializeField] CS_UIButton homeButton;
        [SerializeField] CS_UIButton playAgainBtn;

        [Header("Sliders")]
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;

        [Header("Optional targets")]
        [SerializeField] AudioSource bgmAudio;

        Vector2 _menuStart;
        bool _initializedPos;

        CS_UIToggleSlider _musicToggle;
        CS_UIToggleSlider _sfxToggle;

        private CS_GameManager _gameManager;
        CS_AudioManager audioManager;

        public override void Init(CS_BaseUI parent)
        {
            _gameManager = CS_GameManager.Instance;

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

            if (resumeButton != null)
            {
                resumeButton.Bind(() =>
                {
                    CS_LevelManager.Instance.GameStart();
                    _gameManager.SetState(GameState.Play);
                });
            }
            if (homeButton != null)
            {
                homeButton.Bind(() =>
                {
                    _gameManager.SetState(GameState.Initialize);
                    CS_LoadingScreenRoot.Instance.LoadScene("StartGame");
                });
            }
            if (playAgainBtn != null)
            {
                playAgainBtn.Bind(() =>
                {
                    _gameManager.SetState(GameState.Play);
                    CS_LoadingScreenRoot.Instance.LoadScene("GamePlay");
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
