using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

// 이벤트 타입을 정의하는 열거형
public enum PlayerEventType
{
    None,
    Jumped,
    Landed,
    EnteredWater,
    Collision,
    SlippedOnIce
}

[System.Serializable]
public class PlayerEvent
{
    public PlayerEventType eventType;
    public UnityEvent onEventTriggered;
}

public class PlayerHapticEvent : MonoBehaviour
{
    [Header("Player Events")]
    [Tooltip("플레이어의 각종 행동에 따라 발생할 이벤트 목록")]
    [SerializeField] private List<PlayerEvent> playerEvents;

    /// <summary>
    /// 주어진 이벤트 타입에 해당하는 UnityEvent를 찾아 호출합니다.
    /// </summary>
    public void TriggerPlayerEvent(PlayerEventType typeToTrigger)
    {
        PlayerEvent playerEvent = playerEvents.FirstOrDefault(e => e.eventType == typeToTrigger);

        if (playerEvent != null)
        {
            // 이벤트에 연결된 리스너가 있다면 실행
            playerEvent.onEventTriggered?.Invoke();
        }
        else
        {
            Debug.LogWarning($"{typeToTrigger} 타입의 이벤트가 PlayerEvents 목록에 정의되지 않았습니다.");
        }
    }
}