using UnityEngine;

public class RCC_BasicMoveState : BaseState
{
    public RCC_BasicMoveState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnterState()
    {
        stateMachine.canMove = true;
    }

    public override void OnUpdateState()
    {
        if (stateMachine.playerInput.HeadbuttInput)
        {
            stateMachine.playerInput.ClearHeadbuttInput();

            if (stateMachine.canHeadbutt && stateMachine.motor.IsOnValidGround)
            {
                stateMachine.ChangeState(stateMachine.headbuttState);
                return;
            }
        }
    }
    
    public override void OnFixedUpdateState()
    {
    }

    public override void OnExitState()
    {
        stateMachine.canMove = false;
    }
}
