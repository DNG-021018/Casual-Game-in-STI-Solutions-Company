using UnityEngine;
using UnityEngine.AI;

namespace Bowmancer
{
    public class B_EnemyRef : MonoBehaviour
    {
        [Space(10)]
        [Header("References")]
        [SerializeField] public Animator Animator;
        [SerializeField] public CapsuleCollider CapsuleCollider;
        [SerializeField] public NavMeshAgent NavMeshAgent;

        [Space(10)]
        [Header("Components")]
        [SerializeField] public B_EnemyController EnemyController;
        [SerializeField] public B_EnemyAnimationController EnemyAnimationController;
        [SerializeField] public B_EnemyDetected EnemyDetected;
        [SerializeField] public B_AttackRadius AttackRadius;
    }
}
