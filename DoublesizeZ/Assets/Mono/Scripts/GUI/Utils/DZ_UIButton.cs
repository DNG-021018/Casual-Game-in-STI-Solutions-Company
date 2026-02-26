using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoublesideZ
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class DZ_UIButton : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] Button button;
        [SerializeField] DZ_UIClickSfx sfx;

        [Header("Behavior")]
        [SerializeField] bool blockWhileInvoking = true;
        [Range(0f, 1f)][SerializeField] float cooldown = 0.1f;

        TextMeshProUGUI _text;
        Action _onClick;
        bool _isInvoking;
        float _lastClickTime;

        void Awake()
        {
            if (!button) button = GetComponent<Button>();
            if (!sfx) sfx = GetComponent<DZ_UIClickSfx>();
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void SetText(string txt)
        {
            if (_text != null)
            {
                _text.text = txt;
            }
        }

        public void Bind(Action onClick)
        {
            _onClick = onClick;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }

        public void UnBind()
        {
            _onClick = null;
            button.onClick.RemoveListener(HandleClick);
            button.onClick.RemoveAllListeners();
        }

        void HandleClick()
        {
            if (!button.interactable) return;

            if (Time.unscaledTime - _lastClickTime < cooldown) return;
            _lastClickTime = Time.unscaledTime;

            if (_isInvoking && blockWhileInvoking) return;

            if (blockWhileInvoking)
            {
                _isInvoking = true;
                button.interactable = false;
            }

            if (sfx != null)
            {
                sfx.Play(() =>
                {
                    SafeInvoke();
                    FinishInvoke();
                });
            }
            else
            {
                SafeInvoke();
                FinishInvoke();
            }
        }

        void SafeInvoke()
        {
            try { _onClick?.Invoke(); }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        void FinishInvoke()
        {
            if (blockWhileInvoking)
            {
                _isInvoking = false;
                button.interactable = true;
            }
        }

        public void SetInteractable(bool interacted)
        {
            if (button != null) button.interactable = interacted;
        }
    }
}
