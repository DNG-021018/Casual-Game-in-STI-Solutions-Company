using UnityEngine;

namespace CataFury
{
    public class CF_PopupManager : MonoBehaviour
    {
        [SerializeField] private RectTransform graphicsHolder;
        [SerializeField] private CF_Popup popupPrefab;
        [SerializeField] private RectTransform topPopupAnchor;
        [SerializeField] private Canvas canvas;

        public void ShowTopNotification(string text, Color color)
        {
            CF_Popup popup = Instantiate(popupPrefab, topPopupAnchor);
            popup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            popup.PlayTopNotification(text, color);
        }
    }
}
