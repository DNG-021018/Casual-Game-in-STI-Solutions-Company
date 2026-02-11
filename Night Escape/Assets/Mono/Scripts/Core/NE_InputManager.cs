using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NightEscape
{
    [DefaultExecutionOrder(-80)]
    public class NE_InputManager : MonoBehaviour
    {
        public static NE_InputManager Instance { get; private set; }

        public delegate void StartTouch(Vector2 position, float time);
        public event StartTouch OnStartTouch;

        public delegate void EndTouch(Vector2 startPos, Vector2 endPos, float time);
        public event EndTouch OnEndTouch;

        private PlayerInputActions inputActions;
        private Vector2 _touchStartPos;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            inputActions = new PlayerInputActions();
        }

        void OnEnable()
        {
            inputActions.Enable();
            inputActions.Movement.PrimaryContact.started += StartTouchPrimary;
            inputActions.Movement.PrimaryContact.canceled += EndTouchPrimary;
        }

        void OnDisable()
        {
            inputActions.Movement.PrimaryContact.started -= StartTouchPrimary;
            inputActions.Movement.PrimaryContact.canceled -= EndTouchPrimary;
            inputActions.Disable();
        }

        private void StartTouchPrimary(InputAction.CallbackContext ctx)
        {
            StartCoroutine(StartTouchPrimaryCoroutine(ctx));
        }

        IEnumerator StartTouchPrimaryCoroutine(InputAction.CallbackContext ctx)
        {
            yield return null;

            Vector2 screenPos = inputActions.Movement.PrimaryPosition.ReadValue<Vector2>();
            _touchStartPos = screenPos;

            OnStartTouch?.Invoke(screenPos, (float)ctx.startTime);
        }

        private void EndTouchPrimary(InputAction.CallbackContext ctx)
        {
            Vector2 screenPos = inputActions.Movement.PrimaryPosition.ReadValue<Vector2>();

            OnEndTouch?.Invoke(_touchStartPos, screenPos, (float)ctx.time);
        }
    }
}
