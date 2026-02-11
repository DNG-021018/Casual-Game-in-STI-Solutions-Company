using System;
using UnityEngine;

public class HealthController<T> : MonoBehaviour
{
    public IDamage<T> T_controller;

    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }
    bool IsDead => CurrentHealth <= 0f;
    public event Action<float, float> OnHealthChanged;

    private void Awake()
    {
        if (TryGetComponent<IDamage<T>>(out IDamage<T> foundOwner))
        {
            Initialize(foundOwner);
        }
    }

    public void Initialize(IDamage<T> obj)
    {
        T_controller = obj;
        MaxHealth = obj.GetMaxHealth();
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (IsDead)
        {
            T_controller?.OnDeath();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        CurrentHealth += amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}
