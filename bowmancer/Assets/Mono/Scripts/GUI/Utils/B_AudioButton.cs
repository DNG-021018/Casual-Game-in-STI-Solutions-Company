using UnityEngine;

namespace Bowmancer
{
    public class B_AudioButton : MonoBehaviour
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
