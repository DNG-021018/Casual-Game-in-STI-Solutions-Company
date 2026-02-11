using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_MissionItems : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] Image itemIcon;
        [SerializeField] TextMeshProUGUI itemRequestText;

        [Header("Panels")]
        [SerializeField] GameObject DefaultPanel;
        [SerializeField] GameObject SuccessPanel;

        int _required;
        int _current;
        bool _done;

        public void Init(bJakGZQ3_Item slot)
        {
            _required = slot.requiredAmount;
            _current = slot.currentAmount;
            _done = slot.IsComplete;

            if (itemIcon && slot.icon) itemIcon.sprite = slot.icon;

            UpdateText();
            ApplyVisual();
        }

        public void RefreshProgress(bJakGZQ3_Item slot)
        {
            _required = slot.requiredAmount;
            _current = slot.currentAmount;
            _done = slot.IsComplete;

            UpdateText();
            ApplyVisual();
        }

        void UpdateText()
        {
            if (itemRequestText != null)
            {
                itemRequestText.text = $"{_current}/{_required}";
            }
        }

        void ApplyVisual()
        {
            if (_done)
            {
                if (DefaultPanel) DefaultPanel.SetActive(false);

                if (SuccessPanel) SuccessPanel.SetActive(true);
            }
            else
            {
                if (DefaultPanel) DefaultPanel.SetActive(true);

                if (SuccessPanel) SuccessPanel.SetActive(false);
            }
        }
    }
}
