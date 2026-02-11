using UnityEngine;
using System;

public class PoolableObstacle : MonoBehaviour
{
    private Action onReturn;

    public void SetPool(Action onReturn) => this.onReturn = onReturn;

    public void ReturnToPool() => onReturn?.Invoke();
}
