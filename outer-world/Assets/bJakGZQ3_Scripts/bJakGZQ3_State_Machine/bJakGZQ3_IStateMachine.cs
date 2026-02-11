namespace bJakGZQ3_Outer_World
{
    public interface bJakGZQ3_IStateMachine
    {
        public bJakGZQ3_IState CurrentState { get; }
        public void ChangeState(bJakGZQ3_IState newState);
        public void ResetState();
        public void UpdateState(float deltaTime);
    }
}
