using UnityEngine;
using UnityEngine.InputSystem;

namespace bJakGZQ3_Outer_World
{
    [DefaultExecutionOrder(-1)]
    public class bJakGZQ3_InputSystem : MonoBehaviour
    {
        public static bJakGZQ3_InputSystem Instance { get; private set; }

        public delegate void StartTouch(Vector2 position, float time);
        public event StartTouch OnStartTouch;

        public delegate void EndTouch(Vector2 position, float time);
        public event EndTouch OnEndTouch;

        private bJakGZQ3_PlayerInputAction inputActions;
        private Camera _camera;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            inputActions = new();
            _camera = Camera.main;
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
            inputActions.Player.PrimaryContact.performed += ctx => StartTouchPrimary(ctx);
            inputActions.Player.PrimaryContact.canceled += ctx => EndTouchPrimary(ctx);
        }

        private void StartTouchPrimary(InputAction.CallbackContext ctx)
        {
            if (OnStartTouch != null)
            {
                Vector2 playerTouch = inputActions.Player.PrimaryPosition.ReadValue<Vector2>();
                Vector2 touchPos = bJakGZQ3_Utils.ScreenToWorld(_camera, playerTouch);
                OnStartTouch(touchPos, (float)ctx.startTime);
            }
        }

        private void EndTouchPrimary(InputAction.CallbackContext ctx)
        {
            if (OnEndTouch != null)
            {
                Vector2 playerTouch = inputActions.Player.PrimaryPosition.ReadValue<Vector2>();
                Vector2 touchPos = bJakGZQ3_Utils.ScreenToWorld(_camera, playerTouch);
                OnEndTouch(touchPos, (float)ctx.time);
            }
        }
    }
}
