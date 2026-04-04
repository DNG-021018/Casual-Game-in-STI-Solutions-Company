using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace CataFury
{
    public class CF_InputManager : MonoBehaviour
    {
        [Header("Swipe Settings")]
        [SerializeField] private float minSwipeDistance = 50f;

        [Tooltip("Tỉ lệ trục chính / trục phụ tối thiểu. 1.7 = chặt vừa. Tăng nếu vẫn nhầm.")]
        [SerializeField] private float axisRatio = 1.7f;

        [Tooltip("Góc xoay layout (độ). 45 = hình chữ X, 0 = hình dấu +")]
        [SerializeField] private float layoutAngle = 45f;

        private Input _input;
        private CF_PlayerController _playerController;

        private Vector2 _touchStart;
        private bool _tracking;
        private bool _inputEnabled;

        void Awake()
        {
            _playerController = ServiceLocator.Get<CF_PlayerController>();
            _input = new Input();
        }

        void OnEnable() => CF_GameManager.OnGameStateChanged += HandleGameState;

        void OnDestroy()
        {
            CF_GameManager.OnGameStateChanged -= HandleGameState;
            _input.Player.Disable();
            _input.Dispose();
        }

        private void HandleGameState(GameState state)
        {
            _inputEnabled = state == GameState.Play;

            if (_inputEnabled)
                _input.Player.Enable();
            else
            {
                _input.Player.Disable();
                _tracking = false;
            }
        }

        void Update()
        {
            if (!_inputEnabled) return;

            TouchControl touch = Touchscreen.current?.primaryTouch;

            if (touch != null)
            {
                HandleTouch(
                    touch.press.wasPressedThisFrame,
                    touch.press.wasReleasedThisFrame,
                    touch.position.ReadValue()
                );
            }
            else if (Mouse.current != null)
            {
                HandleTouch(
                    Mouse.current.leftButton.wasPressedThisFrame,
                    Mouse.current.leftButton.wasReleasedThisFrame,
                    Mouse.current.position.ReadValue()
                );
            }
        }

        private void HandleTouch(bool pressed, bool released, Vector2 position)
        {
            if (pressed)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                _touchStart = position;
                _tracking = true;
            }
            else if (released && _tracking)
            {
                _tracking = false;

                Vector2 delta = position - _touchStart;
                if (delta.magnitude < minSwipeDistance) return;

                // Xoay delta vào không gian của layout để các hướng X-layout
                // trở thành trục ngang/dọc → phân tích dễ và chính xác hơn
                Vector2 rotated = RotateDelta(delta, -layoutAngle);

                float absX = Mathf.Abs(rotated.x);
                float absY = Mathf.Abs(rotated.y);
                float major = Mathf.Max(absX, absY);
                float minor = Mathf.Min(absX, absY);

                // Bỏ qua nếu quá chéo (nằm trong dead zone giữa 2 lane)
                if (major < minor * axisRatio) return;

                PlayerDirection dir = GetSwipeDirection(rotated);
                _playerController?.MovePlayer(dir);
            }
        }

        private Vector2 RotateDelta(Vector2 delta, float angleDegrees)
        {
            float rad = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(
                delta.x * cos - delta.y * sin,
                delta.x * sin + delta.y * cos
            );
        }

        private PlayerDirection GetSwipeDirection(Vector2 rotatedDelta)
        {
            if (Mathf.Abs(rotatedDelta.x) >= Mathf.Abs(rotatedDelta.y))
                return rotatedDelta.x > 0 ? PlayerDirection.Right : PlayerDirection.Left;
            else
                return rotatedDelta.y > 0 ? PlayerDirection.Up : PlayerDirection.Down;
        }
    }
}