using UnityEngine;

namespace DoublesideZ
{
    public class DZ_AudioButton : DZ_UIButton
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
