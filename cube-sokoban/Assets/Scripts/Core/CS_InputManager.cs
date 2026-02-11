using UnityEngine;
using UnityEngine.InputSystem;

namespace CubeSokoban
{
    [DefaultExecutionOrder(-1)]
    public class CS_InputManager : MonoBehaviour
    {
        public static CS_InputManager Instance { get; private set; }

        public delegate void StartTouch(Vector2 position, float time);
        public event StartTouch OnStartTouch;

        public delegate void EndTouch(Vector2 startPos, Vector2 endPos, float time);
        public event EndTouch OnEndTouch;

        private CS_PlayerInputAction inputActions;
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

            inputActions = new CS_PlayerInputAction();
        }

        void OnEnable()
        {
            inputActions.Enable();
        }

        void OnDisable()
        {
            inputActions.Disable();
        }

        void Start()
        {
            inputActions.Movement.PrimaryContact.started += StartTouchPrimary;
            inputActions.Movement.PrimaryContact.canceled += EndTouchPrimary;
        }

        private void StartTouchPrimary(InputAction.CallbackContext ctx)
        {
            Vector2 screenPos = inputActions.Movement.PrimaryPosition.ReadValue<Vector2>();
            _touchStartPos = screenPos;

            OnStartTouch?.Invoke(screenPos, (float)ctx.startTime);
        }

        private void EndTouchPrimary(InputAction.CallbackContext ctx)
        {
            if (OnEndTouch == null) return;

            Vector2 screenPos = inputActions.Movement.PrimaryPosition.ReadValue<Vector2>();

            OnEndTouch(_touchStartPos, screenPos, (float)ctx.time);
        }
    }
}
