using UnityEngine;

namespace Bowmancer
{
    public class B_PlayerRef : MonoBehaviour
    {
        [Space(10)]
        [Header("References")]
        [SerializeField] public Transform CameraTarget;
        [SerializeField] public Animator Animator;
        [SerializeField] public CharacterController CharacterController;

        [Space(10)]
        [Header("Components")]
        [SerializeField] public B_PlayerAnimationController PlayerAnimationController;
        [SerializeField] public B_PlayerController PlayerController;
        [SerializeField] public B_PlayerDetected PlayerDetected;
        [SerializeField] public B_GunController GunController;
    }
}
