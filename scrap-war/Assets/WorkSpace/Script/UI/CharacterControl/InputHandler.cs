using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

// [RequireComponent(typeof(TouchSimulation))]
public class InputHandler : MonoBehaviour
{
    [SerializeField] FloatingJoystick joystick;
    private Vector2 joystickSize = new Vector2(230, 230);

    public Vector2 MovementAmount { get; private set; }

    public System.Action<Vector2> OnMovementInputChanged;
    public System.Action OnMovementStart;
    public System.Action OnMovementStop;

    private Finger movementFinger;
    private Vector2 joystickOriginPosition;
    private Vector2 joystickDefaultPosition;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        ETouch.Touch.onFingerDown += HandleFingerDown;
        ETouch.Touch.onFingerUp += HandleFingerUp;
        ETouch.Touch.onFingerMove += HandleFingerMove;
    }

    private void OnDisable()
    {
        ETouch.Touch.onFingerDown -= HandleFingerDown;
        ETouch.Touch.onFingerUp -= HandleFingerUp;
        ETouch.Touch.onFingerMove -= HandleFingerMove;
        EnhancedTouchSupport.Disable();
    }

    private void Start()
    {

        joystickDefaultPosition = joystick.RectTransform.anchoredPosition;
    }

    private void HandleFingerDown(Finger finger)
    {
        if (movementFinger == null && finger.screenPosition.x <= Screen.width / 2f)
        {
            movementFinger = finger;
            MovementAmount = Vector2.zero;
            joystick.RectTransform.sizeDelta = joystickSize;
            joystickOriginPosition = ClampStartPosition(finger.screenPosition / 2);
            joystick.RectTransform.anchoredPosition = joystickOriginPosition;

            OnMovementStart?.Invoke();
        }
    }

    private void HandleFingerMove(Finger finger)
    {
        if (finger == movementFinger)
        {
            Vector2 knobPosition;
            float maxMovement = joystickSize.x / 2f;
            Vector2 adjustedPosition = finger.currentTouch.screenPosition / 2;

            if (Vector2.Distance(adjustedPosition, joystick.RectTransform.anchoredPosition) > maxMovement)
            {
                knobPosition = (adjustedPosition - joystick.RectTransform.anchoredPosition).normalized * maxMovement;
            }
            else
            {
                knobPosition = adjustedPosition - joystick.RectTransform.anchoredPosition;
            }

            joystick.Knob.anchoredPosition = knobPosition;
            MovementAmount = knobPosition / maxMovement;

            OnMovementInputChanged?.Invoke(MovementAmount);
        }
    }

    private void HandleFingerUp(Finger finger)
    {
        if (finger == movementFinger)
        {
            movementFinger = null;
            joystick.Knob.anchoredPosition = Vector2.zero;
            joystick.RectTransform.anchoredPosition = joystickDefaultPosition;
            MovementAmount = Vector2.zero;

            OnMovementStop?.Invoke();
        }
    }

    private Vector2 ClampStartPosition(Vector2 start)
    {
        if (start.x < joystickSize.x / 2) start.x = joystickSize.x / 2;
        if (start.y < joystickSize.y / 2) start.y = joystickSize.y / 2;
        else if (start.y > Screen.height - joystickSize.y / 2) start.y = Screen.height - joystickSize.y / 2;
        return start;
    }

    public void ResetInput()
    {
        movementFinger = null;
        joystick.Knob.anchoredPosition = Vector2.zero;
        joystick.RectTransform.anchoredPosition = joystickDefaultPosition;
        MovementAmount = Vector2.zero;
        OnMovementStop?.Invoke();
    }
}
