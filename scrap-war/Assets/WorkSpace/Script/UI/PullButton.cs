using UnityEngine;
using UnityEngine.EventSystems;

public class PullButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] MagnetController magnetController;

    public void OnPointerDown(PointerEventData eventData)
    {
        magnetController._magnetComponent.SetPullState(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        magnetController._magnetComponent.SetPullState(false);
    }
}
