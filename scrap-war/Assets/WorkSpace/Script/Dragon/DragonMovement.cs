using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class DragonMovement : DragonComponents
{
    [SerializeField] Transform _target;

    [SerializeField] float footstepInterval = 0.8f; // khoảng cách giữa tiếng bước chân
    [SerializeField] float footstepTimer = 0f; // thời gian nghỉ để thở ra tiếng

    [SerializeField] float breathInterval = 3f;
    [SerializeField] float breathTimer = 0f;

    private float _defaultSpeed;
    private float _speedMultiplier;
    private float _distanceToTarget;
    private readonly float updateSpeed = 0.1f;

    private bool isWalking = false;
    private bool isRunning = false;

    public override void Initialize(DragonController dc)
    {
        base.Initialize(dc);

        _target = GameObject.FindWithTag("Player").transform;
        ValidationUtils.CheckNull(_target, "[DragonController.cs] ---> cant not find Player");

        _defaultSpeed = dragonController._agent.speed;
        _speedMultiplier = _defaultSpeed + 2f;
    }

    private IEnumerator FollowTarget()
    {
        yield return new WaitForSeconds(2f);

        WaitForSeconds wait = new WaitForSeconds(updateSpeed);

        while (dragonController.enabled)
        {
            if (_target != null && !dragonController.IsStunned && !dragonController.IsAttacking)
            {
                _distanceToTarget = Vector3.Distance(dragonController.transform.position, _target.position);

                if (!dragonController._agent.isStopped)
                {
                    if (_distanceToTarget <= dragonController._agent.stoppingDistance)
                    {
                        dragonController._agent.ResetPath();
                        dragonController._animator.SetBool("isWalking", false);
                        dragonController._animator.SetBool("isRunning", false);

                        isWalking = false;
                        isRunning = false;

                        breathTimer += updateSpeed;
                        if (breathTimer >= breathInterval)
                        {
                            dragonController.dragonSound?.PlayBreath();
                            breathTimer = 0f;
                        }
                    }
                    else if (_distanceToTarget >= 60)
                    {
                        if (dragonController._agent.isOnNavMesh)
                        {
                            dragonController._agent.speed = _speedMultiplier;
                            dragonController._agent.SetDestination(_target.position);
                            dragonController._animator.SetBool("isWalking", false);
                            dragonController._animator.SetBool("isRunning", true);

                            isWalking = false;
                            isRunning = true;
                            breathTimer = 0f;
                        }
                    }
                    else
                    {
                        if (dragonController._agent.isOnNavMesh)
                        {
                            dragonController._agent.speed = _defaultSpeed;
                            dragonController._agent.SetDestination(_target.position);
                            dragonController._animator.SetBool("isWalking", true);
                            dragonController._animator.SetBool("isRunning", false);

                            isWalking = true;
                            isRunning = false;
                            breathTimer = 0f;
                        }
                    }

                    if (isWalking || isRunning)
                    {
                        footstepTimer += updateSpeed;
                        if (footstepTimer >= footstepInterval)
                        {
                            dragonController.dragonSound?.PlayFootstep();
                            footstepTimer = 0f;
                        }
                    }
                    else
                    {
                        footstepTimer = 0f;
                    }
                }
            }

            yield return wait;
        }
    }

    public override void Start() => dragonController.StartCoroutine(FollowTarget());
    public override void Update() { }
    public override void DrawGizmos() { }
}
