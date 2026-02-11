using System.Collections;
using UnityEngine;

namespace CubeSokoban
{
    public class CS_PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _rollSpeed = 5;
        [SerializeField] private float _swipeThreshold = 0.5f;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float raycastLength = 2f;
        [SerializeField] private AudioClip PlayerMove;

        private CS_PlayerVisual _playerVisual;
        private CS_AudioManager _audioManager;
        private bool _isMoving;

        void Awake()
        {
            _audioManager = CS_AudioManager.Instance;
            _playerVisual = GetComponent<CS_PlayerVisual>();
        }

        private void OnEnable()
        {
            CS_InputManager.Instance.OnEndTouch += HandleEndTouch;
        }

        private void OnDisable()
        {
            CS_InputManager.Instance.OnEndTouch -= HandleEndTouch;
        }

        private void HandleEndTouch(Vector2 startPos, Vector2 endPos, float time)
        {
            if (CS_GameManager.Instance?.GetState() != GameState.Play) return;
            if (_isMoving) return;

            Vector2 swipeDelta = endPos - startPos;

            if (swipeDelta.magnitude < _swipeThreshold) return;

            Vector3 moveDir;

            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                moveDir = swipeDelta.x > 0 ? Vector3.right : Vector3.left;
            }
            else
            {
                moveDir = swipeDelta.y > 0 ? Vector3.forward : Vector3.back;
            }

            Vector3 nextPos = transform.position + moveDir;

            if (!IsBlocking(nextPos)) return;

            CS_Box box = IsTouchBox(nextPos);
            if (box != null)
            {
                if (box.TryToMove(startPos, endPos, _rollSpeed))
                {
                    Assemble(moveDir, nextPos);
                    return;
                }
                else
                {
                    return;
                }
            }

            Assemble(moveDir, nextPos);
        }

        private bool IsBlocking(Vector3 pos)
        {
            return Physics.Raycast(pos + Vector3.up, Vector3.down, raycastLength, _groundLayer);
        }

        private CS_Box IsTouchBox(Vector3 pos)
        {
            if (Physics.Raycast(pos + Vector3.up, Vector3.down, out RaycastHit hit, raycastLength))
            {
                return hit.collider.GetComponent<CS_Box>();
            }
            return null;
        }

        private void Assemble(Vector3 dir, Vector3 target)
        {
            var anchor = transform.position + (Vector3.down + dir) * 0.5f;
            var axis = Vector3.Cross(Vector3.up, dir);
            StartCoroutine(Roll(anchor, axis, target));
        }

        private IEnumerator Roll(Vector3 anchor, Vector3 axis, Vector3 target)
        {
            if (_audioManager != null)
            {
                _audioManager.PlaySfx(PlayerMove);
            }
            _isMoving = true;
            for (var i = 0; i < 90 / _rollSpeed; i++)
            {
                transform.RotateAround(anchor, axis, _rollSpeed);
                _playerVisual.PlayJellyTween();
                yield return new WaitForSecondsRealtime(0.01f);
            }
            transform.position = target;
            _isMoving = false;
        }

        // private void OnDrawGizmos()
        // {
        //     Vector3[] directions = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        //     Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow };

        //     for (int i = 0; i < directions.Length; i++)
        //     {
        //         Vector3 nextPos = transform.position + directions[i];
        //         Vector3 rayStart = nextPos + Vector3.up;
        //         Vector3 rayEnd = nextPos + Vector3.down * 2;

        //         Gizmos.color = colors[i];
        //         Gizmos.DrawLine(rayStart, rayEnd);

        //         if (Physics.Raycast(rayStart, Vector3.down, raycastLength, _groundLayer))
        //         {
        //             Gizmos.color = colors[i];
        //             Gizmos.DrawSphere(nextPos, 0.1f);
        //         }
        //     }
        // }
    }
}
