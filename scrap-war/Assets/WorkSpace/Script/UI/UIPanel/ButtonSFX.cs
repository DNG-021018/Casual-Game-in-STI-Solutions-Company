using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
    [SerializeField] private AudioClip[] buttonClickSounds;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button?.onClick.AddListener(PlayButtonSound);
    }

    private void OnDisable()
    {
        button?.onClick.RemoveListener(PlayButtonSound);
    }

    private void PlayButtonSound()
    {
        if (buttonClickSounds != null && buttonClickSounds.Length > 0 && SoundManager.Instance != null)
        {
            int index = Random.Range(0, buttonClickSounds.Length);
            SoundManager.Instance.PlaySFX(buttonClickSounds[index]);
        }
    }
}
