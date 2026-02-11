using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bowmancer
{
    public class MB_PopupManager : Singleton<MB_PopupManager>
    {
        [SerializeField] private RectTransform graphicsHolder;
        [SerializeField] private MB_Popup popupPrefab;
        [SerializeField] private RectTransform topPopupAnchor;
        [SerializeField] private Canvas canvas;

        private Camera cam;

        protected override void Awake()
        {
            base.Awake();
            cam = Camera.main;
        }

        public void ShowPopup(string text, Vector3 worldPos)
        {
            MB_Popup popup = Instantiate(popupPrefab, graphicsHolder);

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                graphicsHolder,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
                out Vector2 localPos
            );

            popup.GetComponent<RectTransform>().anchoredPosition = localPos;
            popup.Play(text);
        }

        public void ShowTopNotification(string text, Color color)
        {
            MB_Popup popup = Instantiate(popupPrefab, topPopupAnchor);
            popup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            popup.PlayTopNotification(text, color);
        }
    }
}
