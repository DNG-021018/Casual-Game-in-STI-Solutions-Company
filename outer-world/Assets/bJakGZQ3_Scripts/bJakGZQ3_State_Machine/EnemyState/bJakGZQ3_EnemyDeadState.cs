using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_EnemyDeadState : bJakGZQ3_AEnemyState
    {
        public override void Enter(bJakGZQ3_IStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            gridMovement.DisableMovement();
            anim.ResetTrigger("isHit");
            anim.SetTrigger("isHit");
            characterStateMachine.StartCoroutine(WaitForEndAnimation("GetHit", () =>
            {
                GameObject.Destroy(Enemy.gameObject);
            }));
        }
    }
}
