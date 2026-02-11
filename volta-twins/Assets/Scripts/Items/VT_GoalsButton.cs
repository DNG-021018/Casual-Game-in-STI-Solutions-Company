using UnityEngine;

namespace VoltaTwins
{
    public class VT_GoalsButton : MonoBehaviour
    {
        [SerializeField] GameObject VFX;

        [Header("LayerMash")]
        [SerializeField] private LayerMask triggerLayers;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip pressClip;

        private int _insideCount = 0;
        private bool _isPressed = false;

        private VT_AudioManager audioManager;

        void Start()
        {
            audioManager = VT_AudioManager.Instance;
            SetVFX(false);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsInTriggerLayer(other.gameObject.layer)) return;

            SetVFX(true);

            if (!_isPressed)
            {
                _isPressed = true;
                SetVFX(true);
                audioManager.PlaySfx(pressClip);

                if (VT_LevelManager.Instance != null)
                {
                    VT_LevelManager.Instance.OnGoalButtonStateChanged(true);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!IsInTriggerLayer(other.gameObject.layer))
                return;

            SetVFX(false);

            if (_insideCount == 0 && _isPressed)
            {
                _isPressed = false;
                SetVFX(false);

                if (VT_LevelManager.Instance != null)
                {
                    VT_LevelManager.Instance.OnGoalButtonStateChanged(false);
                }
            }
        }

        bool IsInTriggerLayer(int layer)
        {
            return (triggerLayers.value & (1 << layer)) != 0;
        }

        private void SetVFX(bool isOn)
        {
            if (VFX == null) return;
            VFX.SetActive(isOn);
        }
    }
}
