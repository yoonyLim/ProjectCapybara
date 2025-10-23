using UnityEngine;

/// <summary>
/// InteractionComponent의 static 이벤트를 구독하여
/// 대화 시작/종료 시 콘솔에 로그를 출력하는 테스트용 스크립트입니다.
/// </summary>
public class EventReader : MonoBehaviour
{
    /// <summary>
    /// 스크립트가 활성화될 때 이벤트 구독
    /// </summary>
    private void OnEnable()
    {
        Debug.Log("[EventReader] 이벤트 구독을 시작합니다.");
        InteractionComponent.OnDialogStart += HandleDialogStart;
        InteractionComponent.OnDialogEnd += HandleDialogEnd;
    }

    /// <summary>
    /// 스크립트가 비활성화될 때 이벤트 구독 해제
    /// (메모리 누수 방지)
    /// </summary>
    private void OnDisable()
    {
        Debug.Log("[EventReader] 이벤트 구독을 해제합니다.");
        InteractionComponent.OnDialogStart -= HandleDialogStart;
        InteractionComponent.OnDialogEnd -= HandleDialogEnd;
    }

    /// <summary>
    /// 대화 시작 이벤트가 발생하면 호출됩니다.
    /// </summary>
    private void HandleDialogStart()
    {
        Debug.LogWarning("======= [EventReader] 대화 시작 이벤트 (OnDialogStart) 발생! =======");
    }

    /// <summary>
    /// 대화 종료 이벤트가 발생하면 호출됩니다.
    /// </summary>
    private void HandleDialogEnd()
    {
        Debug.LogWarning("======= [EventReader] 대화 종료 이벤트 (OnDialogEnd) 발생! =======");
    }
}