using UnityEngine;

namespace Bowmancer
{
    public interface B_IDamage
    {
        void TakeDamage(float damage);
        Transform GetTransform();
    }
}
