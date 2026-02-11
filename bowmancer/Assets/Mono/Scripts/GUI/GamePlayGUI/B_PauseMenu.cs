using System.Collections;
using UnityEngine;

namespace Bowmancer
{
    public class B_PauseMenu : B_UIPage
    {
        [Header("Button")]
        [SerializeField] B_UIButton resumeButton;
        [SerializeField] B_AudioButton musicButton;
        [SerializeField] B_UIButton musicToggleBtn;
        [SerializeField] B_AudioButton sfxButton;
        [SerializeField] B_UIButton sfxToggleBtn;

        [Header("Audio")]
        [SerializeField] AudioClip pauseClip;

        private bool _musicOn = true;
        private bool _sfxOn = true;

        private B_GameManager _gameManager;
        private B_AudioManager _audioManager;
        private B_BaseUI parent;

        public override void Init(B_BaseUI parent)
        {
            base.Init(parent);
            this.parent = parent;
            _gameManager = B_GameManager.Instance;
            _audioManager = B_AudioManager.Instance;

            bool musicOn = PlayerPrefs.GetInt(B_SafetyKey.KEY_PLAYPREF_MUSIC_ON, 1) == 1;
            bool sfxOn = PlayerPrefs.GetInt(B_SafetyKey.KEY_PLAYPREF_SFX_ON, 1) == 1;

            _musicOn = musicOn;
            _sfxOn = sfxOn;
            UpdateAudioButtonUI();
        }

        private void OnEnable()
        {
            if (resumeButton != null)
            {
                resumeButton.Bind(() =>
                {
                    parent.Back();
                    _gameManager.SetState(GameState.Play);
                });
            }

            if (musicToggleBtn)
            {
                musicToggleBtn.Bind(OnMusicToggle);
            }

            if (sfxToggleBtn)
            {
                sfxToggleBtn.Bind(OnSfxToggle);
            }
        }

        private void OnDisable()
        {
            if (resumeButton != null)
            {
                resumeButton.UnBind();
            }

            if (musicToggleBtn)
            {
                musicToggleBtn.UnBind();
            }

            if (sfxToggleBtn)
            {
                sfxToggleBtn.UnBind();
            }
        }

        public override IEnumerator Show()
        {
            _audioManager.PlaySfx(pauseClip);
            canvasGroup.alpha = 1f;
            yield return base.Show();
            _gameManager.SetState(GameState.Paused);
        }

        public override IEnumerator Hide()
        {
            canvasGroup.alpha = 0f;
            yield return base.Hide();
        }

        void OnMusicToggle()
        {
            _musicOn = !_musicOn;

            if (_audioManager)
            {
                _audioManager.SetMusicEnabled(_musicOn);
            }

            UpdateAudioButtonUI();
        }

        void OnSfxToggle()
        {
            _sfxOn = !_sfxOn;

            if (_audioManager)
            {
                _audioManager.SetSfxEnabled(_sfxOn);
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
