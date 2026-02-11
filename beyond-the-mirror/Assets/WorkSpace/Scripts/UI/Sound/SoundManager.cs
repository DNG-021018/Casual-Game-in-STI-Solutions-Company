using UnityEngine;
using System;
using UnityEngine.SceneManagement;

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
    [SerializeField] private AudioClip soundPickup;

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
        if (scene.name == MAIN_MENU_SCENE)
        {
            PlayBGM(mainMenuSound, 1.0f);
        }
        else
        {
            PlayBGM(gameSound, 1.2f);
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

    private System.Collections.IEnumerator ResumeBGMWhenSFXEnds(float delay)
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

    private void ApplySFXVolume()
    {
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    public void PickupSound()
    {
        if (soundPickup != null)
        {
            PlaySFX(soundPickup);
        }
    }
}
