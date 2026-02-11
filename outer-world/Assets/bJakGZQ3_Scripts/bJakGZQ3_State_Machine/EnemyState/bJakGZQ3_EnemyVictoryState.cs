using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_EnemyVictoryState : bJakGZQ3_AEnemyState
    {
        public override void Enter(bJakGZQ3_IStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            gridMovement.DisableMovement();
            anim.SetTrigger("isWIn");
        }
    }
}
