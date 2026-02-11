using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace HexaStack
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class HS_UIButton : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] Button button;
        [SerializeField] HS_UIClickSfx sfx;

        [Header("Behavior")]
        [SerializeField] bool blockWhileInvoking = true;
        [Range(0f, 1f)][SerializeField] float cooldown = 0.1f;

        [Header("Tween")]
        [SerializeField] bool usePressAnimation = true;
        [SerializeField] float pressScale = 0.95f;
        [SerializeField] float pressDuration = 0.1f;

        TextMeshProUGUI _text;
        Action _onClick;
        bool _isInvoking;
        float _lastClickTime;
        Tween _pressTween;

        void Awake()
        {
            if (!button) button = GetComponent<Button>();
            if (!sfx) sfx = GetComponent<HS_UIClickSfx>();
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }

        void OnDestroy()
        {
            _pressTween?.Kill();
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

            if (usePressAnimation)
            {
                PlayPressAnimation();
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

        void PlayPressAnimation()
        {
            _pressTween?.Kill();
            RectTransform rectTransform = GetComponent<RectTransform>();

            _pressTween = rectTransform.DOScale(pressScale, pressDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    rectTransform.DOScale(1f, pressDuration).SetEase(Ease.OutQuad);
                });
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
                if (button != null) button.interactable = true;
            }
        }

        public void SetInteractable(bool interacted)
        {
            if (button != null) button.interactable = interacted;
        }
    }
}
