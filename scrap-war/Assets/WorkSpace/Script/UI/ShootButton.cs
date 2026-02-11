using UnityEngine;
using UnityEngine.EventSystems;

public class ShootButton : MonoBehaviour, IPointerDownHandler
{
    public MagnetController magnetController;

    [System.Obsolete]
    public void OnPointerDown(PointerEventData eventData)
    {
        magnetController._magnetComponent.Shooting();
    }
}
