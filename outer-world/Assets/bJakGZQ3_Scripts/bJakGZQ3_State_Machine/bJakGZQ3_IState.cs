namespace bJakGZQ3_Outer_World
{
    public interface bJakGZQ3_IState
    {
        public void Enter(bJakGZQ3_IStateMachine stateMachine);
        public void OnUpdateState(bJakGZQ3_IStateMachine stateMachine);
        public void Exit(bJakGZQ3_IStateMachine stateMachine);
    }
}
