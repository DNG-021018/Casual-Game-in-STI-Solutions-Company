using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace CB_CubeRunner
{
    public class CR_InputTouchController : MonoBehaviour
    {
        private CR_PlayerMovement playerMovement;
        private int _activeFingerId = -1;
        private bool _isHolding = false;
        private Coroutine _spamRoutine;

        void Awake()
        {
            playerMovement = GetComponent<CR_PlayerMovement>();
        }

        void OnEnable()
        {
            ETouch.EnhancedTouchSupport.Enable();
            TouchSimulation.Enable();

            ETouch.Touch.onFingerDown += OnFingerDown;
            ETouch.Touch.onFingerUp += OnFingerUp;
        }

        void OnDisable()
        {
            ETouch.Touch.onFingerDown -= OnFingerDown;
            ETouch.Touch.onFingerUp -= OnFingerUp;

            TouchSimulation.Disable();
            ETouch.EnhancedTouchSupport.Disable();
        }

        bool IsGamePlayable()
        {
            return CB_GameManager.Instance != null &&
                   CB_GameManager.Instance.GetState() == GameState.Play;
        }

        private void OnFingerDown(ETouch.Finger finger)
        {
            if (!IsGamePlayable())
                return;

            if (_activeFingerId != -1) return;

            Vector2 pos = finger.screenPosition;

            if (IsPointerOverUI(pos))
                return;

            _activeFingerId = finger.index;
            _isHolding = true;

            if (_spamRoutine == null)
            {
                _spamRoutine = StartCoroutine(SpamMoveRoutine());
            }
        }

        private void OnFingerUp(ETouch.Finger finger)
        {
            if (finger.index != _activeFingerId) return;

            _isHolding = false;

            if (_spamRoutine != null)
            {
                StopCoroutine(_spamRoutine);
                _spamRoutine = null;
            }

            _activeFingerId = -1;
        }

        private IEnumerator SpamMoveRoutine()
        {
            while (_isHolding)
            {
                if (!IsGamePlayable())
                    break;

                if (TryGetActiveTouch(out var touch))
                {
                    Vector2 pos = touch.screenPosition;

                    if (!IsPointerOverUI(pos))
                    {
                        MoveDirections dir = DecideDirection(pos);
                        TryMove(dir);
                    }
                }
                else
                {
                    break;
                }

                yield return null;
            }

            _isHolding = false;
            _activeFingerId = -1;
            _spamRoutine = null;
        }

        private bool TryGetActiveTouch(out ETouch.Touch touch)
        {
            foreach (var t in ETouch.Touch.activeTouches)
            {
                if (t.finger.index == _activeFingerId)
                {
                    touch = t;
                    return true;
                }
            }

            touch = default;
            return false;
        }

        private MoveDirections DecideDirection(Vector2 screenPos)
        {
            float half = Screen.width * 0.5f;
            return (screenPos.x < half) ? MoveDirections.LEFT : MoveDirections.RIGHT;
        }

        private void TryMove(MoveDirections dir)
        {
            if (playerMovement == null) return;
            if (!IsGamePlayable()) return;

            playerMovement.Assemble(dir);
        }

        private bool IsPointerOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;

            PointerEventData ped = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);

            return results != null && results.Count > 0;
        }
    }
}
