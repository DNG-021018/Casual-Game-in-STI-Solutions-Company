using UnityEngine;

namespace Bowmancer
{
    public class B_EnemyDetected : MonoBehaviour
    {
        [SerializeField] private float detectRange = 10f;
        [SerializeField] private LayerMask playerLayer;

        public Transform Target { get; private set; }

        void FixedUpdate()
        {
            if (Target != null) return;
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                detectRange,
                playerLayer
            );

            if (hits.Length > 0)
            {
                Target = hits[0].transform;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectRange);
        }
#endif
    }
}
