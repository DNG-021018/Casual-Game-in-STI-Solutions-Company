using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private bool isBGM = true; // Nếu là false thì là SFX


    [Header("BGM Icon")]
    [SerializeField] private Button buttonBGM;
    [SerializeField] private Image spriteBGM;
    [SerializeField] private Sprite spriteBGMOn;
    [SerializeField] private Sprite spriteBGMOff;

    [Header("SFX Icon")]
    [SerializeField] private Button buttonSFX;
    [SerializeField] private Image spriteSFX;
    [SerializeField] private Sprite spriteSFXOn;
    [SerializeField] private Sprite spriteSFXOff;

    private void Start()
    {
        if (volumeSlider == null) return;
    }

    void OnEnable()
    {
        StartCoroutine(WaitForSoundManager());
    }

    private IEnumerator WaitForSoundManager()
    {
        // Wait until SoundManager.Instance is available
        while (SoundManager.Instance == null)
        {
            yield return null; // Wait one frame
        }

        // Load volume từ SoundManager
        volumeSlider.value = isBGM ?
            SoundManager.Instance.GetBGMVolume() :
            SoundManager.Instance.GetSFXVolume();

        // Cập nhật icon ban đầu
        if (isBGM)
            UpdateVolumeIcon(volumeSlider.value);
        else
            UpdateVolumeIconSFX(volumeSlider.value);

        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        
        // Subscribe to volume change events
        if (isBGM)
        {
            SoundManager.Instance.OnBGMVolumeChanged += OnBGMVolumeChanged;
        }
        else
        {
            SoundManager.Instance.OnSFXVolumeChanged += OnSFXVolumeChanged;
        }
        
        if (buttonBGM != null)
        {
            buttonBGM.onClick.AddListener(OnClickButtonBGM);
        }
        if (buttonSFX != null)
        {
            buttonSFX.onClick.AddListener(OnClickButtonSFX);
        }
    }

    void OnDisable()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            
        // Unsubscribe from events
        if (SoundManager.Instance != null)
        {
            if (isBGM)
            {
                SoundManager.Instance.OnBGMVolumeChanged -= OnBGMVolumeChanged;
            }
            else
            {
                SoundManager.Instance.OnSFXVolumeChanged -= OnSFXVolumeChanged;
            }
        }
        
        if (buttonBGM != null)
            buttonBGM.onClick.RemoveListener(OnClickButtonBGM);
        if (buttonSFX != null)
            buttonSFX.onClick.RemoveListener(OnClickButtonSFX);
    }

    // Event handlers for volume changes from other sources
    private void OnBGMVolumeChanged(float volume)
    {
        if (isBGM)
        {
            volumeSlider.value = volume;
            UpdateVolumeIcon(volume);
        }
    }

    private void OnSFXVolumeChanged(float volume)
    {
        if (!isBGM)
        {
            volumeSlider.value = volume;
            UpdateVolumeIconSFX(volume);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        // Add null check here too
        if (SoundManager.Instance == null) return;

        if (isBGM)
        {
            SoundManager.Instance.SetBGMVolume(value);
            UpdateVolumeIcon(value);
        }
        else
        {
            SoundManager.Instance.SetSFXVolume(value);
            UpdateVolumeIconSFX(value);
        }
    }

    private void OnClickButtonBGM()
    {
        if (buttonBGM == null || spriteBGM == null || SoundManager.Instance == null) return;

        float newVolume = spriteBGM.sprite == spriteBGMOn ? 0 : 0.5f;
        SoundManager.Instance.SetBGMVolume(newVolume);
        UpdateVolumeIcon(newVolume);
        volumeSlider.value = newVolume;
        Debug.Log($"BGM Volume set to: {newVolume}");
    }

    private void OnClickButtonSFX()
    {
        if (buttonSFX == null || spriteSFX == null || SoundManager.Instance == null) return;

        float newVolume = spriteSFX.sprite == spriteSFXOn ? 0 : 0.5f;
        SoundManager.Instance.SetSFXVolume(newVolume);
        UpdateVolumeIconSFX(newVolume);
        volumeSlider.value = newVolume;
        Debug.Log($"SFX Volume set to: {newVolume}");
    }


    private void UpdateVolumeIcon(float volume)
    {
        if (spriteBGM == null) return;

        spriteBGM.sprite = volume > 0 ? spriteBGMOn : spriteBGMOff;
    }
    
    private void UpdateVolumeIconSFX(float volume)
    {
        if (spriteSFX == null) return;

        spriteSFX.sprite = volume > 0 ? spriteSFXOn : spriteSFXOff;
    }
}