using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VertiblockPass
{
    [DefaultExecutionOrder(-10)]
    public class VP_InputManager : MonoBehaviour
    {
        public static VP_InputManager Instance { get; private set; }

        public delegate void StartTouch(Vector2 position, float time);
        public event StartTouch OnStartTouch;

        public delegate void EndTouch(Vector2 startPos, Vector2 endPos, float time);
        public event EndTouch OnEndTouch;

        public delegate void DoubleTouch();
        public event DoubleTouch OnDoubleTap;

        [Header("Input Settings")]
        [SerializeField] private float doubleTapDelay = 0.3f; // Delay sau double tap
        [SerializeField] private float minSwipeDistance = 50f; // Khoảng cách tối thiểu để tính là swipe

        private VP_PlayerInputActions inputActions;
        private Vector2 _touchStartPos;
        private float _lastTapTime;
        private bool _isDoubleTapPending;
        private float _suppressSwipeUntil = 0f;

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

            inputActions = new VP_PlayerInputActions();
        }

        void OnEnable()
        {
            inputActions.Enable();
            inputActions.Movement.PrimaryContact.started += StartTouchPrimary;
            inputActions.Movement.PrimaryContact.canceled += EndTouchPrimary;
            inputActions.Movement.DoubleTap.performed += StartDoubleTap;
        }

        void OnDisable()
        {
            inputActions.Disable();
            inputActions.Movement.PrimaryContact.started -= StartTouchPrimary;
            inputActions.Movement.PrimaryContact.canceled -= EndTouchPrimary;
            inputActions.Movement.DoubleTap.performed -= StartDoubleTap;
        }

        private void StartDoubleTap(InputAction.CallbackContext context)
        {
            // Mark double tap occurred and suppress swipe handling for the doubleTap window
            _isDoubleTapPending = true;
            _suppressSwipeUntil = Time.time + doubleTapDelay;
            OnDoubleTap?.Invoke();

            StartCoroutine(ClearDoubleTapFlag());
        }

        private IEnumerator ClearDoubleTapFlag()
        {
            yield return new WaitForSeconds(doubleTapDelay);
            _isDoubleTapPending = false;
        }

        private void StartTouchPrimary(InputAction.CallbackContext ctx)
        {
            // Read start position; sometimes PrimaryPosition can be (0,0) on the very first tap,
            // so fall back to Touchscreen primary touch if needed.
            Vector2 screenPos = inputActions.Movement.PrimaryPosition.ReadValue<Vector2>();
            if (screenPos.sqrMagnitude < 1f)
            {
                var ts = UnityEngine.InputSystem.Touchscreen.current;
                if (ts != null && ts.primaryTouch.press.isPressed)
                {
                    screenPos = ts.primaryTouch.position.ReadValue();
                }
            }

            _touchStartPos = screenPos;
            OnStartTouch?.Invoke(screenPos, (float)ctx.startTime);
        }

        private void EndTouchPrimary(InputAction.CallbackContext ctx)
        {
            // If a double-tap was detected recently, suppress swipe handling for that window
            if (Time.time <= _suppressSwipeUntil)
            {
                return;
            }

            if (OnEndTouch == null) return;

            Vector2 screenPos = inputActions.Movement.PrimaryPosition.ReadValue<Vector2>();

            float swipeDistance = Vector2.Distance(_touchStartPos, screenPos);
            if (swipeDistance < minSwipeDistance)
            {
                // Not a swipe — treat as tap, do nothing here
                return;
            }

            OnEndTouch(_touchStartPos, screenPos, (float)ctx.time);
        }
    }
}