//using Gamekit3D;
using Capybara;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Windows;


public interface IPlayerState
{
    void Enter(PlayerController player);
    void Exit(PlayerController player);
    void HandleInput(PlayerController player, InputAction.CallbackContext context);
    void Update(PlayerController player);
    void FixedUpdate(PlayerController player);
}

public class PlayerController : MonoBehaviour
{
    // input reader
    [SerializeField] private CapybaraInputReader inputReader;
    [SerializeField] private PlayerHapticEvent playerHapticEvent;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private string[] footstepSoundNames;
    // State 관련
    private IPlayerState currentState;

    // main camera 위치
    public Transform cameraTransform;
    private bool isInWindZone = false;

    public Animator animator;
    public Rigidbody rb;

    [HideInInspector] public Vector2 LastMoveInput { get; private set; }
    [HideInInspector] public Vector2 MoveInput { get; private set; }
    [HideInInspector] public Vector3 platformVelocity;

    #region 이동 관련 변수
    [Header("이동속도 & 중력")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 7f;
    public float gravity = 9.81f;
    public float fallGravity = 20f;
    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public bool isRunning;
    #endregion

    #region Ground 체크 관련 변수
    [Header("Check isGrounded")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float sphereRadius = 0.2f;
    [SerializeField] private float raycastDistance = 0.5f;
    [SerializeField] private Transform[] groundCheckPoints;
    [HideInInspector] public bool isGrounded;
    #endregion

    #region 경사로 계산 관련 변수
    [Header("최대 경사 각도 검사")]
    [SerializeField] Transform raycastOrigin;
    [SerializeField] float maxSlopeAngle;

    [Header("경사로 회전")]
    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private float timer = 0.25f;
    [HideInInspector] public RaycastHit slopeHit;
    #endregion

    #region 점프 관련 변수
    [Header("Jump Force")]
    public float jumpForce = 7f;
    //[HideInInspector]
    public bool isJumping = false;
    private bool wasFalling = false;
    [HideInInspector] public float airSpeed = 5f;
    [HideInInspector] public float airAcceleration = 20f;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTimeDuration = 0.15f; // 코요테 시간 (0.1 ~ 0.2초 추천)
    public float coyoteTimeCounter;
    #endregion

    #region Ice 관련 변수
    [Header("Ice Layer 체크")]
    public LayerMask iceLayer;
    [HideInInspector] public bool isOnIce;
    #endregion

    #region Glide 관련 변수
    [Header("글라이드 유지 시간")]
    public float glideDuration = 2.5f;

    [Header("Glide 이동 변수")]
    public float glideSpeed = 7f;
    public float glideTurnSpeed = 0.5f;
    public float glideGravity = 4f;
    public float glideDrag = 5f;

    public float normalGravity = 9.81f;
    [HideInInspector] public float normalDrag = 0f;
    public bool glideLocked = false;
    #endregion

    #region DustLand 생성 변수
    [Header("DustLand 생성 속도")]
    [SerializeField] private float dustSpawnVel = -8.0f;
    public bool canSpawnDustLand = false;
    #endregion

    #region 리스폰 변수
    [SerializeField] private float fallDeathYLevel = -50f;
    #endregion

    #region 충돌 관련 변수
    [Header("Hit 설정")]
    [SerializeField] private string obstacleTag = "Obstacle"; // 장애물에 사용할 태그
    #endregion

    #region Y Mesh 
    [Header("Squash 설정")]
    [Tooltip("플레이어의 시각적 모델(메시)의 Transform")]
    [SerializeField] private Transform playerModelTransform;
    [Tooltip("납작해져 있는 시간 (초)")]
    [SerializeField] private float squashDuration = 1f;
    [Tooltip("Y축 스케일 (0.2 = 20%)")]
    [SerializeField] private float squashAmount = 0.2f;
    [Tooltip("X, Z축 스케일 (1.5 = 150%)")]
    [SerializeField] private float squashWidenAmount = 1.5f;
    [Tooltip("납작해지는 애니메이션 속도")]
    [SerializeField] private float squashAnimSpeed = 10f;
    #endregion 

    private Vector3 originalModelScale;
    private Transform originalModelTransform;
    private MountController mountController;
    void Awake()
    {
        originalModelScale = playerModelTransform.localScale;
        mountController = GetComponent<MountController>();
        ChangeState(new RunningState());
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }
    void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.MoveEvent += HandleMove;
            inputReader.MoveCanceledEvent += HandleMoveCanceled;
            inputReader.SprintEvent += HandleSprint;
            inputReader.SprintCanceledEvent += HandleSprintCanceled;
            inputReader.JumpEvent += HandleJump;
            inputReader.HeadbuttEvent += HandleHeadbutt;


            // 게임플레이 입력 활성화
            inputReader.EnableGamePlayActionInputs();
        }
    }

    void OnDisable()
    {
        // OnEnable에서 구독한 모든 이벤트를 반드시 해지해야 합니다.
        if (inputReader != null)
        {
            inputReader.MoveEvent -= HandleMove;
            inputReader.MoveCanceledEvent -= HandleMoveCanceled;
            inputReader.SprintEvent -= HandleSprint;
            inputReader.SprintCanceledEvent -= HandleSprintCanceled;
            inputReader.JumpEvent -= HandleJump;
            inputReader.HeadbuttEvent -= HandleHeadbutt;
            // 예: inputReader.GlideEvent -= HandleGlide;
        }
    }

    void Update()
    {
        CheckGround();
        currentState?.Update(this);

        if (rb.linearVelocity.y <= dustSpawnVel)
        {
            canSpawnDustLand = true;
        }

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTimeDuration;
        }
        else
        {
            // 공중에 있으면 코요테 시간 감소
            coyoteTimeCounter -= Time.deltaTime;
        }

        CheckForFallDeath();
    }

    void FixedUpdate()
    {
        currentState?.FixedUpdate(this);
    }

    public void ChangeState(IPlayerState newState)
    {
        Debug.Log($"상태 변경: {currentState?.GetType().Name} -> {newState.GetType().Name}");

        if (currentState is GlideState && !(newState is GlideState))
        {
            // Glide 상태에서 벗어나는 경우
            mountController?.OnPlayerGlide(false);
        }

        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);

        if (newState is JumpState)
        {
            mountController?.OnPlayerJump();
        }
        else if (newState is GlideState)
        {
            mountController?.OnPlayerGlide(true);
        }
    }
    private void HandleMove(Vector2 moveInput)
    {
        // MoveInput 변수에 직접 값을 넣어줍니다. Update에서 이미 ReadValue를 하고 있으므로
        MoveInput = moveInput;
        LastMoveInput = moveInput;

    }

    private void HandleHeadbutt()
    {
        //박치기 State ㄱㄱ혓
        ChangeState(new HeadbuttState());
    }
    private void HandleMoveCanceled(Vector2 moveInput)
    {
        MoveInput = Vector2.zero;
    }

    private void HandleSprint()
    {
        isRunning = true;
        airSpeed = sprintSpeed;
    }

    private void HandleSprintCanceled()
    {
        isRunning = false;
        airSpeed = walkSpeed;
    }

    private void HandleJump()
    {
        if (coyoteTimeCounter > 0f && !isJumping)
        {
            //JumpSoundPlay();
            ChangeState(new JumpState());
            //playerHapticEvent.TriggerPlayerEvent(PlayerEventType.Jumped);
            coyoteTimeCounter = 0f; // 점프하면 즉시 시간 초기화
        }
    }


    public void FootStepSoundPlay()
    {
        //string soundToPlay = footstepSoundNames[Random.Range(0, footstepSoundNames.Length)];
        //soundManager.PlaySFX(soundToPlay);
    }

    public void JumpSoundPlay()
    {
        soundManager.PlaySFX("JumpSound");
    }
    public void LandSoundPlay()
    {
        soundManager.PlaySFX("LandSound");
    }

    // 글라이드 
    private void HandleGlide()
    {
        ChangeState(new GlideState());
    }

    private void HandleGlideCanceled()
    {
        if (currentState is GlideState)
        {
            ChangeState(new RunningState());
        }
    }



    public bool IsOnIceGround()
    {
        // 플레이어 발밑으로 레이쏴서 iceLayer만 맞는지 확인
        return Physics.Raycast(transform.position, Vector3.down, out _, raycastDistance + sphereRadius, iceLayer);
    }

    /// <summary>
    /// 플레이어가 땅에 닿아있는지 체크하는 함수
    /// 3개의 SphereCast 중 하나라도 땅에 닿아있다면 isGrounde = true
    /// </summary>
    public void CheckGround()
    {
        RaycastHit hit;
        foreach (var point in groundCheckPoints)
        {
            if (Physics.SphereCast(point.position, sphereRadius, Vector3.down, out hit, raycastDistance, groundLayer))
            {
                isGrounded = true;
                isJumping = false;
                wasFalling = false;
                //glideLocked = false;

                if (canSpawnDustLand)
                {
                    playerHapticEvent.TriggerPlayerEvent(PlayerEventType.Landed);
                }

                if (!isInWindZone)
                {
                    animator.SetBool("isFly", false);
                }

                animator.SetBool("isGrounded", true);
                break;
            }
            else
            {
                isGrounded = false;
                animator.SetBool("isGrounded", false);
                if (rb.linearVelocity.y < -0.1f && !wasFalling)
                {
                    wasFalling = true;

                }
            }
        }

        isOnIce = IsOnIceGround();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("jumpPad") || other.CompareTag("windZone"))
        {
            isInWindZone = true;
            animator.SetBool("isGrounded", false);
            animator.SetBool("isFly", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("jumpPad") || other.CompareTag("windZone"))
        {
            isInWindZone = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 장애물 태그를 가진 오브젝트와 충돌했는지, 그리고 현재 HitState가 아닌지 확인합니다.
        if (currentState is HitState) return;

        if (collision.gameObject.CompareTag(obstacleTag))
        {
            ChangeState(new HitState());
            // 2. 충돌 지점의 반대 방향으로 밀려날 방향을 계산
            Vector3 knockbackDirection = (transform.position - collision.contacts[0].point).normalized;
            knockbackDirection.y = 0;

            rb.linearVelocity = Vector3.zero;
        }
        else if (collision.gameObject.CompareTag("BreakableRock"))
        {
            Debug.Log("BreakableRock과 충돌");
            ChangeState(new SquashState());
        }
    }

    #region 납작해지는 코루틴

    public void StartSquashAndRecover()
    {
        // 이전에 실행 중이던 코루틴이 있다면 중지 (중복 실행 방지)
        StopCoroutine("SquashAndRecoverCoroutine");
        StartCoroutine(SquashAndRecoverCoroutine());
    }

    // 스케일을 변경하고, 기다렸다가, 복구하는 실제 로직
    private System.Collections.IEnumerator SquashAndRecoverCoroutine()
    {
        if (playerModelTransform == null)
        {
            ChangeState(new RunningState()); // 오류 발생 시 강제로 상태 복구
            yield break;
        }

        // 1. 원래 스케일 저장 및 목표 스케일 계산
        Vector3 squashedScale = new Vector3(originalModelScale.x * squashWidenAmount,
                                           originalModelScale.y * squashWidenAmount,
                                           originalModelScale.z * squashAmount);

        // --- 2. 납작해지는 애니메이션 (Lerp 사용) ---
        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * squashAnimSpeed;
            playerModelTransform.localScale = Vector3.Lerp(originalModelScale, squashedScale, t);
            yield return null;
        }
        playerModelTransform.localScale = squashedScale; // 정확히 목표 스케일로 설정

        // --- 3. 납작한 상태로 대기 ---
        yield return new WaitForSeconds(squashDuration);

        // --- 4. 원래대로 돌아오는 애니메이션 (Lerp 사용) ---
        t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * squashAnimSpeed;
            playerModelTransform.localScale = Vector3.Lerp(squashedScale, originalModelScale, t);
            yield return null;
        }
        playerModelTransform.localScale = originalModelScale; // 정확히 원래 스케일로 복구

        // --- 5. 상태를 RunningState로 복구 ---
        ChangeState(new RunningState());
    }

    #endregion
    //SphereCast Gizmo 그리는 코드
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoints == null) return;

        Gizmos.color = Color.green;
        foreach (var point in groundCheckPoints)
        {
            Vector3 start = point.position;
            Vector3 end = start + Vector3.down * raycastDistance;
            // 구의 궤적 그리기
            Gizmos.DrawWireSphere(start, sphereRadius);
            Gizmos.DrawWireSphere(end, sphereRadius);
            // 사이 선으로 연결
            Gizmos.DrawLine(start, end);
        }
    }

    #region 경사 계산 함수

    // 플레이어가 경사 위에 있는 지 확인하는 코드
    // 플레이어 아래로 rayCast를 쏴서 angle이 0이 아니면 isOnSlope = true
    public bool IsOnSlope()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out slopeHit, raycastDistance, groundLayer))
        {
            var angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle != 0f && angle < maxSlopeAngle;
        }
        return false;
    }

    // 경사 위에서의 각도를 계산하는 코드
    // 평지에서는 Quaternion.Euler(0, 0, 0)로 설정
    //public Quaternion SurfaceAlignment()
    //{
    //    Quaternion RotationRef = Quaternion.Euler(0, 0, 0);

    //    if (IsOnSlope())
    //    {
    //        Vector3 adjustedForward = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    //        Quaternion targetRotation = Quaternion.LookRotation(adjustedForward, slopeHit.normal);
    //        RotationRef = Quaternion.Lerp(transform.rotation, targetRotation, animCurve.Evaluate(timer));
    //    }

    //    return RotationRef;
    //}
    // PlayerController.cs의 기존 SurfaceAlignment 함수를 삭제하고 아래 코드로 붙여넣으세요.
    public Quaternion SurfaceAlignment()
    {
        Quaternion targetRotation;

        if (IsOnSlope() && isGrounded)
        {
            // 경사로에 있을 때
            // 1. 사용할 전방 벡터 결정 (움직일 땐 입력 방향, 멈췄을 땐 현재 바라보는 방향)
            Vector3 forwardToUse = moveDirection.magnitude > 0.01f ? moveDirection : transform.forward;

            // 2. 전방 벡터를 경사면 법선(normal)에 투영하여 경사면에 평행한 방향 계산
            Vector3 adjustedForward = Vector3.ProjectOnPlane(forwardToUse, slopeHit.normal).normalized;

            // 3. 만약 정면이 경사면과 거의 수직이라 계산이 0에 가까워지면(드문 경우), transform.up을 대신 사용
            if (adjustedForward.sqrMagnitude < 0.01f)
            {
                adjustedForward = Vector3.ProjectOnPlane(transform.up, slopeHit.normal).normalized;
            }

            // 4. 경사면에 맞춘 최종 목표 회전값 계산 (바라볼 방향: adjustedForward, 위쪽: slopeHit.normal)
            targetRotation = Quaternion.LookRotation(adjustedForward, slopeHit.normal);
        }
        else
        {
            // 평지에 있거나 공중에 있을 때
            // 1. 사용할 전방 벡터 결정
            Vector3 forwardToUse = moveDirection.magnitude > 0.01f
                ? moveDirection // 움직일 땐 입력 방향
                : new Vector3(transform.forward.x, 0, transform.forward.z).normalized; // 멈췄을 땐 현재 y축 회전만

            // 2. 캐릭터가 기울어지지 않도록 y축 회전만 계산 (바라볼 방향: forwardToUse, 위쪽: Vector3.up)
            if (forwardToUse.sqrMagnitude > 0.01f)
            {
                targetRotation = Quaternion.LookRotation(forwardToUse, Vector3.up);
            }
            else
            {
                // 완전히 멈췄고 입력도 없으면 현재 y축 회전 유지
                targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            }
        }

        // 5. 현재 회전에서 목표 회전으로 부드럽게 보간 (Slerp 사용)
        // 이 함수는 FixedUpdate에서 호출될 것이므로 Time.fixedDeltaTime 사용
        return Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.fixedDeltaTime);
    }

    //이거 뭐더라 기억안남
    public Vector3 AdjustDirectionToSlope(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    // 경사 각도 체크 함수
    public float CalculateNextFrameGroundAngle(float moveSpeed)
    {
        var nextFramePlayerPosition =
                           raycastOrigin.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

        if (Physics.Raycast(nextFramePlayerPosition, Vector3.down, out RaycastHit hitInfo,
                            0.5f, groundLayer))
            return Vector3.Angle(Vector3.up, hitInfo.normal);
        return 0f;
    }

    #endregion

    #region 리스폰 함수
    private void CheckForFallDeath()
    {
        if (transform.position.y < fallDeathYLevel)
        {
            Respawn();
        }
    }

    // 리스폰을 처리하는 함수
    public void Respawn()
    {
        Debug.Log("플레이어가 마지막 체크포인트에서 리스폰됩니다.");

        // Rigidbody의 속도를 초기화하여 떨어지던 가속도를 없애줍니다.
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Checkpoint 스크립트에 저장된 마지막 활성 위치로 플레이어를 즉시 이동시킵니다.
        // 순간이동 시 발생할 수 있는 물리적 오류를 방지하기 위해 잠시 CharacterController나 Rigidbody를 비활성화했다가 켜는 것이 더 안정적일 수 있습니다.
        transform.position = RespawnPoint.LastActivatedRespawnPpointPosition;

        // 여기에 체력 초기화 등 리스폰 시 필요한 다른 로직을 추가할 수 있습니다.
    }

    #endregion
}

