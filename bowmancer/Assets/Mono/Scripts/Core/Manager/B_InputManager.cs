using UnityEngine;
using UnityEngine.InputSystem;

namespace Bowmancer
{
    public class B_InputManager : Singleton<B_InputManager>
    {
        public delegate void StartedTouch(Vector2 position);
        public delegate void PerformedTouch(Vector2 position);
        public delegate void EndTouch(Vector2 position);

        public event StartedTouch OnTouchStart;
        public event PerformedTouch OnTouch;
        public event EndTouch OnTouchEnd;

        private InputSystemActions inputActions;

        protected override void Awake()
        {
            base.Awake();
            inputActions = new();
        }

        void OnEnable()
        {
            inputActions.Enable();
            inputActions.Player.Move.started += StarTouchPrimary;
            inputActions.Player.Move.performed += PerformedTouchPrimary;
            inputActions.Player.Move.canceled += EndTouchPrimary;
        }

        void OnDisable()
        {
            inputActions.Player.Move.performed -= PerformedTouchPrimary;
            inputActions.Player.Move.started -= StarTouchPrimary;
            inputActions.Player.Move.canceled -= EndTouchPrimary;
            inputActions.Disable();
        }

        private void StarTouchPrimary(InputAction.CallbackContext context)
        {
            OnTouchStart?.Invoke(context.ReadValue<Vector2>());
        }

        private void PerformedTouchPrimary(InputAction.CallbackContext context)
        {
            OnTouch?.Invoke(context.ReadValue<Vector2>());
        }

        private void EndTouchPrimary(InputAction.CallbackContext context)
        {
            OnTouchEnd?.Invoke(context.ReadValue<Vector2>());
        }
    }
}
