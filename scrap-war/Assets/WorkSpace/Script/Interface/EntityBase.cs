using UnityEngine;

public abstract class EntityBase<T> : MonoBehaviour, IDamage<T> where T : MonoBehaviour
{
    protected T controller;
    protected HealthController<T> healthController;

    public virtual void BaseInit(T controller)
    {
        this.controller = controller;
        healthController = GetComponent<HealthController<T>>();
    }

    public virtual void TakeDamage(float damage)
    {
        healthController?.TakeDamage(damage);
    }

    public virtual void Heal(float amount)
    {
        healthController?.Heal(amount);
    }

    public abstract float GetMaxHealth();
    public abstract void OnHealthChanged(float current, float max);
    public abstract void OnDeath();
}
