using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    private PlayerInput defaultAction;
    private Rigidbody rb;
    private Animator animator;
    private Vector3 InputKey;
    float Myfloat;

    private Vector3 moveDirection;
    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;
    public float glideSpeed = 4.5f;
    bool isRunning;

    [Header("레이어 구분")]
    public LayerMask groundLayer;
    public LayerMask waterLayerMask;

    private float raycastDistance = 0.5f;
    private bool isGrounded;

    private bool isJumping = false;
    private bool wasFalling = false;
    private float jumpForce = 6f;

    private bool isSwimming = false;
  

    private float glideGravity = 4.0f; 
    private float normalGravity = 9.81f;
    private float glideDrag = 5f;              
    private float normalDrag = 0f;            
    private bool isGliding = false;

    private float normalSmoothing = 0.1f;    
    private float glideSmoothing = 0.5f;      
    float currentSmoothing;

    private float glideTurnSpeed = 50f; // 초당 최대 회전 각도
    private float targetGlideAngle; // 목표 방향 각도
    private float currentGlideAngle; // 실제 캐릭터가 바라보는 각도

    private void Start()
    {
        defaultAction = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        var glideAction = defaultAction.actions["Glide"];
        glideAction.performed += ctx => StartGlide();
        glideAction.canceled += ctx => StopGlide();

        var sprintAction = defaultAction.actions["Sprint"];
        sprintAction.performed += ctx => StartSprint();
        sprintAction.canceled += ctx => StopSprint();
    }

    void Update()
    {
        if (isGliding)
            currentSmoothing = glideSmoothing;   // 활강중엔 회전 속도 느리게
        else
            currentSmoothing = normalSmoothing;

        if (isGliding && isGrounded) StopGlide();

        

        // Ground check
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            isGrounded = true;
            isJumping = false;
            wasFalling = false;
            animator.SetBool("isFall", false);
        }
        else
        {
            isGrounded = false;

            if (rb.linearVelocity.y < -0.1f && !wasFalling)
            {
                wasFalling = true;             
                animator.SetBool("isJump", false);
                animator.SetBool("isFall", true);
            }
        }
    }

    void FixedUpdate()
    {
        bool hasControl = (moveDirection !=  Vector3.zero);
        if (isGliding)
        {
            if (moveDirection.sqrMagnitude > 0.01f)
                targetGlideAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

            // 현재 각도에서 목표 각도로, 부드럽게 Lerp(=관성/딜레이) 처리
            float newAngle = Mathf.MoveTowardsAngle(
                transform.eulerAngles.y,  
                targetGlideAngle,          
                glideTurnSpeed * Time.deltaTime   // 초당 최대 회전
            );
            transform.rotation = Quaternion.Euler(0, newAngle, 0);

            // 이전처럼 전방 이동
            Vector3 forward = transform.forward;
            rb.MovePosition(transform.position + forward * glideSpeed * Time.deltaTime);
        }
        else if (hasControl)
        {
            float speed = isRunning ? runSpeed : walkSpeed;
            rb.MovePosition(transform.position + moveDirection * speed * Time.deltaTime);

            if (isSwimming)
                animator.SetBool("isSwim", true);
            else
                animator.SetInteger("Walk", isRunning ? 2 : 1);

            float angle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float smooth = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref Myfloat, currentSmoothing);
            transform.rotation = Quaternion.Euler(0, smooth, 0);

        }
        else
        {
            if (isSwimming)
                animator.SetBool("isSwim", false);
            else
                animator.SetInteger("Walk", 0);
        }

        // 글라이딩 시 이동 코드
        if (isGliding)
        {
            Vector3 glideMove = new Vector3(moveDirection.x, 0, moveDirection.z);
            rb.AddForce(glideMove * 7f, ForceMode.Acceleration);
        }

    }

    void OnJump()
    {
        if(isGrounded)
        {
            isJumping = true;
            animator.SetBool("isJump", true);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        if (isGliding)
        {
            if (input.magnitude > 0.1f)
                moveDirection = new Vector3(input.x, 0, input.y).normalized;
            // 입력이 없을 땐 방향 유지
        }
        else
        {
            moveDirection = new Vector3(input.x, 0f, input.y);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if ((waterLayerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            // waterLayerMask에 포함된 Layer일 때만 실행
            isSwimming = true;
            animator.SetBool("isInWater", true);
            rb.useGravity = false;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if ((waterLayerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            isSwimming = false;
            animator.SetBool("isInWater", false);
            rb.useGravity = true;
        }
    }

    void StartSprint()
    {
        isRunning = true;
    }

    void StopSprint()
    {
        isRunning = false;
    }
    void StartGlide()
    {
        // 땅에 붙었으면 글라이딩 안됨
        if (isGrounded) return;
        if (isGliding) return;

        isGliding = true;
        animator.SetBool("isFly", true); // 자유롭게 원하는 파라미터로
        rb.linearDamping = glideDrag;
        Physics.gravity = new Vector3(0, -glideGravity, 0);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z); // 낙하 초기속도 완화. 값 조절 가능
    }

    void StopGlide()
    {
        if (!isGliding) return;

        isGliding = false;
        animator.SetBool("isFly", false);
        rb.linearDamping = normalDrag;
        Physics.gravity = new Vector3(0, -normalGravity, 0);
    }
}
