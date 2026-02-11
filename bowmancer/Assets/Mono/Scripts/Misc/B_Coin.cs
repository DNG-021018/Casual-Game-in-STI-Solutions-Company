using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace Bowmancer
{
    public class B_Coin : MonoBehaviour, IPoolableWithInit<B_Coin>
    {
        Pooler<B_Coin> _pool;

        public void InitPool(Pooler<B_Coin> pool)
        {
            this._pool = pool;
        }

        public void OnGetFromPool()
        {
            gameObject.SetActive(true);
        }

        public void OnReturnToPool()
        {
            gameObject.SetActive(false);
            _pool.ReturnToPool(this);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                OnReturnToPool();
            }
        }
    }
}
