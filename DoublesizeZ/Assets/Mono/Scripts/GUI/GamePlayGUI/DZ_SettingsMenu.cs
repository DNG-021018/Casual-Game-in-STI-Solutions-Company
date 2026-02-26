using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DoublesideZ
{
    public class DZ_SettingsMenu : DZ_UIPage
    {
        [Header("Button")]
        [SerializeField] DZ_UIButton closeButton;

        [SerializeField] DZ_AudioButton musicButton;
        [SerializeField] Toggle musicToggleBtn;

        [SerializeField] DZ_AudioButton sfxButton;
        [SerializeField] Toggle sfxToggleBtn;

        [Header("Tween")]
        [SerializeField] private RectTransform settingsPopup;
        [SerializeField] private float popDuration = 0.35f;
        private Vector3 _popupOriginalScale;
        private bool _scaleCached;

        private Vector3 GetOriginalScale()
        {
            if (!_scaleCached && settingsPopup != null)
            {
                _popupOriginalScale = settingsPopup.localScale;
                if (_popupOriginalScale == Vector3.zero)
                    _popupOriginalScale = Vector3.one;
                _scaleCached = true;
            }
            return _popupOriginalScale;
        }


        private bool _musicOn = true;
        private bool _sfxOn = true;

        private DZ_AudioManager _audioManager;
        private DZ_BaseUI parent;

        void Awake()
        {
            _audioManager = ServiceLocator.Get<DZ_AudioManager>();
        }

        public override void Init(DZ_BaseUI parent)
        {
            base.Init(parent);
            this.parent = parent;

            _musicOn = PlayerPrefs.GetInt(DZ_SafetyKey.KEY_PLAYPREF_MUSIC_ON, 1) == 1;
            _sfxOn = PlayerPrefs.GetInt(DZ_SafetyKey.KEY_PLAYPREF_SFX_ON, 1) == 1;
            UpdateAudioButtonUI();
        }

        private void OnEnable()
        {
            if (closeButton != null)
                closeButton.Bind(() => parent.Back());

            if (musicButton) musicButton.Bind(OnMusicToggle);
            if (musicToggleBtn) musicToggleBtn.onValueChanged.AddListener(OnMusicToggleChanged);
            if (sfxButton) sfxButton.Bind(OnSfxToggle);
            if (sfxToggleBtn) sfxToggleBtn.onValueChanged.AddListener(OnSfxToggleChanged);
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.UnBind();
            if (musicButton) musicButton.UnBind();
            if (musicToggleBtn) musicToggleBtn.onValueChanged.RemoveListener(OnMusicToggleChanged);
            if (sfxButton) sfxButton.UnBind();
            if (sfxToggleBtn) sfxToggleBtn.onValueChanged.RemoveListener(OnSfxToggleChanged);
        }


        public override IEnumerator Show()
        {
            canvasGroup.alpha = 1f;
            yield return base.Show();

            if (settingsPopup != null)
            {
                Vector3 target = GetOriginalScale();
                settingsPopup.localScale = Vector3.zero;
                settingsPopup.DOScale(target, popDuration)
                    .SetEase(Ease.OutBack, 1.5f)
                    .SetUpdate(true);
            }

            yield return new WaitForSecondsRealtime(popDuration);

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        public override IEnumerator Hide()
        {
            canvasGroup.alpha = 0f;

            if (settingsPopup != null)
            {
                settingsPopup.DOScale(0f, popDuration * 0.7f)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true)
                    .OnComplete(() => canvasGroup.alpha = 0f);

            }

            if (settingsPopup != null) settingsPopup.localScale = GetOriginalScale();
            yield return base.Hide();
        }


        void OnMusicToggle() { _musicOn = !_musicOn; _audioManager?.SetMusicEnabled(_musicOn); UpdateAudioButtonUI(); }
        void OnSfxToggle() { _sfxOn = !_sfxOn; _audioManager?.SetSfxEnabled(_sfxOn); UpdateAudioButtonUI(); }
        void OnMusicToggleChanged(bool v) { _musicOn = v; _audioManager?.SetMusicEnabled(_musicOn); UpdateAudioButtonUI(); }
        void OnSfxToggleChanged(bool v) { _sfxOn = v; _audioManager?.SetSfxEnabled(_sfxOn); UpdateAudioButtonUI(); }

        void UpdateAudioButtonUI()
        {
            if (musicButton) musicButton.SetAudioState(_musicOn);
            if (musicToggleBtn) musicToggleBtn.isOn = _musicOn;
            if (sfxButton) sfxButton.SetAudioState(_sfxOn);
            if (sfxToggleBtn) sfxToggleBtn.isOn = _sfxOn;
        }
    }
}