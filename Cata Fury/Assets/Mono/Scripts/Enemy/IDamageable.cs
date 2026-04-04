using UnityEngine;

namespace CataFury
{
    public interface IDamageable
    {
        bool ApplyDamage(float damage, Vector3 hitPoint);
    }
}
