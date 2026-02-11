using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;

public class FreeLookCameraControl : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private CinemachineFreeLook camFreeLook;
    [SerializeField] private bool invertY = false;
    [SerializeField] private float sensitivityX = 0.1f;
    [SerializeField] private float sensitivityY = 0.05f;

    private Image imageFreeLookArea;
    private Vector2 lastPos;
    private bool isDragging;

    void Start()
    {
        imageFreeLookArea = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            imageFreeLookArea.rectTransform,
            eventData.position,
            eventData.enterEventCamera,
            out Vector2 outPos))
        {
            lastPos = outPos;
            isDragging = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            imageFreeLookArea.rectTransform,
            eventData.position,
            eventData.enterEventCamera,
            out Vector2 currentPos))
        {
            float deltaX = currentPos.x - lastPos.x;
            float deltaY = currentPos.y - lastPos.y;

            lastPos = currentPos;


            float yDir = invertY ? 1f : -1f;

            camFreeLook.m_XAxis.m_InputAxisValue = -deltaX * sensitivityX;
            camFreeLook.m_YAxis.m_InputAxisValue = -deltaY * sensitivityY * yDir;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        camFreeLook.m_XAxis.m_InputAxisValue = 0f;
        camFreeLook.m_YAxis.m_InputAxisValue = 0f;
    }
}

