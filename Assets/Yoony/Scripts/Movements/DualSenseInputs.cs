using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

[RequireComponent(typeof(CharacterController))]
public class DualSenseInputs : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 10.0f;
    public float jumpSpeed = 0.5f;
    public float gravity = 20.0f;
    
    public Camera cam;

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    
    // Dualsense
    private DualSenseGamepadHID dualSenseGamepad;
    // private InputUser inputUser;

    private Coroutine currentRotationCoroutine;
    private Coroutine dualSenseRumbleCoroutine;

    void Start()
    {
        if (dualSenseGamepad != null)
        {
            dualSenseGamepad.SetLightBarColor(Color.green);
        }
        
        controller = GetComponent<CharacterController>();

        // Main Camera가 할당되지 않았다면 자동으로 찾습니다.
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    private void StartRumble(float lowFreq, float highFreq, float duration)
    {
        if (dualSenseRumbleCoroutine != null)
            StopCoroutine(dualSenseRumbleCoroutine);

        dualSenseRumbleCoroutine = StartCoroutine(RumblePattern(lowFreq, highFreq, duration));
    }

    IEnumerator RumblePattern(float lowFreq, float highFreq, float duration)
    {
        dualSenseGamepad.SetMotorSpeeds(lowFreq, highFreq);
        yield return new WaitForSeconds(duration);
        dualSenseGamepad.SetMotorSpeeds(0f, 0f);
    }

    void Update()
    {
        if (dualSenseGamepad.leftTriggerButton.isPressed)
        {
            dualSenseGamepad.SetLightBarColor(Color.red);
            // StartRumble(0.1f, 1f, 2.5f);
        }

        /*if (shouldIncreaseRumble)
        {
            rumbleHighFreq += Time.deltaTime;
            rumbleDuration += Time.deltaTime;
            dualSenseGamepad.SetMotorSpeeds(rumbleHighFreq / 10f, rumbleHighFreq / 2f);
            
            Debug.Log(rumbleHighFreq);

            if (rumbleDuration > 2.0f)
            {
                dualSenseGamepad.SetMotorSpeeds(0f, 0f);
                rumbleHighFreq = 0f;
                rumbleDuration = 0f;
                shouldIncreaseRumble = false;
            }
        }*/
        
        if (dualSenseGamepad.rightTriggerButton.isPressed)
        {
            dualSenseGamepad.SetLightBarColor(Color.blue);
            dualSenseGamepad.PauseHaptics();
        }
        
        
        
        // 1. 입력 받기
        float horizontal = Input.GetAxis("Horizontal"); // A, D 또는 좌우 화살표
        float vertical = Input.GetAxis("Vertical");   // W, S 또는 위아래 화살표
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical);

        // 캐릭터가 땅에 있을 때만 이동과 점프를 처리
        if (controller.isGrounded)
        {
            // 2. 카메라 기준 이동 방향 계산
            if (inputDirection.magnitude >= 0.1f)
            {
                // 카메라의 정면 방향과 오른쪽 방향을 가져옵니다.
                Vector3 camForward = cam.transform.forward;
                Vector3 camRight = cam.transform.right;

                // y축 값은 무시하여 수평 방향만 사용합니다.
                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();

                // 카메라 방향과 플레이어 입력을 조합하여 최종 이동 방향을 결정합니다.
                moveDirection = (camForward * vertical + camRight * horizontal).normalized;
                
                // 3. 플레이어 회전
                // 이동 방향으로 캐릭터를 부드럽게 회전시킵니다.
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                // 입력이 없으면 이동 방향을 0으로 설정합니다.
                moveDirection = Vector3.zero;
            }
            
            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }

            // 이동 방향에 속도를 곱합니다.
            moveDirection *= moveSpeed;
        }

        // 4. 중력 적용 및 이동 실행
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }


    public void RotateTowards(Vector3 target)
    {
        if (currentRotationCoroutine != null)
            StopCoroutine(currentRotationCoroutine);
    
        currentRotationCoroutine = StartCoroutine(RotateCoroutine(target));
    }

    private IEnumerator RotateCoroutine(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;
    
        if (direction == Vector3.zero) yield break;
    
        Quaternion targetRotation = Quaternion.LookRotation(direction);
    
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }
    
        transform.rotation = targetRotation;
    }
}