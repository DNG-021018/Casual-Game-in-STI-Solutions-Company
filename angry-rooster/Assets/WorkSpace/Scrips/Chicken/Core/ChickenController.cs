using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(ChickenMovement), typeof(ChickenSensor))]
public class ChickenController : MonoBehaviour, IChicken
{
    [Header("Chicken Data")]
    [Tooltip("Require Scriptable Object \"ChichkenData\"")]
    [SerializeField]
    ChickenData chickenData;

    // Components
    [Header("Target")]
    [Tooltip("Require Player Target Transform")]
    [SerializeField]
    Transform target; // Player Transform
    NavMeshAgent agent;
    Animator animator;

    // Implement Interface Components
    public Transform Target => target;
    public NavMeshAgent Agent => agent;
    public Animator Animator => animator;

    // Implement Interface Properties
    public float JumpCountDownTimer => chickenData.jumpCoolDownTimer;
    public float JumpSpeed => chickenData.jumpSpeed;
    public float JumpHeight => chickenData.jumpHeight;
    public float JumpDuration => chickenData.jumpDuration;

    public float HonkCountDownTimer => chickenData.honkCoolDownTimer;
    public float SlowDuration => chickenData.slowDuration;
    public float SlowAmount => chickenData.slowAmount;

    private void Awake()
    {
        Initialize();

        target = FindFirstObjectByType<Character>().transform;
        ValidationUtils.CheckNull(target, "[ChickenControler.cs] ---> cant not find Player Target");
    }

    private void Initialize()
    {
        agent = GetComponent<NavMeshAgent>();
        ValidationUtils.CheckNull(agent, "[ChickenControler.cs] ---> cant not find NavMeshAgent");
        BindingNavMeshData(agent);

        animator = GetComponent<Animator>();
        ValidationUtils.CheckNull(animator, "[ChickenControler.cs] ---> cant not find Animator");
    }

    public void BindingNavMeshData(NavMeshAgent a)
    {
        a.speed = chickenData.Speed;
        a.angularSpeed = chickenData.AngularSpeed;
        a.acceleration = chickenData.Acceleration;
        a.stoppingDistance = chickenData.StoppingDistance;
        a.radius = chickenData.Radius;
        a.height = chickenData.Height;
    }
}
