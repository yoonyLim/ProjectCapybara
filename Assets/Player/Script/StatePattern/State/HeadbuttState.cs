using UnityEngine.InputSystem;
using UnityEngine;

public class HeadbuttState : IPlayerState
{
    private float stateDuration = 0.6f; // ��ġ�� �ִϸ��̼� ���� �Ǵ� ���� ���� �ð�
    private float timer;

    private string obstacleTag = "BreakableRock";  // �� �� �±׸� Ÿ�� ����
    private float hitRange = 3f;          // ���� ����
    private float headHeight = 0.8f;          // ���� ���� ����
    private float hitDelay = 0.08f;         // �ִ� Ÿ�ֿ̹� ���� 1ȸ�� üũ
    private bool hitPerformed = false;
    private float dashSpeed = 7f; // dash �ӵ�

    public void Enter(PlayerController player)
    {
        Debug.Log("HeadbuttState ����");
        timer = 0f;

        player.animator.SetTrigger("Headbutt");


    }

    public void Update(PlayerController player)
    {


    }

    public void FixedUpdate(PlayerController player)
    {
        float verticalVelocity = player.rb.linearVelocity.y;
        verticalVelocity -= player.gravity * Time.fixedDeltaTime;
        Vector3 fwd = player.playerForward;
        Vector3 v = player.rb.linearVelocity;
        v.x = fwd.x * dashSpeed;
        v.y = verticalVelocity;
        v.z = fwd.z * dashSpeed;
        player.rb.linearVelocity = v;

        if (!hitPerformed)
        {
            hitPerformed = true;

            Vector3 origin = player.transform.position + Vector3.up * headHeight;
            Vector3 dir = player.transform.forward;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, hitRange, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag(obstacleTag))
                {
                    if (hit.collider.TryGetComponent<DestructibleRock>(out var rock))
                    {
                        rock.Hit();                      // �� �� ������ �Ͷ߸���
                    }
                }
            }

            // ����׿� ����
            Debug.DrawRay(origin, dir * hitRange, Color.red, 0.3f);
        }

        timer += Time.fixedDeltaTime;
        // ������ �ð��� ������ �⺻ ����(RunningState)��
        if (timer >= stateDuration)
        {
            player.ChangeState(new RunningState());
        }
    }

    public void HandleInput(PlayerController player, InputAction.CallbackContext context)
    {
    }

    public void Exit(PlayerController player)
    {

    }
}