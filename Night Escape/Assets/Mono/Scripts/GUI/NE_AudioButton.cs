using UnityEngine;

namespace NightEscape
{
    public class NE_AudioButton : MonoBehaviour
    {
        public GameObject TurnOffIcon;

        public void SetAudioState(bool isOn)
        {
            if (TurnOffIcon)
            {
                TurnOffIcon.SetActive(!isOn);
            }
        }
    }
}
