using UnityEngine;

namespace NightEscape
{
    public class NE_CanvasCamera : MonoBehaviour
    {
        void Awake()
        {
            Canvas canvas = GetComponent<Canvas>();

            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (canvas.worldCamera == null)
                {
                    canvas.worldCamera = Camera.main;
                }
            }
        }
    }
}
