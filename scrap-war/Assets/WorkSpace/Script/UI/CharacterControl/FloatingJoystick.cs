using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class FloatingJoystick : MonoBehaviour
{
    [HideInInspector] public RectTransform RectTransform;
    public RectTransform Knob;

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();

        if (Knob == null)
        {
            Knob = transform.GetChild(0).GetComponent<RectTransform>();
        }
    }
}
