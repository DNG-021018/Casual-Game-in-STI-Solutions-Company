using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_EnemyMoveState : bJakGZQ3_AEnemyState
    {
        public override void Enter(bJakGZQ3_IStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            anim.SetBool("isMoving", true);
        }

        public override void OnUpdateState(bJakGZQ3_IStateMachine stateMachine)
        {
            base.OnUpdateState(stateMachine);
            if (!gridMovement.IsMoving)
            {
                SwitchState(EntityState.Idle);
            }
        }

        public override void Exit(bJakGZQ3_IStateMachine stateMachine)
        {
            anim.SetBool("isMoving", false);
        }

        public override void OnTriggerEnter(bJakGZQ3_IStateMachine stateMachine, Collider collider)
        {
            base.OnTriggerEnter(stateMachine, collider);
        }
    }
}
