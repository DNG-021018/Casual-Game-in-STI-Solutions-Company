using System;
using UnityEngine;

namespace HexaStack
{
    public class HS_StackController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LayerMask hexagonlayerMask;
        [SerializeField] private LayerMask gridHexagonlayerMask;
        [SerializeField] private LayerMask groundlayerMask;
        private HS_HexStack currentStack;
        private Vector3 currentStackInitialPos;

        [Header("Data")]
        private HS_GridCell targetCell;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip PlacedClip;
        [SerializeField] private AudioClip pickupClick;

        public static Action<HS_GridCell> OnStackPlaced;

        private HS_AudioManager audioManager;

        void Awake()
        {
            audioManager = HS_AudioManager.Instance;
        }

        void Update()
        {
            if (HS_GameManager.Instance == null) return;
            if (HS_GameManager.Instance.GetState() != GameState.Play) return;

            ManageControl();
        }

        private void ManageControl()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        ManageTouchDown(touch.position);
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        ManageTouchDrag(touch.position);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        ManageTouchUp();
                        break;
                }
            }
        }

        private void ManageTouchDown(Vector2 screenPosition)
        {
            Physics.Raycast(GetRayFromScreen(screenPosition), out RaycastHit hit, 500, hexagonlayerMask);

            if (hit.collider == null)
            {
                return;
            }

            audioManager.PlaySfx(pickupClick);
            currentStack = hit.collider.GetComponent<HS_Hexagon>().HexStack;
            currentStackInitialPos = currentStack.transform.position;
        }

        private void ManageTouchDrag(Vector2 screenPosition)
        {
            if (currentStack == null)
            {
                return;
            }

            Physics.Raycast(GetRayFromScreen(screenPosition), out RaycastHit hit, 500, gridHexagonlayerMask);

            if (hit.collider == null)
            {
                DraggingAboveGround(screenPosition);
            }
            else
            {
                DraggingAboveGridCell(hit, screenPosition);
            }
        }

        private void DraggingAboveGround(Vector2 screenPosition)
        {
            Physics.Raycast(GetRayFromScreen(screenPosition), out RaycastHit hit, 500, groundlayerMask);

            if (hit.collider == null)
            {
                return;
            }

            if (targetCell != null)
            {
                targetCell.HightLight(false);
                targetCell = null;
            }

            Vector3 currentStackTargetPos = hit.point.With(y: 2);
            currentStack.transform.position = Vector3.MoveTowards(currentStack.transform.position, currentStackTargetPos, Time.deltaTime * 100f);
        }

        private void DraggingAboveGridCell(RaycastHit hit, Vector2 screenPosition)
        {
            HS_GridCell gridCell = hit.collider.GetComponent<HS_GridCell>();

            if (gridCell.IsOccupied)
            {
                DraggingAboveGround(screenPosition);
            }
            else
            {
                DraggingAboveNonOccupieGridCell(gridCell, screenPosition);
            }
        }

        private void DraggingAboveNonOccupieGridCell(HS_GridCell gridCell, Vector2 screenPosition)
        {
            Physics.Raycast(GetRayFromScreen(screenPosition), out RaycastHit hit, 500, groundlayerMask);

            if (hit.collider == null)
            {
                return;
            }

            if (targetCell != null && targetCell != gridCell)
            {
                targetCell.HightLight(false);
            }

            Vector3 currentStackTargetPos = hit.point.With(y: 2);
            currentStack.transform.position = Vector3.MoveTowards(currentStack.transform.position, currentStackTargetPos, Time.deltaTime * 100f);

            targetCell = gridCell;
            targetCell.HightLight(true);
        }

        private void ManageTouchUp()
        {
            if (currentStack == null)
            {
                return;
            }

            if (targetCell == null)
            {
                currentStack.transform.position = currentStackInitialPos;
                currentStack = null;
                targetCell = null;
                return;
            }

            currentStack.transform.position = targetCell.transform.position.With(y: 0.2f);
            currentStack.transform.SetParent(targetCell.transform);
            currentStack.Place();

            targetCell.AssignStack(currentStack);

            OnStackPlaced?.Invoke(targetCell);
            audioManager.PlaySfx(PlacedClip);
            if (targetCell != null)
            {
                targetCell.HightLight(false);
            }

            targetCell = null;
            currentStack = null;
        }

        private Ray GetRayFromScreen(Vector2 screenPosition) => Camera.main.ScreenPointToRay(screenPosition);
    }
}