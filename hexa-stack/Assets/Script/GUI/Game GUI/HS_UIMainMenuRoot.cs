using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace HexaStack
{
    public class HS_UIMainMenuRoot : HS_BaseUI
    {
        [SerializeField] HS_UIButton tutorialBtn;
        [SerializeField] HS_UIButton settingsBtn;

        [SerializeField] HS_UIButton audioBtn;
        [SerializeField] HS_UIButton soundBtn;
        [SerializeField] HS_UIButton restartBtn;

        [SerializeField] RectTransform settingsGroup;
        [SerializeField] float slideDuration = 0.3f;
        [SerializeField] Ease slideEase = Ease.OutCubic;

        private Image audioBtnDisabledIcon;
        private Image soundBtnDisabledIcon;

        HS_AudioManager audioManager;

        private bool isMusicEnabled = true;
        private bool isSfxEnabled = true;
        private bool isSettingsOpen = false;

        private Vector2 settingsHiddenPos;
        private Vector2 settingsVisiblePos;

        protected override void Awake()
        {
            base.Awake();
            audioManager = HS_AudioManager.Instance;

            if (audioBtn != null && audioBtn.transform.childCount > 0)
            {
                audioBtnDisabledIcon = audioBtn.transform.GetChild(0).GetComponent<Image>();
            }

            if (soundBtn != null && soundBtn.transform.childCount > 0)
            {
                soundBtnDisabledIcon = soundBtn.transform.GetChild(0).GetComponent<Image>();
            }

            isMusicEnabled = PlayerPrefs.GetInt(HS_SafetyKey.KEY_MUSIC_ON, 1) == 1;
            isSfxEnabled = PlayerPrefs.GetInt(HS_SafetyKey.KEY_SFX_ON, 1) == 1;

            if (tutorialBtn) tutorialBtn.Bind(() => Open(UIPageId.Tutorial));
            if (settingsBtn) settingsBtn.Bind(() => ToggleSettings());
            if (restartBtn) restartBtn.Bind(() =>
            {
                HS_GameManager.Instance.SetState(GameState.Initialize);
                HS_GameManager.Instance.StartGame();
            });

            if (audioBtn) audioBtn.Bind(OnAudioToggle);
            if (soundBtn) soundBtn.Bind(OnSoundToggle);

            if (settingsGroup != null)
            {
                settingsVisiblePos = settingsGroup.anchoredPosition;
                settingsHiddenPos = settingsVisiblePos + new Vector2(0, settingsGroup.rect.height);
                settingsGroup.anchoredPosition = settingsHiddenPos;
            }
        }

        void Start()
        {
            if (isMusicEnabled) OnMusicOn();
            else OnMusicOff();

            if (isSfxEnabled) OnSfxOn();
            else OnSfxOff();

            UpdateAudioButtonState();
            UpdateSoundButtonState();

            Open(UIPageId.MainMenu);
        }

        void OnDestroy()
        {
            if (tutorialBtn) tutorialBtn.UnBind();
            if (settingsBtn) settingsBtn.UnBind();
            if (restartBtn) restartBtn.UnBind();
            if (audioBtn) audioBtn.UnBind();
            if (soundBtn) soundBtn.UnBind();

            if (settingsGroup != null)
            {
                settingsGroup.DOKill();
            }
        }

        protected override void HandleGameState(GameState s)
        {
            switch (s)
            {
                case GameState.Initialize:
                    Open(UIPageId.MainMenu);
                    break;
                case GameState.Play:
                    Open(UIPageId.GamePlay);
                    break;
                case GameState.Lose:
                    Open(UIPageId.FinishGame);
                    break;
            }
        }

        private void ToggleSettings()
        {
            if (isSettingsOpen)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }

        private void OpenSettings()
        {
            if (settingsGroup == null) return;

            isSettingsOpen = true;

            settingsGroup.DOKill();

            settingsGroup.DOAnchorPos(settingsVisiblePos, slideDuration)
                .SetEase(slideEase);
        }

        private void CloseSettings()
        {
            if (settingsGroup == null) return;

            isSettingsOpen = false;

            settingsGroup.DOKill();

            settingsGroup.DOAnchorPos(settingsHiddenPos, slideDuration)
                .SetEase(slideEase);
        }

        private void OnAudioToggle()
        {
            if (isMusicEnabled)
                OnMusicOff();
            else
                OnMusicOn();

            UpdateAudioButtonState();
        }

        private void OnSoundToggle()
        {
            if (isSfxEnabled)
                OnSfxOff();
            else
                OnSfxOn();

            UpdateSoundButtonState();
        }

        private void UpdateAudioButtonState()
        {
            if (audioBtnDisabledIcon == null) return;
            audioBtnDisabledIcon.gameObject.SetActive(!isMusicEnabled);
        }

        private void UpdateSoundButtonState()
        {
            if (soundBtnDisabledIcon == null) return;
            soundBtnDisabledIcon.gameObject.SetActive(!isSfxEnabled);
        }

        public void OnMusicOn()
        {
            isMusicEnabled = true;
            if (audioManager) audioManager.SetMusicEnabled(true);
            PlayerPrefs.SetInt(HS_SafetyKey.KEY_MUSIC_ON, 1);
        }

        public void OnMusicOff()
        {
            isMusicEnabled = false;
            if (audioManager) audioManager.SetMusicEnabled(false);
            PlayerPrefs.SetInt(HS_SafetyKey.KEY_MUSIC_ON, 0);
        }

        public void OnSfxOn()
        {
            isSfxEnabled = true;
            if (audioManager) audioManager.SetSfxEnabled(true);
            PlayerPrefs.SetInt(HS_SafetyKey.KEY_SFX_ON, 1);
        }

        public void OnSfxOff()
        {
            isSfxEnabled = false;
            if (audioManager) audioManager.SetSfxEnabled(false);
            PlayerPrefs.SetInt(HS_SafetyKey.KEY_SFX_ON, 0);
        }
    }
}