using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

public class Character : MonoBehaviour, IMovementModifier
{
    [Header("Components")]
    public InputController inputController;
    public CharacterController characterController;
    public Animator animator;
    public CharacterStateMachine characterStateMachine;
    public GameObject rayPoint;
    public GameObject _arrow;
    private Transform _eggTarget;

    [SerializeField] private AudioClip stripClip;

    [Header("Particles"), Space(10)]
    [SerializeField] private ParticleSystem _dustParticle;
    [SerializeField] private ParticleSystem _dustStrippedParticle;
    [SerializeField] private ParticleSystem _dustFightingParticle;
    [SerializeField] private ParticleSystem[] _textParticles;

    [Header("TimeLine"), Space(10)]
    [SerializeField] private PlayableDirector losingTimeline;

    [Header("Input Settings"), Space(10)]
    public Vector2 inputDirection;
    public float inputAngle;
    private Vector3 lookDirection;

    [Header("Settings"), Space(10)]
    public float moveSpeed;  // Tốc độ hiện tại
    private float baseMovementSpeed;  // Tốc độ cơ bản
    private float currentSpeedMultiplier = 1f;
    public float speedUpMultiplier = 1.5f;
    public float speedUpDuration = 4f;
    public float speedUpTimer = 0f;
    public float speedDownMultiplier = 0.5f;
    public float speedDownDuration = 2f;
    public float speedDownTimer = 0f;
    public float rotationSpeed;
    public float jumpForce;
    public bool isStripped = false;
    public bool IsGetCatch = false;
    private bool isJumping = false;
    private bool isSlowDown = false;
    private float verticalVelocity = -1f;
    private float gravity = -4.9f;
    private float cameraOffset;
    public bool isMoving;
    public LayerMask groundLayerMask;
    [SerializeField] private bool isGrounded;
    private bool isGameFinish = false;
    public event Action OnGetCatch;

    private void Start()
    {
        Camera camera = Camera.main;
        Vector3 playerStartRotation = camera.transform.eulerAngles;
        cameraOffset = playerStartRotation.y;
        if (inputController == null)
        {
            inputController = FindFirstObjectByType<InputController>();
        }

        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (characterStateMachine == null)
        {
            characterStateMachine = GetComponent<CharacterStateMachine>();
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        inputController.OnJumpButtonPressedEvent += HandleJump;
        inputController.OnSprintButtonPressedEvent += HandleSprint;
        isJumping = false;
        verticalVelocity = -1f; // Đặt giá trị ban đầu cho vận tốc Y
        baseMovementSpeed = moveSpeed;
        GamePlayController.Instance.OnLevelCompleted += OnLevelComplete;
    }

    private void OnLevelComplete(bool obj)
    {
        isGameFinish = true;
    }

    private void Update()
    {
        HandleInput();
        HandleAnimation();
        HandleJumpMovement();
        HandleRotateArrow();
    }

    private void HandleAnimation()
    {
        if (isStripped || IsGetCatch)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsTag("StandUp") && stateInfo.normalizedTime >= 0.94f)
            {
                isStripped = false;
                Debug.Log("Character is no longer stripped.");
            }
            return;
        }
        animator.SetFloat("Running", inputDirection.magnitude);
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        isGrounded = Physics.Raycast(rayPoint.transform.position, Vector3.down, out hit, .3f, groundLayerMask);
        animator.SetBool("IsGrounded", isGrounded);
        HandleRotation();
        HandleMovement();
    }
    private void HandleRotation()
    {
        if (isStripped || IsGetCatch)
            return;
        if (inputDirection.sqrMagnitude < 0.01f)
            return;
        Quaternion targetRotation = Quaternion.Euler(0, inputAngle, 0);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void HandleMovement()
    {
        if (isStripped || IsGetCatch)
            return;
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            float angleRad = inputAngle * Mathf.Deg2Rad;
            // Chỉ di chuyển trên mặt phẳng XZ
            Vector3 moveDir = new Vector3(Mathf.Sin(angleRad), 0, Mathf.Cos(angleRad));
            characterController.Move(moveDir * moveSpeed * Time.fixedDeltaTime);
            isMoving = true;
            if (!_dustParticle.isPlaying)
                _dustParticle.Play();
        }
        else
        {
            isMoving = false;
            if (_dustParticle.isPlaying)
            {
                _dustParticle.Stop();
            }
        }
    }

    private void HandleInput()
    {
        inputDirection.x = inputController.joystick.GetHorizontalAxisRaw();
        inputDirection.y = inputController.joystick.GetVerticalAxisRaw();
        inputDirection.Normalize();
        // 90 là angle camera, xoay góc camera thì thay angle
        inputAngle = inputController.joystick.GetAngle() + cameraOffset;
    }

    private void HandleJump()
    {
        Debug.Log("HandleJump1: " + isStripped + " " + IsGetCatch + " " + isJumping + " " + !isGrounded);
        if (isStripped || IsGetCatch || isJumping || !isGrounded || isSlowDown) return;
        Debug.Log("HandleJump2: " + isStripped + " " + IsGetCatch + " " + isJumping + " " + !isGrounded);
        isJumping = true;
        animator.SetTrigger("Jump");
        verticalVelocity = jumpForce;
    }
    private void HandleJumpMovement()
    {
        if (isGrounded)
        {
            if (isJumping)
            {
                isJumping = false;
            }
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }
        }
        else
        {
            Debug.Log("NoGrounded");
            verticalVelocity += gravity * Time.fixedDeltaTime;
        }

        // Chỉ áp dụng vận tốc Y
        Vector3 jumpMove = new Vector3(0, verticalVelocity, 0) * Time.fixedDeltaTime;
        characterController.Move(jumpMove);
    }

    private void HandleSprint()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (isGameFinish)
            return;
        if (other.tag.Equals("Egg"))
            {
                if (IsGetCatch || isStripped) return;
                animator.SetTrigger("PickUp");
            }
            else if (other.tag.Equals("Hoe"))
            {
                if (isStripped || IsGetCatch) return;
                isStripped = true;
                Debug.Log("OnTriggerEnter: " + other.name);
                animator.SetTrigger("Strip");
                _dustStrippedParticle.Play();
            StartCoroutine(StartSoundDelay(stripClip));
                _dustParticle.Stop();
            }
            else if (other.tag.Equals("Chicken"))
            {
                Debug.Log("OnTriggerEnter: " + other.name);
                if (IsGetCatch) return;
                IsGetCatch = true;
                animator.SetTrigger("Catch");
                OnGetCatch?.Invoke();
                StartCoroutine(ShowFightingParticle());
                StartCoroutine(StartRandomTextParticles());
                _dustParticle.Stop();
            }
    }


    private IEnumerator StartSoundDelay(AudioClip audioClip)
    {
        yield return new WaitForSeconds(0.2f);
        SoundManager.Instance.PlaySFX(audioClip);
    }
    private void HandleRotateArrow()
    {
        if (_eggTarget is not null)
        {
            Vector3 targetPosition = _eggTarget.position;
            Vector3 arrowPosition = _arrow.transform.position;
            Vector3 direction = new Vector3(
                targetPosition.x - arrowPosition.x,
                0f,
                targetPosition.z - arrowPosition.z
            );
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                _arrow.transform.rotation = Quaternion.Euler(
                    0f,
                    targetRotation.eulerAngles.y,
                    0f
                );
            }
        }
    }

    public void ChangeEggTarget(Egg eggTarget)
    {
        _eggTarget = eggTarget.transform;
    }

    IEnumerator ShowFightingParticle()
    {
        losingTimeline.gameObject.SetActive(false);
        _dustFightingParticle.Play();
        _arrow.gameObject.SetActive(false);
        yield return new WaitForSeconds(3f);

        ChickenSpawnManager.Instance?.HideAllChicken();
        losingTimeline.gameObject.SetActive(true);
        losingTimeline.Play();

        yield return new WaitForSeconds(3f);
        _dustFightingParticle.Stop();
    }
    IEnumerator StartRandomTextParticles()
    {
        float elapsed = 0f;
        yield return new WaitForSeconds(1f);
        while (elapsed < 3f)
        {
            int randomIndex = UnityEngine.Random.Range(0, _textParticles.Length);
            ParticleSystem randomParticle = _textParticles[randomIndex];
            randomParticle.Play();
            yield return new WaitForSeconds(0.5f);
            randomParticle.Stop();
            elapsed += 0.5f;
        }
    }

    private void OnDestroy()
    {
        if (inputController != null)
        {
            inputController.OnJumpButtonPressedEvent -= HandleJump;
            inputController.OnSprintButtonPressedEvent -= HandleSprint;
        }
        if (OnGetCatch != null)
        {
            OnGetCatch = null;
        }
    }

    public void ApplySpeedModifier(float multiplier)
    {
        currentSpeedMultiplier = multiplier;
        moveSpeed = baseMovementSpeed * currentSpeedMultiplier;
        isSlowDown = true;
        animator.SetBool("isInjured", true);
    }

    public void RemoveSpeedModifier()
    {
        currentSpeedMultiplier = 1f;
        moveSpeed = baseMovementSpeed;
        isSlowDown = false;
        animator.SetBool("isInjured", false);
    }
}
