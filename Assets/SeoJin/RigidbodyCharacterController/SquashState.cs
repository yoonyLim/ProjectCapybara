using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RCC_SquashState : BaseState
{
    public RCC_SquashState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    private float remainingSquashDuration;
    private const float squashDuration = 1f;

    private bool isRestoring;
    
    public override void OnEnterState()
    {
        stateMachine.rb.linearVelocity = Vector3.zero;
        remainingSquashDuration = squashDuration;
        isRestoring = false;
        stateMachine.StartCoroutine(SquashCoroutine());
    }

    public override void OnUpdateState()
    {
        Debug.Log($"remainingSquashDuration : {remainingSquashDuration}");
        
        remainingSquashDuration -= Time.deltaTime;
        if (remainingSquashDuration < 0f && !isRestoring)
        {
            isRestoring = true;
            Debug.Log("Restore!");
            stateMachine.StartCoroutine(RestoreScaleCoroutine());
        }
    }
    
    public override void OnFixedUpdateState()
    {
        stateMachine.rb.angularVelocity = Vector3.zero;
        stateMachine.rb.linearVelocity = Vector3.zero;
    }

    public override void OnExitState()
    {
        remainingSquashDuration = 0f;
    }
    
    public void AddSquashDuration()
    {
        remainingSquashDuration += squashDuration;
    }

    IEnumerator SquashCoroutine()
    {
        Vector3 originalLocalScale = stateMachine.scaleTarget.localScale;

        float duration = 0.2f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            stateMachine.scaleTarget.localScale = Vector3.Lerp(
                originalLocalScale,  
                80f * new Vector3(1.1f, 1.1f, 0.2f), 
                elapsedTime / duration);
            yield return null;
        }
        stateMachine.scaleTarget.localScale = 80f * new Vector3(1.1f, 1.1f, 0.2f);
    }

    IEnumerator RestoreScaleCoroutine()
    {
        Vector3 morphedLocalScale = stateMachine.scaleTarget.localScale;
        
        float duration = 0.2f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            stateMachine.scaleTarget.localScale = Vector3.Lerp(
                morphedLocalScale,
                80f * Vector3.one, 
                elapsedTime / duration);
            yield return null;
        }

        stateMachine.scaleTarget.localScale = 80f * Vector3.one;
        stateMachine.ChangeState(stateMachine.basicMoveState);
    }
}
