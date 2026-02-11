using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace CB_CubeRunner
{
    public class CB_ShopContentItems : MonoBehaviour, IPointerClickHandler
    {
        [Header("Image")]
        [SerializeField] private Image skinIcon;

        [Header("TMP")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI priceText;

        private Action onClick;

        public void BindClick(Action callback)
        {
            onClick = callback;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke();
        }

        public void SetContent(string name, string price, Sprite icon)
        {
            if (nameText != null)
                nameText.text = string.IsNullOrEmpty(name) ? "Unknown" : name;

            if (priceText != null)
                priceText.text = price;

            if (skinIcon != null && icon != null)
                skinIcon.sprite = icon;
        }
    }
}