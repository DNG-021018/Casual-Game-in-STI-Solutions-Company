using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DragonController : EntityBase<DragonController>
{
    [Header("Dragon Data")]
    [SerializeField] public DragonData dragonData;

    public NavMeshAgent _agent;
    public Animator _animator;
    [SerializeField] private HealthBarUI healthBarUI;

    [SerializeField] DragonMovement dragonMovement;
    [SerializeField] DragonAction dragonAction;
    [SerializeField] public DragonSound dragonSound;


    public event Action<bool> OnDragonDeath;

    List<DragonComponents> _components = new List<DragonComponents>();

    void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        healthController.OnHealthChanged += OnHealthChanged;
    }

    void OnDisable()
    {
        healthController.OnHealthChanged -= OnHealthChanged;
    }

    void Start()
    {
        foreach (DragonComponents c in _components)
        {
            c.Start();
        }
    }

    private void Initialize()
    {
        InitializeDragon();

        _components.Clear();
        _components.Add(dragonMovement);
        _components.Add(dragonAction);
        _components.Add(dragonSound);

        foreach (DragonComponents c in _components)
        {
            c.Initialize(this);
        }

        BindingNavMeshData(_agent);
    }

    void InitializeDragon()
    {
        _animator = GetComponent<Animator>();
        ValidationUtils.CheckNull(_animator, "[DragonController.cs] ---> cant not find Animator");

        _agent = GetComponent<NavMeshAgent>();
        ValidationUtils.CheckNull(_agent, "[DragonController.cs] ---> cant not find NavMeshAgent");

        base.BaseInit(this);
    }

    void BindingNavMeshData(NavMeshAgent a)
    {
        a.speed = dragonData.Speed;
        a.angularSpeed = dragonData.AngularSpeed;
        a.acceleration = dragonData.Acceleration;
        a.stoppingDistance = dragonData.StoppingDistance;
        a.radius = dragonData.Radius;
        a.height = dragonData.Height;
    }

    // Animation Event
    [HideInInspector] public bool canFlame = false;

    public void canFlameThrower()
    {
        canFlame = true;
    }

    [HideInInspector] public bool canGo = false;
    public void CanGo()
    {
        canGo = true;
    }
    //

    public override float GetMaxHealth()
    {
        return dragonData.maxHealth;
    }

    public override void OnHealthChanged(float current, float max)
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(current, max);
        }
    }

    public bool IsStunned { get; private set; } = false;
    public bool IsInCombat { get; private set; } = false;
    [HideInInspector] public bool IsAttacking = false;

    public override void TakeDamage(float amount)
    {
        if (IsInCombat || IsAttacking) return;

        base.TakeDamage(amount);

        if (!IsStunned)
            StartCoroutine(PlayHitReaction());
    }

    private IEnumerator PlayHitReaction()
    {
        if (IsInCombat) yield break;

        IsStunned = true;
        _agent.isStopped = true;

        _animator.ResetTrigger("isHit");
        _animator.SetTrigger("isHit");

        float duration = _animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration);

        _agent.isStopped = false;
        IsStunned = false;
    }

    public void SetCanRotate(bool canRotate)
    {
        _agent.updateRotation = canRotate;
    }

    [ContextMenu("DragonDead")]
    public override void OnDeath()
    {
        _agent.isStopped = true;
        IsStunned = true;

        _animator.SetTrigger("isDead");

        if (dragonSound != null)
        {
            dragonSound.PlayDeath();
        }

        enabled = false;
        OnDragonDeath?.Invoke(true);
    }

    public void EnterCombat()
    {
        IsInCombat = true;
    }

    public void ExitCombat()
    {
        IsInCombat = false;
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (_components == null || _components.Count == 0)
        {
            Initialize();
        }
#endif

        foreach (DragonComponents c in _components)
        {
            c?.DrawGizmos();
        }
    }
}
