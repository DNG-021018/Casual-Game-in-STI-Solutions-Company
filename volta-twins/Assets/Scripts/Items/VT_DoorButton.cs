using System.Collections.Generic;
using UnityEngine;

namespace VoltaTwins
{
    public class VT_DoorButton : MonoBehaviour
    {
        [Header("Door Groups")]
        [SerializeField] private List<VT_Door> doorsOpenOnPress = new();
        [SerializeField] private List<VT_Door> doorsCloseOnPress = new();

        [Header("Trigger Settings")]
        [SerializeField] private LayerMask triggerLayers;
        [SerializeField] private bool holdToKeepOpen = true;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip pressClip;

        private VT_AudioManager audioManager;
        private Animator buttonAnimator;

        int _buttonHash;
        bool _isPressed;
        int _insideCount;

        void Awake()
        {
            audioManager = VT_AudioManager.Instance;

            buttonAnimator = GetComponent<Animator>();
            _buttonHash = Animator.StringToHash(VT_SafetyKey.ANIM_BUTTON);

            BoxCollider box = GetComponent<BoxCollider>();
            box.isTrigger = true;
        }

        void Start()
        {
            _isPressed = false;
            buttonAnimator.SetBool(_buttonHash, _isPressed);

            ApplyDoorGroups(_isPressed);
        }

        bool IsInTriggerLayer(int layer)
        {
            return (triggerLayers.value & (1 << layer)) != 0;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsInTriggerLayer(other.gameObject.layer)) return;

            if (holdToKeepOpen)
            {
                _insideCount++;
                if (_insideCount == 1)
                {
                    SetPressed(true);
                }
            }
            else
            {
                SetPressed(!_isPressed);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!holdToKeepOpen) return;
            if (!IsInTriggerLayer(other.gameObject.layer)) return;

            _insideCount--;
            if (_insideCount <= 0)
            {
                _insideCount = 0;
                SetPressed(false);
            }
        }

        void SetPressed(bool pressed)
        {
            _isPressed = pressed;

            if (buttonAnimator != null)
            {
                buttonAnimator.SetBool(_buttonHash, _isPressed);
            }

            if (audioManager != null && pressClip != null)
            {
                audioManager.PlaySfx(pressClip);
            }

            ApplyDoorGroups(_isPressed);
        }

        void ApplyDoorGroups(bool pressed)
        {
            bool openForGroupOpen = pressed;
            bool openForGroupClose = !pressed;

            if (doorsOpenOnPress != null)
            {
                foreach (var d in doorsOpenOnPress)
                {
                    if (d == null) continue;
                    d.SetOpen(openForGroupOpen);
                }
            }

            if (doorsCloseOnPress != null)
            {
                foreach (var d in doorsCloseOnPress)
                {
                    if (d == null) continue;
                    d.SetOpen(openForGroupClose);
                }
            }
        }
    }
}
