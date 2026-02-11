using UnityEngine;
using System.Collections.Generic;

namespace VoltaTwins
{
    public class VT_ConveyorBelt : MonoBehaviour
    {
        public enum ConveyorDirection
        {
            FORWARD,
            BACKWARD
        }

        [Header("Direction Settings")]
        [SerializeField] private ConveyorDirection directionMode = ConveyorDirection.FORWARD;

        [Header("Arrow / Material")]
        [SerializeField] private Renderer arrowRenderer;
        private string baseMapProperty = "_BaseMap";

        [Header("Texture Scroll (Offset)")]
        [SerializeField] private float offsetSpeed = 1f;
        [SerializeField] private float maxOffsetY = 5f;

        [Header("Conveyor Move")]
        [SerializeField] private float conveyorSpeed = 2f;

        [Header("Gizmos Settings")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color gizmoColor = Color.cyan;
        [SerializeField] private float arrowLength = 2f;
        [SerializeField] private float arrowHeadSize = 0.3f;

        private Material _material;
        private Vector2 _originalOffset;
        private float _offsetY;

        private readonly HashSet<VT_PlayerController> playersOnBelt = new();

        // ---------- DIRECTION HELPER (local Z -> world) ----------
        private Vector3 GetWorldDirection()
        {
            // Hướng local trên băng chuyền: dọc theo trục Z của nó
            Vector3 localDir = (directionMode == ConveyorDirection.FORWARD)
                ? Vector3.forward
                : Vector3.back;

            return transform.TransformDirection(localDir).normalized;
        }

        void Start()
        {
            if (arrowRenderer != null)
            {
                _material = arrowRenderer.material;

                if (_material.HasProperty(baseMapProperty))
                    _originalOffset = _material.GetTextureOffset(baseMapProperty);
                else
                    _originalOffset = Vector2.zero;
            }
        }

        void Update()
        {
            if (_material == null) return;

            // FORWARD = +1, BACKWARD = -1 (muốn đảo thì đổi dấu ở đây)
            float dirSign = (directionMode == ConveyorDirection.FORWARD) ? 1f : -1f;
            _offsetY = Mathf.Repeat(_offsetY + offsetSpeed * dirSign * Time.deltaTime, maxOffsetY);

            Vector2 offset = _originalOffset;
            offset.y += _offsetY;
            _material.SetTextureOffset(baseMapProperty, offset);
        }

        void FixedUpdate()
        {
            Vector3 velocity = GetWorldDirection() * conveyorSpeed;

            foreach (var playerMovement in playersOnBelt)
            {
                playerMovement?.SetExternalVelocity(velocity);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<VT_PlayerController>(out var player))
            {
                var movement = player.GetComponent<VT_PlayerController>();
                if (movement != null)
                {
                    playersOnBelt.Add(movement);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<VT_PlayerController>(out var player))
            {
                var movement = player.GetComponent<VT_PlayerController>();
                if (movement != null && playersOnBelt.Contains(movement))
                {
                    movement.ClearExternalVelocity();
                    playersOnBelt.Remove(movement);
                }
            }
        }

        void OnDisable()
        {
            foreach (var playerMovement in playersOnBelt)
            {
                if (playerMovement != null)
                {
                    playerMovement.ClearExternalVelocity();
                }
            }
            playersOnBelt.Clear();
        }

        void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Vector3 center = transform.position;
            Vector3 worldDirection = GetWorldDirection();

            Gizmos.color = gizmoColor;
            Vector3 arrowEnd = center + worldDirection * arrowLength;
            Gizmos.DrawLine(center, arrowEnd);

            DrawArrowHead(arrowEnd, worldDirection, arrowHeadSize);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(center + Vector3.up * 0.5f,
                $"Direction: {directionMode}\nSpeed: {conveyorSpeed}");
#endif
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            Vector3 center = transform.position;
            Vector3 worldDirection = GetWorldDirection();

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, 0.2f);

            for (int i = 0; i < 5; i++)
            {
                float t = i / 5f;
                Vector3 pos = center + worldDirection * arrowLength * t;
                Gizmos.DrawWireSphere(pos, 0.1f * (1f - t * 0.5f));
            }
        }

        private void DrawArrowHead(Vector3 tip, Vector3 direction, float size)
        {
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
            if (right.sqrMagnitude < 0.01f)
                right = Vector3.Cross(direction, Vector3.right).normalized;

            Vector3 up = Vector3.Cross(right, direction).normalized;

            Vector3 basePoint = tip - direction * size;

            Vector3[] corners = new Vector3[4]
            {
                basePoint + (right + up) * size * 0.3f,
                basePoint + (-right + up) * size * 0.3f,
                basePoint + (-right - up) * size * 0.3f,
                basePoint + (right - up) * size * 0.3f
            };

            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(tip, corners[i]);
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            }
        }

        void OnValidate()
        {
            if (conveyorSpeed < 0f)
                conveyorSpeed = Mathf.Abs(conveyorSpeed);
        }
    }
}
