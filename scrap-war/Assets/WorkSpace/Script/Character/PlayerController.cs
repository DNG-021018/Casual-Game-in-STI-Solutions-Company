using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputHandler))]
public class PlayerController : EntityBase<PlayerController>
{
    public CharacterData characterData;
    public Animator animator;
    public CharacterController characterController;

    [SerializeField] private HealthBarUI healthBarUI;

    [Header("Magnet Controller")]
    [SerializeField] private GameObject magnet;
    [SerializeField] private GameObject handRig;
    [SerializeField] private MagnetController magnetController;

    [Space(10)][SerializeField] PlayerMovement playerMovement;
    [Space(10)][SerializeField] public PlayerSound characterSound;

    List<CharacterComponents> _components = new List<CharacterComponents>();
    public event Action<bool> OnPlayerDeath;

    void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        foreach (CharacterComponents c in _components)
        {
            c.OnEnable();
        }

        healthController.OnHealthChanged += OnHealthChanged;
    }

    void OnDisable()
    {
        foreach (CharacterComponents c in _components)
        {
            c.OnDisable();
        }

        healthController.OnHealthChanged -= OnHealthChanged;
    }

    void Update()
    {
        foreach (CharacterComponents c in _components)
        {
            c.Update();
        }
    }

    public void Initialize()
    {
        InitComponents();
        InitCharacterComponents();
        base.BaseInit(controller);
    }

    private void InitCharacterComponents()
    {
        _components.Clear();
        _components.Add(playerMovement);
        _components.Add(characterSound);

        foreach (CharacterComponents c in _components)
        {
            c.Initialize(this);
        }
    }

    private void InitComponents()
    {
        characterController = GetComponent<CharacterController>();
        ValidationUtils.CheckNull(characterController, "[PlayerController.cs] ---> Cannot find CharacterController");

        magnetController = GetComponentInChildren<MagnetController>();
        ValidationUtils.CheckNull(magnetController, "[PlayerController.cs] ---> Cannot find MagnetController in children");
    }

    [SerializeField] private float cooldownToTakeDame = 1f;
    Coroutine _stunRoutine;
    private bool canTakeDamage = true;
    private bool isStunned = false;
    private bool isDead = false;

    public override void TakeDamage(float damage)
    {
        if (!canTakeDamage || isStunned || isDead)
            return;

        base.TakeDamage(damage);

        if (healthController.CurrentHealth <= 0f)
        {
            isDead = true;
            OnDeath();
            return;
        }

        characterSound.PlayHit();
        canTakeDamage = false;
        Invoke(nameof(ResetDamageCooldown), cooldownToTakeDame);

        if (_stunRoutine != null)
            StopCoroutine(_stunRoutine);

        _stunRoutine = StartCoroutine(HitStun());
    }

    private void ResetDamageCooldown()
    {
        canTakeDamage = true;
    }

    IEnumerator HitStun()
    {
        isStunned = true;
        playerMovement.SetCanMove(false);
        animator.SetBool("isHit", true);

        yield return new WaitForSeconds(0.1f);

        animator.SetBool("isHit", false);
        playerMovement.SetCanMove(true);
        isStunned = false;
    }

    public override float GetMaxHealth()
    {
        return characterData.maxHealth;
    }

    public override void OnHealthChanged(float current, float max)
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(current, max);
        }
    }

    [ContextMenu("Player Death")]
    public override void OnDeath()
    {
        InputHandler _inputHandler = GetComponent<InputHandler>();

        animator.SetBool("isDead", true);
        characterSound.PlayDeath();
        handRig.SetActive(false);

        Rigidbody wrb = magnet.GetComponent<Rigidbody>();
        wrb.useGravity = true;
        wrb.isKinematic = false;

        magnetController.pullButton.enabled = false;
        magnetController.shootButton.enabled = false;
        magnetController.enabled = false;

        _inputHandler.ResetInput();
        _inputHandler.enabled = false;

        enabled = false;
        OnPlayerDeath?.Invoke(false);
    }
}
