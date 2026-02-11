using System;
using UnityEngine;
using UnityEngine.UI;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_UIToggleSlider : MonoBehaviour
    {
        [Header("Slider setup")]
        [SerializeField] private Slider _slider;

        public float CurrentValue { get; private set; }

        public void Initialize()
        {
            if (_slider == null)
            {
                _slider = GetComponent<Slider>();
            }
        }

        private void SetValue(float value)
        {
            _slider.value = value;
        }
    }
}
