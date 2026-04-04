using UnityEngine;

namespace CataFury
{
    public class CF_AudioButton : CF_UIButton
    {
        public GameObject TurnOnIcon;
        public GameObject TurnOffIcon;

        public void SetAudioState(bool isOn)
        {
            if (TurnOffIcon)
            {
                TurnOffIcon.SetActive(!isOn);
            }

            if (TurnOnIcon)
            {
                TurnOnIcon.SetActive(isOn);
            }
        }
    }
}
