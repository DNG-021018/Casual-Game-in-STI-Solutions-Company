using UnityEngine;
using UnityEngine.UI;

public class SliderRotateFollow : MonoBehaviour
{
    public Slider slider;
    public RectTransform fillRect;
    public RectTransform rotateIcon;

    void Update()
    {
        float fillWidth = fillRect.rect.width;
        rotateIcon.anchoredPosition = new Vector2(fillWidth, rotateIcon.anchoredPosition.y);
    }
}
