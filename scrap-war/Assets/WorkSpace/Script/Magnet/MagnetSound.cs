using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class MagnetSound : MagnetComponent
{
    private Coroutine loopRoutine;
    private AudioSource audioSource;

    [Header("Sound Clips")]
    [SerializeField] private AudioClip pullClip;
    [SerializeField, Range(0f, 1f)] private float pullVolume = 1f;

    [SerializeField] private AudioClip shootClip;
    [SerializeField, Range(0f, 1f)] private float shootVolume = 1f;

    [Header("Clip Settings")]
    [SerializeField] private float shootStartTime = 0f;
    [SerializeField] private float loopStartTime = 1.5f;
    [SerializeField] private float loopEndTime = 4.5f;

    public override void Initialize(MagnetController mc)
    {
        base.Initialize(mc);

        audioSource = magnetController.GetComponent<AudioSource>();
        ValidationUtils.CheckNull(audioSource, "[MagnetSound.cs] ---> Missing Audio Source");
        ValidationUtils.CheckNull(pullClip, "[MagnetSound.cs] ---> Missing pullClip");
        ValidationUtils.CheckNull(shootClip, "[MagnetSound.cs] ---> Missing shootClip");
        
        // Đăng ký với SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.RegisterAudioSource(audioSource);
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký khi destroy
        if (SoundManager.Instance != null && audioSource != null)
        {
            SoundManager.Instance.UnregisterAudioSource(audioSource);
        }
    }

    private void PlayClip(AudioClip clip, float volume = 1f, float startTime = 0f, bool loop = false)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayManagedClip(audioSource, clip, volume, startTime, loop);
        }
        else
        {
            // Fallback nếu không có SoundManager
            if (clip == null || audioSource == null) return;

            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = clip;
            audioSource.volume = Mathf.Clamp01(volume);
            audioSource.time = Mathf.Clamp(startTime, 0f, clip.length);
            audioSource.loop = loop;
            audioSource.Play();
        }
    }

    public void PlayPull()
    {
        StopPull();

        if (pullClip == null || audioSource == null) return;

        loopRoutine = magnetController.StartCoroutine(LoopPullClip());
    }

    public void StopPull()
    {
        if (loopRoutine != null)
        {
            magnetController.StopCoroutine(loopRoutine);
            loopRoutine = null;
        }

        audioSource.Stop();
    }

    private IEnumerator LoopPullClip()
    {
        PlayClip(pullClip, pullVolume, loopStartTime, false);

        while (true)
        {
            if (!audioSource.isPlaying)
            {
                PlayClip(pullClip, pullVolume, loopStartTime, false);
            }

            if (audioSource.time >= loopEndTime)
            {
                audioSource.time = loopStartTime;
            }

            yield return null;
        }
    }

    public void PlayShoot()
    {
        StopPull(); // Ngắt loop nếu đang kéo

        PlayClip(shootClip, shootVolume, shootStartTime, false);
    }

    public override void Update() { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}