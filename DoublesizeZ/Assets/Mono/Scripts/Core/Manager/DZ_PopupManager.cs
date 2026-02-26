using UnityEngine;

namespace DoublesideZ
{
    public class DZ_PopupManager : MonoBehaviour
    {
        [SerializeField] private RectTransform graphicsHolder;
        [SerializeField] private DZ_Popup popupPrefab;
        [SerializeField] private RectTransform topPopupAnchor;
        [SerializeField] private Canvas canvas;

        public void ShowTopNotification(string text, Color color)
        {
            DZ_Popup popup = Instantiate(popupPrefab, topPopupAnchor);
            popup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            popup.PlayTopNotification(text, color);
        }
    }
}
