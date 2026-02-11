using UnityEngine;

public interface IAttractable
{
    public void AttrachItems(Transform targetPosition, float force) { }
    public void Shoot(Vector3 direction, float force) { }
}
