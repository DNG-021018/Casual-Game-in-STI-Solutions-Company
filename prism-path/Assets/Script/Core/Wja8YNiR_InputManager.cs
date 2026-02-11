using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_InputManager : MonoBehaviour
    {
        [SerializeField] private LayerMask _tileMask;
        [SerializeField] private LayerMask _mirrorMask;

        PlayerInput _playerInput;

        private Camera _cam;
        private InputAction _pointerPress;

        private static readonly List<RaycastResult> _uiRaycastResults = new();
        private bool _pressBeganOverUI;

        void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _cam = Camera.main;
        }

        void OnEnable()
        {
            _playerInput.actions.Enable();
            _pointerPress = _playerInput.actions["Pointer"];

            if (_pointerPress != null)
            {
                _pointerPress.Enable();
                _pointerPress.started += OnPressStarted;
                _pointerPress.canceled += OnPressCanceled;
            }
        }

        void OnDisable()
        {
            if (_pointerPress != null)
            {
                _pointerPress.started -= OnPressStarted;
                _pointerPress.canceled -= OnPressCanceled;
                _pointerPress.Disable();
            }

            _playerInput.actions.Disable();
        }

        private void OnPressStarted(InputAction.CallbackContext ctx)
        {
            var pos = GetPointerPos();
            _pressBeganOverUI = IsPointerOverUI(pos);
        }

        private void OnPressCanceled(InputAction.CallbackContext ctx)
        {
            var pos = GetPointerPos();
            _pressBeganOverUI = IsPointerOverUI(pos);
            if (!float.IsFinite(pos.x) || !float.IsFinite(pos.y)) return;
            if (!_cam.pixelRect.Contains(pos)) return;
            if (_pressBeganOverUI || IsPointerOverUI(pos)) return;

            Ray ray = _cam.ScreenPointToRay(pos);

            if (Physics.Raycast(ray, out var hitMirror, 1000f, _mirrorMask, QueryTriggerInteraction.Collide))
            {
                // Debug.DrawLine(ray.origin, hitMirror.point, Color.cyan, 0.5f);
                if (hitMirror.collider.TryGetComponent(out Wja8YNiR_Mirror mirror))
                    mirror.Interact();
                return;
            }

            if (Physics.Raycast(ray, out var hitTile, 1000f, _tileMask, QueryTriggerInteraction.Collide))
            {
                // Debug.DrawLine(ray.origin, hitTile.point, Color.green, 0.5f);
                if (hitTile.collider.TryGetComponent(out Wja8YNiR_Tile tile) && Wja8YNiR_GameManager.Instance?.GetState() == GameState.Playing)
                {
                    tile.Interact();
                }
                return;
            }

            Wja8YNiR_Mirror.ClearCurrentHighlight();
        }

        private Vector2 GetPointerPos()
        {
            if (Pointer.current != null)
                return Pointer.current.position.ReadValue();

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                return Touchscreen.current.primaryTouch.position.ReadValue();

            return new Vector2(float.NaN, float.NaN);
        }

        private bool IsPointerOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;

            var data = new PointerEventData(EventSystem.current) { position = screenPos };
            _uiRaycastResults.Clear();
            EventSystem.current.RaycastAll(data, _uiRaycastResults);
            return _uiRaycastResults.Count > 0;
        }
    }
}
