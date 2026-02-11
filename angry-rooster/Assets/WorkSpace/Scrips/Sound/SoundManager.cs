using UnityEngine;
using System;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    // Events for volume changes
    public event Action<float> OnBGMVolumeChanged;
    public event Action<float> OnSFXVolumeChanged;
    
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    
    [SerializeField] private float bgmVolume = 0.5f;
    [SerializeField] private float sfxVolume = 0.5f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        // Apply volume to audio sources
        ApplyBGMVolume();
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("BGMVolume", volume);
        // Notify all listeners
        OnBGMVolumeChanged?.Invoke(volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        // Apply volume to audio sources
        ApplySFXVolume();
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("SFXVolume", volume);
        // Notify all listeners
        OnSFXVolumeChanged?.Invoke(volume);
    }
    
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
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
}