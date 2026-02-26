using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace DoublesideZ
{
    public class DZ_PlayerInput : MonoBehaviour
    {
        public event Action<TapSide> OnTap;

        private GameInput input;

        void Awake()
        {
            input = new GameInput();
        }

        void OnEnable()
        {
            input.Enable();
            input.Gameplay.Tap.performed += OnTapPerformed;
        }

        void OnDisable()
        {
            input.Gameplay.Tap.performed -= OnTapPerformed;
            input.Disable();
        }

        private void OnTapPerformed(InputAction.CallbackContext ctx)
        {
            Vector2 screenPos = input.Gameplay.PointerPosition.ReadValue<Vector2>();
            DetectSide(screenPos);
        }

        private void DetectSide(Vector2 screenPos)
        {
            float screenWidth = Screen.width;
            float screenCenter = screenWidth / 2f;

            if (screenPos.x < screenCenter)
                OnTap?.Invoke(TapSide.Left);
            else
                OnTap?.Invoke(TapSide.Right);
        }
    }
}
