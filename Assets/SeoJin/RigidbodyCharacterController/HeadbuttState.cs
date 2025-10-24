using UnityEngine;

public class RCC_HeadbuttState : BaseState
{
    public RCC_HeadbuttState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnterState()
    {
        stateMachine.rb.linearVelocity = Vector3.zero;
        
        stateMachine.lastheadbuttUsagetime = Time.time;
        stateMachine.playerAnimator.TriggerHeadbuttInputParam();

        stateMachine.rockBreaker.SetActive(true);
        
        stateMachine.Lunge(5f);
    }

    public override void OnUpdateState()
    {
        AnimatorStateInfo stateInfo = stateMachine.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Headbutt") && stateInfo.normalizedTime >= 1.0f)
        {
            stateMachine.ChangeState(stateMachine.basicMoveState);
        }
    }
    
    public override void OnFixedUpdateState()
    {
    }

    public override void OnExitState()
    {
        stateMachine.animator.ResetTrigger("Headbutt");
        stateMachine.rockBreaker.SetActive(false);
    }
}
