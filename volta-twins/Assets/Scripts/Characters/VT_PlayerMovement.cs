using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VoltaTwins
{
    [Serializable]
    public class VT_PlayerMovement : VT_PlayerComponents
    {
        Vector2 moveVector;
        VT_PlayerConfig config;
        VT_PlayerController c;
        PlayerInputActions input;
        VT_PlayerAnimationController anim;
        CharacterController characterController;

        float moveSpeed => config.moveSpeed;

        [SerializeField] float speedChangeRate = 10f;
        [SerializeField] float maxAnimBlendSpeed = 8f;
        [SerializeField] float rotationSmoothTime = 0.1f;

        float _rotationVelocity;
        float currentSpeed;
        bool canMove = true;

        private Vector3 externalVelocity = Vector3.zero;

        public override void Initialized(VT_PlayerController controller)
        {
            base.Initialized(controller);
            c = controller;
            config = c.PlayerConfig;
            characterController = c.CharacterController;
            anim = c.AnimController;
            input = c.Input;
        }

        public override void PlayerStart()
        {
            input.movement.Input.performed += OnInput;
            input.movement.Input.canceled += OnInputCanceled;

            EnableMovement();
        }

        private void OnInput(InputAction.CallbackContext ctx)
        {
            moveVector = ctx.ReadValue<Vector2>();
        }

        public void EnableMovement()
        {
            canMove = true;
        }

        public void DisableMovement()
        {
            canMove = false;
        }

        private void OnInputCanceled(InputAction.CallbackContext ctx)
        {
            moveVector = Vector2.zero;
        }

        public override void PlayerFixedUpdate()
        {
            if (VT_LevelManager.Instance != null && VT_LevelManager.Instance.isGameFinish)
            {
                DisableMovement();
                currentSpeed = 0f;
                externalVelocity = Vector3.zero;
                if (anim != null) anim.SetMoveSpeed(0f);
                return;
            }
            GravityApplier();
            ApplyExternalVelocity();
            if (!canMove) return;
            Move();
        }

        private void Move()
        {
            if (anim == null || config == null) return;

            float inputMagnitude = Mathf.Clamp01(moveVector.magnitude);

            float targetSpeed = moveSpeed * inputMagnitude;

            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                speedChangeRate * Time.deltaTime
            );

            if (inputMagnitude > 0.01f)
            {
                float cameraYaw = 0f;
                var cam = Camera.main;
                if (cam != null)
                {
                    cameraYaw = cam.transform.eulerAngles.y;
                }

                float targetRotation = Mathf.Atan2(moveVector.x, moveVector.y) * Mathf.Rad2Deg + cameraYaw;

                float rotation = Mathf.SmoothDampAngle(
                    c.transform.eulerAngles.y,
                    targetRotation,
                    ref _rotationVelocity,
                    rotationSmoothTime
                );

                c.transform.rotation = Quaternion.Euler(0f, rotation, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;

                characterController.Move(currentSpeed * Time.deltaTime * moveDir);
            }

            float normalizedSpeed01 = (moveSpeed > 0.01f) ? currentSpeed / moveSpeed : 0f;

            float animSpeed = normalizedSpeed01 * maxAnimBlendSpeed;
            anim.SetMoveSpeed(animSpeed);
        }

        public void SetExternalVelocity(Vector3 velocity)
        {
            externalVelocity = velocity;
        }

        public void ClearExternalVelocity()
        {
            externalVelocity = Vector3.zero;
        }

        private void ApplyExternalVelocity()
        {
            if (externalVelocity.sqrMagnitude > 0.001f)
            {
                characterController.Move(externalVelocity * Time.deltaTime);
            }
        }

        private void GravityApplier()
        {
            if (characterController.isGrounded && externalVelocity.y < 0f)
            {
                externalVelocity.y = 0f;
            }

            externalVelocity.y += -9.8f * Time.deltaTime;
        }
    }
}
