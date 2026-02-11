public interface IDamage<T>
{
    void BaseInit(T controller);
    void TakeDamage(float damage);
    void Heal(float amount);
    float GetMaxHealth();
    void OnHealthChanged(float current, float max);
    void OnDeath();
}
