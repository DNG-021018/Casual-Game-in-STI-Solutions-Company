using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public event Action<float> OnBGMVolumeChanged;
    public event Action<float> OnSFXVolumeChanged;

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private float bgmVolume = 0.5f;
    [SerializeField] private float sfxVolume = 0.5f;

    [Header("Audio clips")]
    [SerializeField] private AudioClip gameSound;
    [SerializeField] private AudioClip mainMenuSound;

    private AudioSource audioSource;
    private PlayerController playerController;

    // Danh sách các AudioSource được quản lý
    private List<AudioSource> managedAudioSources = new List<AudioSource>();

    private const string MAIN_MENU_SCENE = "MakeUI";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumeSettings();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Clear danh sách AudioSource cũ
        managedAudioSources.Clear();
        if (sfxSource != null && sfxSource.isPlaying)
            sfxSource.Stop();

        if (scene.name == MAIN_MENU_SCENE)
        {
            PlayBGM(mainMenuSound, 1.0f);
        }
        else
        {
            PlayBGM(gameSound, 1f);
            playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                audioSource = playerController.characterSound.audioSource;
                playerController.characterSound.OnSoundPlayed += PlayClip;
            }

            // Tự động đăng ký tất cả AudioSource của Magnet và Dragon
            RegisterMagnetAndDragonAudioSources();
        }
    }

    private void RegisterMagnetAndDragonAudioSources()
    {
        // Đăng ký AudioSource của tất cả MagnetController
        MagnetController[] magnets = FindObjectsByType<MagnetController>(FindObjectsSortMode.None);
        foreach (var magnet in magnets)
        {
            AudioSource magnetAudio = magnet.GetComponent<AudioSource>();
            if (magnetAudio != null)
            {
                RegisterAudioSource(magnetAudio);
            }
        }

        // Đăng ký AudioSource của tất cả DragonController
        DragonController[] dragons = FindObjectsByType<DragonController>(FindObjectsSortMode.None);
        foreach (var dragon in dragons)
        {
            AudioSource dragonAudio = dragon.GetComponent<AudioSource>();
            if (dragonAudio != null)
            {
                RegisterAudioSource(dragonAudio);
            }
        }
    }

    // Đăng ký AudioSource để quản lý volume
    public void RegisterAudioSource(AudioSource source)
    {
        if (source != null && !managedAudioSources.Contains(source))
        {
            managedAudioSources.Add(source);
            // Áp dụng volume hiện tại
            ApplySFXVolumeToSource(source);
        }
    }

    // Hủy đăng ký AudioSource
    public void UnregisterAudioSource(AudioSource source)
    {
        if (source != null && managedAudioSources.Contains(source))
        {
            managedAudioSources.Remove(source);
        }
    }

    public void PlayBGM(AudioClip clip, float pitch = 1.0f)
    {
        if (bgmSource == null || clip == null)
            return;

        bgmSource.clip = clip;
        bgmSource.pitch = pitch;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        ApplyBGMVolume();
        PlayerPrefs.SetFloat("BGMVolume", volume);
        OnBGMVolumeChanged?.Invoke(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        ApplySFXVolume();
        PlayerPrefs.SetFloat("SFXVolume", volume);
        OnSFXVolumeChanged?.Invoke(volume);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
            StartCoroutine(ResumeBGMWhenSFXEnds(clip.length));
        }
    }

    public void PlayClip(AudioClip clip, float volume = 1f, bool loop = false)
    {
        if (clip == null || audioSource == null) return;

        // Nếu đang phát clip khác => dừng lại
        if (audioSource.isPlaying && audioSource.clip != clip)
            audioSource.Stop();

        audioSource.clip = clip;
        audioSource.volume = Mathf.Clamp01(volume * sfxVolume); // Áp dụng sfxVolume
        audioSource.time = 0f;
        audioSource.loop = loop;
        audioSource.Play();
    }

    // Phương thức mới để play clip với AudioSource được quản lý
    public void PlayManagedClip(AudioSource source, AudioClip clip, float volume = 1f, float startTime = 0f, bool loop = false)
    {
        if (clip == null || source == null) return;

        if (source.isPlaying)
            source.Stop();

        source.clip = clip;
        source.volume = Mathf.Clamp01(volume * sfxVolume);
        source.time = Mathf.Clamp(startTime, 0f, clip.length);
        source.loop = loop;
        source.Play();
    }

    public System.Collections.IEnumerator ResumeBGMWhenSFXEnds(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResumeBGM();
    }

    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.UnPause();
        }
    }

    public float GetBGMVolume() => bgmVolume;
    public float GetSFXVolume() => sfxVolume;

    private void LoadVolumeSettings()
    {
        if (!PlayerPrefs.HasKey("BGMVolume"))
            PlayerPrefs.SetFloat("BGMVolume", 0.5f);
        if (!PlayerPrefs.HasKey("SFXVolume"))
            PlayerPrefs.SetFloat("SFXVolume", 0.5f);

        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        ApplyBGMVolume();
        ApplySFXVolume();
    }

    private void ApplyBGMVolume()
    {
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
    }

    private void ApplySFXVolumeToSource(AudioSource source)
    {
        if (source != null)
        {
            // Lưu volume gốc trong metadata hoặc tính toán lại
            // Giả sử volume hiện tại đã được scale, ta cần tính lại
            if (source.isPlaying)
            {
                // Nếu đang phát, giữ nguyên tỷ lệ volume hiện tại
                float currentNormalizedVolume = source.volume;
                source.volume = Mathf.Clamp01(currentNormalizedVolume * sfxVolume);
            }
        }
    }

    private void ApplySFXVolume()
    {
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;

        // Áp dụng volume cho tất cả AudioSource được quản lý
        foreach (var source in managedAudioSources)
        {
            if (source != null && source.isPlaying)
            {
                // Tính lại volume dựa trên SFX volume mới
                source.volume = Mathf.Clamp01(source.volume / (sfxVolume > 0 ? sfxVolume : 1f) * sfxVolume);
            }
        }
    }
}