using UnityEngine;

[System.Serializable]
public class PlayerMovement : CharacterComponents
{
    private float runSpeed => characterController.characterData.runSpeed; // 5
    private float turnSmoothTime => characterController.characterData.turnSmoothTime; //0.1
    private float gravity => characterController.characterData.gravity; // -9.81

    private float turnSmoothVelocity;
    private float verticalVelocity = 0f;
    bool _canMove = true;
    public void SetCanMove(bool value) => _canMove = value;
    private Vector2 movementInput;

    [SerializeField] private Transform _cameraTransform;

    private Animator _animator => characterController.animator;
    private InputHandler _inputHandler;
    private CharacterController _controller => characterController.characterController;

    private void OnMove(Vector2 input)
    {
        movementInput = input;
    }

    private void OnMoveStart()
    {
        _animator.SetBool("isRunning", _canMove);
        if (_canMove) characterController.characterSound?.PlayFootstepLoop();
    }

    private void OnMoveStop()
    {
        _animator.SetBool("isRunning", false);
        movementInput = Vector2.zero;
        characterController.characterSound?.StopFootstepLoop();
    }

    public override void Initialize(PlayerController pc)
    {
        base.Initialize(pc);

        _cameraTransform = Camera.main != null ? Camera.main.transform : null;
        ValidationUtils.CheckNull(_cameraTransform, "[PlayerController.cs] ---> Cannot find Main Camera");

        _inputHandler = characterController.GetComponent<InputHandler>();
        ValidationUtils.CheckNull(_inputHandler, "[PlayerController.cs] ---> Cannot find InputHandler");
    }

    public override void OnEnable()
    {
        _inputHandler.OnMovementInputChanged += OnMove;
        _inputHandler.OnMovementStart += OnMoveStart;
        _inputHandler.OnMovementStop += OnMoveStop;
    }

    public override void OnDisable()
    {
        _inputHandler.OnMovementInputChanged -= OnMove;
        _inputHandler.OnMovementStart -= OnMoveStart;
        _inputHandler.OnMovementStop -= OnMoveStop;
    }

    public override void Update()
    {
        if (!_canMove) return;

        Vector2 input = movementInput;
        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

        Vector3 movement = Vector3.zero;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(
                characterController.transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );

            characterController.transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            movement = moveDir.normalized * runSpeed * Time.deltaTime;
        }

        if (_controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        movement.y = verticalVelocity * Time.deltaTime;

        _controller.Move(movement);
    }
}
