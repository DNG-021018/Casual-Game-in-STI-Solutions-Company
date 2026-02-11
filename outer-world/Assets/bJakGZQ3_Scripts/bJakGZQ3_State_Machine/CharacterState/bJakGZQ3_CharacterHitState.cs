using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_CharacterHitState : bJakGZQ3_ACharacterState
    {
        public override void Enter(bJakGZQ3_IStateMachine stateMachine)
        {
            base.Enter(stateMachine);

            anim.ResetTrigger("isHit");
            anim.SetTrigger("isHit");
            gridMovement.DisableMovement();

            characterStateMachine.StartCoroutine(WaitForEndAnimation("Hit", () => SwitchState(EntityState.Idle)));
        }

        public override void Exit(bJakGZQ3_IStateMachine stateMachine)
        {
            gridMovement.EnableMovement();
        }

        public override void OnTriggerEnter(bJakGZQ3_IStateMachine stateMachine, Collider collider)
        {
            base.OnTriggerEnter(stateMachine, collider);
        }
    }
}
