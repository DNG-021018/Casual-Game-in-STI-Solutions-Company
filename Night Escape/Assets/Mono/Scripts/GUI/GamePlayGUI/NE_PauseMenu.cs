using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NightEscape
{
    public class NE_PauseMenu : NE_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels MenuPanel;

        [Header("Button")]
        [SerializeField] NE_UIButton resumeButton;
        [SerializeField] NE_UIButton homeButton;
        [SerializeField] NE_UIButton playAgainBtn;

        [Header("Audio Buttons")]
        [SerializeField] NE_AudioButton musicButton;
        [SerializeField] Button musicToggleBtn;
        [SerializeField] NE_AudioButton sfxButton;
        [SerializeField] Button sfxToggleBtn;

        [Header("Optional targets")]
        [SerializeField] AudioSource bgmAudio;

        Vector2 _menuStart;
        bool _initializedPos;

        bool _musicOn = true;
        bool _sfxOn = true;

        private NE_GameManager _gameManager;
        NE_AudioManager audioManager;

        public override void Init(NE_BaseUI parent)
        {
            _gameManager = NE_GameManager.Instance;

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
                    _gameManager.SetState(GameState.Initialize);
                    NE_LoadingScreenRoot.Instance.LoadScene("MainMenu");
                });
            }

            if (playAgainBtn != null)
            {
                playAgainBtn.Bind(() =>
                {
                    NE_GameManager.Instance.LoadLevelScene(NE_GameManager.Instance.CurrentLevel);
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

            Vector2 to = GetOffscreenPos(MenuPanel.panel, MenuPanel.slideDir, _menuStart, offscreenPadding);

            yield return HideMovePanels(
                duration, hideEase, 1f, 0f,
                (MenuPanel.panel, _menuStart, to)
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
