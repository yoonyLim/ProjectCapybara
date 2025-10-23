using UnityEngine.InputSystem;
using UnityEngine;

public class HeadbuttState : IPlayerState
{
    private float stateDuration = 0.6f; // 박치기 애니메이션 길이 또는 상태 유지 시간
    private float timer;

    private string obstacleTag = "Obstacle";  // ← 이 태그만 타격 판정
    private float hitRange = 2f;          // 레이 길이
    private float headHeight = 0.8f;          // 레이 시작 높이
    private float hitDelay = 0.08f;         // 애니 타이밍에 맞춰 1회만 체크
    private bool hitPerformed = false;
    private float dashSpeed = 7f; // dash 속도

    public void Enter(PlayerController player)
    {
        Debug.Log("HeadbuttState 진입");
        timer = 0f;

        player.animator.SetTrigger("Headbutt");
        //player.rb.linearVelocity = Vector3.zero;

        Vector3 fwd = player.transform.forward;
        Vector3 v = player.rb.linearVelocity;
        v.x = fwd.x * dashSpeed;
        v.z = fwd.z * dashSpeed;
        player.rb.linearVelocity = v;
    }

    public void Update(PlayerController player)
    {
        if (!hitPerformed)
        {
            hitPerformed = true;

            Vector3 origin = player.transform.position + Vector3.up * headHeight;
            Vector3 dir = player.transform.forward;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, hitRange, ~0, QueryTriggerInteraction.Ignore))
            {
                // Tag = Obstacle 인 것만 타격
                if (hit.collider.CompareTag(obstacleTag))
                {
                    if (hit.collider.TryGetComponent<DestructibleRock>(out var rock))
                    {
                        rock.brokenPosition = hit.point; // ★ 먼저 세팅
                        rock.Hit();                      // ★ 그 다음에 터뜨리기
                    }
                }
            }

            // 디버그용 레이
            Debug.DrawRay(origin, dir * hitRange, Color.red, 0.3f);
        }

        timer += Time.deltaTime;
        // 정해진 시간이 지나면 기본 상태(RunningState)로
        if (timer >= stateDuration)
        {
            player.ChangeState(new RunningState());
        }

    }

    public void FixedUpdate(PlayerController player)
    {
    }

    public void HandleInput(PlayerController player, InputAction.CallbackContext context)
    {
    }

    public void Exit(PlayerController player)
    {

    }
}