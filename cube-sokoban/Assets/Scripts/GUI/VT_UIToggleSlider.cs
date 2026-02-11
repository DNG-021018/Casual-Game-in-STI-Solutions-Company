using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeSokoban
{
    public class CS_UIToggleSlider : MonoBehaviour, IPointerClickHandler
    {
        [Header("Slider setup")]
        public bool CurrentValue { get; private set; }

        private bool _previousValue;
        private Slider _slider;

        [Header("Sprite")]
        [SerializeField] private Sprite OnSprite;
        [SerializeField] private Sprite OffSprite;
        [SerializeField] Image _sprite;

        [Header("Events")]
        [SerializeField] public UnityEvent onToggleOn;
        [SerializeField] public UnityEvent onToggleOff;

        protected Action transitionEffect;

        public void Initialize()
        {
            _slider = GetComponent<Slider>();

            _slider.interactable = false;
            _slider.transition = Selectable.Transition.None;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Toggle();
        }

        private void Toggle()
        {
            SetStateAndStartAnimation(!CurrentValue);
        }

        public void ToggleByGroupManager(bool valueToSetTo)
        {
            SetStateAndStartAnimation(valueToSetTo);
        }

        private void SetStateAndStartAnimation(bool state)
        {
            _previousValue = CurrentValue;
            CurrentValue = state;
            if (_previousValue != CurrentValue)
            {
                if (CurrentValue)
                {
                    onToggleOn?.Invoke();
                    if (_sprite != null) _sprite.sprite = OnSprite;
                }
                else
                {
                    if (_sprite != null) _sprite.sprite = OffSprite;
                    onToggleOff?.Invoke();
                }
            }
            AnimateSlider();
        }

        private void AnimateSlider()
        {
            float endValue = CurrentValue ? 1 : 0;
            _slider.value = endValue;
        }
    }
}
