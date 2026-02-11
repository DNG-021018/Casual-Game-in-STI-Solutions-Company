using UnityEngine;
using UnityEngine.AI;

public interface IChicken
{
    NavMeshAgent Agent { get; }
    Transform Target { get; }
    Animator Animator { get; }

    float JumpCountDownTimer { get; }
    float JumpSpeed { get; }
    float JumpHeight { get; }
    float JumpDuration { get; }

    float HonkCountDownTimer { get; }
    float SlowDuration { get; }
    float SlowAmount { get; }
}
