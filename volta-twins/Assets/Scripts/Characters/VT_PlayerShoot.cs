using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace VoltaTwins
{
    [System.Serializable]
    public class VT_PlayerShoot : VT_PlayerComponents
    {
        [Header("Shoot Settings")]
        [SerializeField] private float shootCooldown = 0.3f;
        [SerializeField] private float raycastMaxDistance = 100f;

        [Header("Raycast Layers")]
        [SerializeField] private LayerMask raycastLayers = ~0;

        [Header("Line Renderer Settings")]
        [SerializeField] private bool showAimLine = true;

        [Header("Aim Settings")]
        [SerializeField] private bool useTouchDragAim = true;
        [SerializeField] private float minAimScreenDistance = 20f;
        [SerializeField] private float aimSmoothSpeed = 0.15f;

        private float lastShootTime;
        private VT_EnergyCore energyBall;
        private Vector3 targetPoint;
        private LineRenderer aimLine;
        private PlayerInputActions input;

        private Camera mainCam;
        private Vector3 aimDirection;
        private Vector2 lastDragScreenPos;
        private bool isDragging = false;

        private static readonly List<RaycastResult> uiRaycastResults = new();

        public override void Initialized(VT_PlayerController controller)
        {
            base.Initialized(controller);

            input = controller.Input;
            aimLine = controller.GetComponent<LineRenderer>();

            if (aimLine != null)
            {
                aimLine.positionCount = 2;
                aimLine.enabled = false;
            }

            mainCam = Camera.main;
            aimDirection = controller.transform.forward;
        }

        public override void PlayerOnEnable()
        {
            VT_LevelManager.Instance.OnShoot += TriggerShoot;

            if (input != null && useTouchDragAim)
            {
                input.movement.Drag.performed += OnDrag;
                input.movement.Drag.canceled += OnDragCanceled;
            }
        }

        public override void PlayerOnDisable()
        {
            VT_LevelManager.Instance.OnShoot -= TriggerShoot;

            if (input != null && useTouchDragAim)
            {
                input.movement.Drag.performed -= OnDrag;
                input.movement.Drag.canceled -= OnDragCanceled;
            }

            if (aimLine != null)
            {
                aimLine.enabled = false;
            }
        }

        public override void PlayerUpdate()
        {
            base.PlayerUpdate();

            // Chỉ update aim khi đang drag
            if (isDragging)
            {
                UpdateAimDirectionFromDrag();
            }

            UpdateRaycastAndLine();

            SetShowAimLine(CanShoot() && controller.HasCore);
        }

        private void OnDrag(InputAction.CallbackContext context)
        {
            if (!useTouchDragAim) return;

            Vector2 dragPos = context.ReadValue<Vector2>();

            if (IsPointerOverUI(dragPos))
                return;

            lastDragScreenPos = dragPos;
            isDragging = true;
        }

        private void OnDragCanceled(InputAction.CallbackContext context)
        {
            if (!useTouchDragAim) return;
            isDragging = false;
        }

        private void UpdateAimDirectionFromDrag()
        {
            if (!useTouchDragAim) return;
            if (!isDragging) return;
            if (mainCam == null) return;
            if (!controller.HasCore) return;

            if (IsPointerOverUI(lastDragScreenPos))
                return;

            Vector3 playerScreenPos3 = mainCam.WorldToScreenPoint(controller.shootPos.position);
            Vector2 playerScreenPos = new(playerScreenPos3.x, playerScreenPos3.y);

            Vector2 delta = lastDragScreenPos - playerScreenPos;

            if (delta.sqrMagnitude < minAimScreenDistance * minAimScreenDistance)
                return;

            Vector2 deltaNorm = delta.normalized;

            Vector3 camForward = mainCam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = mainCam.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            Vector3 worldDir = camRight * deltaNorm.x + camForward * deltaNorm.y;
            worldDir.y = 0f;

            if (worldDir.sqrMagnitude > 0.0001f)
            {
                Vector3 targetAimDir = worldDir.normalized;
                aimDirection = Vector3.Lerp(aimDirection, targetAimDir, aimSmoothSpeed);
                aimDirection.Normalize();
            }
        }

        private bool IsPointerOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

            uiRaycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, uiRaycastResults);
            return uiRaycastResults.Count > 0;
        }

        private void UpdateRaycastAndLine()
        {
            if (!controller.HasCore || energyBall == null || aimLine == null)
            {
                if (aimLine != null)
                    aimLine.enabled = false;
                return;
            }

            bool canShoot = CanShoot();

            Vector3 origin = controller.transform.position;

            Vector3 direction;
            if (useTouchDragAim && aimDirection.sqrMagnitude > 0.0001f)
            {
                direction = aimDirection;
            }
            else
            {
                direction = controller.transform.forward.normalized;
            }

            int mask = raycastLayers.value;
            int ownerLayerMask = 1 << controller.gameObject.layer;
            mask &= ~ownerLayerMask;

            if (Physics.Raycast(origin + Vector3.up, direction, out RaycastHit hit, raycastMaxDistance, mask, QueryTriggerInteraction.Ignore))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = origin + direction * raycastMaxDistance;
            }

            if (showAimLine && canShoot)
            {
                aimLine.enabled = true;

                Vector3 startPos = origin;
                Vector3 endPos = targetPoint;

                float y = controller.shootPos.position.y;
                startPos.y = y;
                endPos.y = y;

                aimLine.SetPosition(0, startPos);
                aimLine.SetPosition(1, endPos);
            }
            else
            {
                aimLine.enabled = false;
            }
        }

        private bool CanShoot()
        {
            if (Time.time - lastShootTime < shootCooldown)
                return false;

            if (energyBall == null || energyBall.transform.parent != controller.shootPos)
                return false;

            return true;
        }

        public void Shoot()
        {
            if (!controller.HasCore) return;
            if (!CanShoot()) return;
            if (energyBall == null) return;

            Vector3 shootDir = targetPoint - controller.shootPos.position;
            shootDir.y = 0f;
            if (shootDir.sqrMagnitude < 0.0001f)
                shootDir = controller.transform.forward;

            shootDir.Normalize();

            energyBall.Shoot(controller, shootDir);
            lastShootTime = Time.time;

            if (aimLine != null)
                aimLine.enabled = false;

            controller.SetHasCore(false);
        }

        private void TriggerShoot()
        {
            if (!controller.HasCore) return;
            if (!CanShoot()) return;
            if (energyBall == null) return;

            Vector3 shootDir = targetPoint - controller.shootPos.position;
            shootDir.y = 0f;

            if (shootDir.sqrMagnitude < 0.0001f)
                shootDir = controller.transform.forward;

            shootDir.Normalize();

            controller.transform.rotation = Quaternion.LookRotation(shootDir, Vector3.up);

            controller.AnimController.TriggerShoot();
        }

        public void SetEnergyBall(VT_EnergyCore ball)
        {
            energyBall = ball;
        }

        public void SetShowAimLine(bool show)
        {
            showAimLine = show;
        }
    }
}
