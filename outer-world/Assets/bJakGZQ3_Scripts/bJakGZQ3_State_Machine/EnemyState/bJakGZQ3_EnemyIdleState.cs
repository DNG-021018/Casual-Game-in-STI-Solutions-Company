using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_EnemyIdleState : bJakGZQ3_AEnemyState
    {
        public override void Enter(bJakGZQ3_IStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            float blend = RandomIdleAnimation();
            if (anim != null)
            {
                anim.SetFloat("IdleBlend", Mathf.RoundToInt(blend));
            }
        }

        public override void Exit(bJakGZQ3_IStateMachine stateMachine)
        {
            base.Exit(stateMachine);

        }

        public override void OnUpdateState(bJakGZQ3_IStateMachine stateMachine)
        {
            base.OnUpdateState(stateMachine);
            if (gridMovement != null && gridMovement.IsMoving)
            {
                SwitchState(EntityState.Move);
                return;
            }
        }

        public override void OnTriggerEnter(bJakGZQ3_IStateMachine stateMachine, Collider collider)
        {
            base.OnTriggerEnter(stateMachine, collider);

        }

        float RandomIdleAnimation()
        {
            return Random.Range(0f, 2.999f);
        }
    }
}
