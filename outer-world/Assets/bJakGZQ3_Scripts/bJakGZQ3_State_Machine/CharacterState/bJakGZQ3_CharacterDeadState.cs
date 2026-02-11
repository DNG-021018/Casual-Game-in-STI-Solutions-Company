namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_CharacterDeadState : bJakGZQ3_ACharacterState
    {
        public override void Enter(bJakGZQ3_IStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            gridMovement.DisableMovement();
            anim.SetTrigger("isDead");
        }
    }
}
