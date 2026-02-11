using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class DragonSound : DragonComponents
{
    private Coroutine flameLoopRoutine;
    private AudioSource audioSource;

    [Header("Sound Clips")]
    [SerializeField] private AudioClip breathClip;
    [SerializeField, Range(0f, 1f)] private float breathVolume = 1f;
    [Space(10)]

    [SerializeField] private AudioClip deathClip;
    [SerializeField, Range(0f, 1f)] private float deathVolume = 1f;
    [Space(10)]

    [SerializeField] private AudioClip flameClip;
    [SerializeField, Range(0f, 1f)] private float flameVolume = 1f;

    [Space(10)]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 1f;
    [Space(10)]

    [SerializeField] private AudioClip roarClip;
    [SerializeField, Range(0f, 1f)] private float roarVolume = 1f;

    [Space(10)]
    [Header("Start Times (seconds)")]
    [SerializeField] private float flameStartTime = 0f;
    [SerializeField] private float flameLoopStartTime = 0f;
    [SerializeField] private float roarStartTime = 0f;
    [SerializeField] private float footstepStartTime = 0f;

    public override void Initialize(DragonController dc)
    {
        base.Initialize(dc);

        audioSource = dragonController.GetComponent<AudioSource>();
        ValidationUtils.CheckNull(audioSource, "[DragonSound.cs] ---> Missing AudioSource");
        ValidationUtils.CheckNull(breathClip, "[DragonSound.cs] ---> Missing breathClip");
        ValidationUtils.CheckNull(deathClip, "[DragonSound.cs] ---> Missing deathClip");
        ValidationUtils.CheckNull(flameClip, "[DragonSound.cs] ---> Missing flameClip");
        ValidationUtils.CheckNull(footstepClip, "[DragonSound.cs] ---> Missing footstepClip");
        ValidationUtils.CheckNull(roarClip, "[DragonSound.cs] ---> Missing roarClip");

        // Đăng ký với SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.RegisterAudioSource(audioSource);
            // Subscribe để cập nhật volume khi SFX volume thay đổi
            SoundManager.Instance.OnSFXVolumeChanged += OnSFXVolumeChanged;
        }
    }


    private void OnDestroy()
    {
        // Hủy đăng ký khi destroy
        if (SoundManager.Instance != null && audioSource != null)
        {
            SoundManager.Instance.UnregisterAudioSource(audioSource);
            SoundManager.Instance.OnSFXVolumeChanged -= OnSFXVolumeChanged;
        }
    }

    private float lastPlayedVolume = 1f; // Lưu volume gốc của clip đang phát

    private void OnSFXVolumeChanged(float newSFXVolume)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            // Cập nhật volume theo SFX volume mới
            audioSource.volume = Mathf.Clamp01(lastPlayedVolume * newSFXVolume);
        }
    }

    private void PlayClip(AudioClip clip, float startTime = 0f, float volume = 1f)
    {
        lastPlayedVolume = volume; // Lưu volume gốc

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayManagedClip(audioSource, clip, volume, startTime, false);
        }
        else
        {
            // Fallback nếu không có SoundManager
            if (clip == null || audioSource == null) return;

            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.volume = Mathf.Clamp01(volume);
            audioSource.time = Mathf.Clamp(startTime, 0, clip.length);
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    public void PlayFlameWithLoop()
    {
        StopFlameLoop();
        PlayClip(flameClip, flameStartTime, flameVolume);
        flameLoopRoutine = dragonController.StartCoroutine(FlameLoopRoutine());
    }

    private IEnumerator FlameLoopRoutine()
    {
        yield return new WaitUntil(() => !audioSource.isPlaying);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayManagedClip(audioSource, flameClip, flameVolume, flameLoopStartTime, true);
        }
        else
        {
            audioSource.clip = flameClip;
            audioSource.time = flameLoopStartTime;
            audioSource.loop = true;
            audioSource.volume = flameVolume;
            audioSource.Play();
        }
    }

    public void StopFlameLoop()
    {
        if (flameLoopRoutine != null)
        {
            dragonController.StopCoroutine(flameLoopRoutine);
            flameLoopRoutine = null;
        }

        if (audioSource.clip == flameClip)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    public void PlayBreath() => PlayClip(breathClip, 0f, breathVolume);
    public void PlayDeath() => PlayClip(deathClip, 0f, deathVolume);
    public void PlayFlame() => PlayClip(flameClip, flameStartTime, flameVolume);
    public void PlayFootstep() => PlayClip(footstepClip, footstepStartTime, footstepVolume);
    public void PlayRoar() => PlayClip(roarClip, roarStartTime, roarVolume);

    public override void Update() { }
    public override void Start() { }
    public override void DrawGizmos() { }
}