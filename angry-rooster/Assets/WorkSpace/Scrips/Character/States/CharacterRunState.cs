using UnityEngine;

public class CharacterRunState : IState
{
    private CharacterStateMachine _characterStateMachine;

    public CharacterRunState(CharacterStateMachine characterStateMachine)
    {
        _characterStateMachine = characterStateMachine;
    }


    public void Enter()
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void Update(float deltaTime)
    {
        throw new System.NotImplementedException();
    }
}
