using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(AgentLinkMover))]
public class ChickenMovement : MonoBehaviour
{
    IChicken chickenAgent;
    private float updateSpeed = 0.1f;

    void Awake()
    {
        chickenAgent = GetComponentInParent<IChicken>();
        ValidationUtils.CheckNull(chickenAgent, "[ChickenMovement.cs] ---> chickenAgent is null");
    }

    void Start()
    {
        StartCoroutine(FollowTarget());
    }

    private IEnumerator FollowTarget()
    {
        yield return new WaitUntil(() => chickenAgent.Target != null);
        yield return new WaitForSeconds(2f);

        WaitForSeconds wait = new WaitForSeconds(updateSpeed);

        while (enabled)
        {
            if (chickenAgent.Target != null)
            {
                if (chickenAgent.Agent.speed > 0f)
                {
                    chickenAgent.Agent.SetDestination(chickenAgent.Target.position);
                    yield return wait;
                }
                else
                {
                    while (chickenAgent.Agent.speed == 0f && chickenAgent.Target != null)
                    {
                        Vector3 dir = chickenAgent.Target.position - transform.position;
                        dir.y = 0f;

                        if (dir.sqrMagnitude > 0.01f)
                        {
                            Quaternion targetRot = Quaternion.LookRotation(dir);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
                        }

                        yield return null;
                    }
                }
            }
            else
            {
                yield return wait;
            }
        }
    }

}
