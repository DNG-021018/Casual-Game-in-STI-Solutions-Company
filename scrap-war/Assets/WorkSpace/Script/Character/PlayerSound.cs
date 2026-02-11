using System;
using UnityEngine;

public enum PlayerSoundType
{
    Footstep,
    Hit,
    Death
}

[System.Serializable]
public class PlayerSound : CharacterComponents
{
    public AudioSource audioSource;

    [Space(10)]
    [SerializeField] private AudioClip footStep;
    [SerializeField, Range(0f, 1f)] private float footStepVolume = 1f;
    [Space(10)]
    [SerializeField] private AudioClip hitClip;
    [SerializeField, Range(0f, 1f)] private float hitVolume = 1f;
    [Space(10)]
    [SerializeField] private AudioClip deathClip;
    [SerializeField, Range(0f, 1f)] private float deathVolume = 1f;

    public event Action<AudioClip, float, bool> OnSoundPlayed;

    public override void Initialize(PlayerController pc)
    {
        base.Initialize(pc);

        audioSource = characterController.GetComponent<AudioSource>();
        ValidationUtils.CheckNull(audioSource, "[PlayerSound.cs] ---> Missing Audio Source");
        ValidationUtils.CheckNull(hitClip, "[PlayerSound.cs] ---> Missing hitClip");
        ValidationUtils.CheckNull(deathClip, "[PlayerSound.cs] ---> Missing deathClip");
        ValidationUtils.CheckNull(footStep, "[PlayerSound.cs] ---> Missing footStep");
    }

    public void PlayClip(PlayerSoundType soundType, float volume, bool loop = false)
    {
        AudioClip clip = null;

        switch (soundType)
        {
            case PlayerSoundType.Footstep:
                clip = footStep;
                volume = footStepVolume;
                break;
            case PlayerSoundType.Hit:
                clip = hitClip;
                volume = hitVolume;
                break;
            case PlayerSoundType.Death:
                clip = deathClip;
                volume = deathVolume;
                break;
        }

        if (clip != null)
        {
            OnSoundPlayed?.Invoke(clip, volume, loop);
        }
    }

    public void PlayFootstepLoop() => PlayClip(PlayerSoundType.Footstep, footStepVolume, true);
    public void StopFootstepLoop()
    {
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == footStep)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    public void PlayHit() => PlayClip(PlayerSoundType.Hit, hitVolume);
    public void PlayDeath() => PlayClip(PlayerSoundType.Death, deathVolume);

    public override void Update() { }
    public override void OnEnable() { }
    public override void OnDisable() { }
}