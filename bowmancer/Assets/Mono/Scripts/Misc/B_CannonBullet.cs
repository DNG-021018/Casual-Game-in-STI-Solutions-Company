using System.Collections.Generic;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace Bowmancer
{
    public class B_CannonBullet : MonoBehaviour, IPoolableWithInit<B_CannonBullet>
    {
        private List<Vector3> path;
        private float speed;
        private float lifetime;
        private int currentPathIndex = 0;
        private float spawnTime;

        Pooler<B_CannonBullet> pool;
        Pooler<B_CannonBulletVFX> poolVFX;

        public void InitPool(Pooler<B_CannonBullet> pool)
        {
            this.pool = pool;
        }

        public void InitVFXPool(Pooler<B_CannonBulletVFX> poolVFX)
        {
            this.poolVFX = poolVFX;
        }

        public void OnGetFromPool()
        {
            gameObject.SetActive(true);
        }

        public void OnReturnToPool()
        {
            gameObject.SetActive(false);
            pool.ReturnToPool(this);
        }

        public void SetPath(List<Vector3> bulletPath, float bulletSpeed, float bulletLifetime)
        {
            path = bulletPath;
            speed = bulletSpeed;
            lifetime = bulletLifetime;
            currentPathIndex = 0;
            spawnTime = Time.time;

            if (path.Count >= 2)
            {
                transform.position = path[0];
            }
        }

        void Update()
        {
            if (Time.time - spawnTime >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (path == null || path.Count < 2 || currentPathIndex >= path.Count - 1)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 currentTarget = path[currentPathIndex + 1];
            Vector3 direction = (currentTarget - transform.position).normalized;

            float distanceToMove = speed * Time.deltaTime;
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget);

            if (distanceToMove >= distanceToTarget)
            {
                transform.position = currentTarget;
                currentPathIndex++;

                if (currentPathIndex >= path.Count - 1)
                {
                    Destroy(gameObject);
                    return;
                }

                Vector3 newDirection = (path[currentPathIndex + 1] - transform.position).normalized;
                if (newDirection != Vector3.zero)
                {
                    transform.forward = newDirection;
                }
            }
            else
            {
                transform.position += direction * distanceToMove;

                if (direction != Vector3.zero)
                {
                    transform.forward = direction;
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                B_PlayerController player = other.GetComponent<B_PlayerController>();
                B_CannonBulletVFX vfx = poolVFX.GetRandom(transform.position, Quaternion.identity);
                vfx.InitPool(poolVFX);
                if (player != null)
                {
                    player.TakeDamage(20f);
                }
                OnReturnToPool();
            }
        }
    }
}
