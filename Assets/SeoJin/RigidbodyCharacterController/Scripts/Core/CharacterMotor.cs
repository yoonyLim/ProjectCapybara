using System;
using System.Collections.Generic;
using Moko;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.Serialization;

namespace Moko
{
    public interface IPlayerModule
    {
        Vector3 CalculateVelocity(CharacterMotor motor);
    }

    public class CharacterMotor : MonoBehaviour
    {
        public Rigidbody Rb { get; private set; }
        private IPlayerModule[] _modules;
        private PlayerInput _playerInput;
        public PlayerAnimator playerAnimator { get; private set; }
        
        public Camera playerCamera;


        [Header("Data Properties")] public MovementData MovementData;
        public JumpData JumpData;
        public GravityData GravityData;
        public DashData DashData;

        [Header("CheckGrounded Properties")]
        [SerializeField] private List<Transform> groundCheckOffsets;

        [SerializeField] private float sphereCastRadius;
        [SerializeField] private float sphereCastMaxDistance;
        [SerializeField] private LayerMask groundLayerMask;

        [Header("Wall Detection Properties")]
        [SerializeField] private float playerHeight = 2f;
        [SerializeField] private float playerRadius = 0.5f;
        [SerializeField] private float wallCheckDistance = 0.5f;
        [SerializeField] private LayerMask wallLayerMask;

        public Vector3 ApproximatedGroundNormal;
        public RaycastHit GroundHit { get; private set; }
        public RaycastHit WallHit { get; private set; }
        public Vector3 RawMoveDirection { get; private set; }
        public Vector3 CurrentVelocity { get; private set; }
        public bool IsOnValidGround { get; private set; }
        public bool IsSliding { get; private set; }
        private bool IsFrontGrounded;
        private bool IsMiddleGrounded;
        private bool IsBackGrounded;
        public bool IsGrounded { get; private set; }
        public bool IsAgainstWall { get; private set; }
        public bool IsDashing { get; set; }
        public bool RetainAirMovement { get; set; }

        [Header("Initialize Options")] 
        public bool MoveComponent;
        public bool RotationComponent;
        public bool GravityComponent;
        public bool JumpComponent;
        public bool SlideComponent;
        public bool DashComponent;

        [HideInInspector] public PlayerMove PlayerMove;
        [HideInInspector] public PlayerRotation PlayerRotation;
        [HideInInspector] public PlayerGravity PlayerGravity;
        [HideInInspector] public PlayerJump PlayerJump;
        [HideInInspector] public PlayerSlide PlayerSlide;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Rb.useGravity = false;

            _playerInput = GetComponent<PlayerInput>();
            if (MoveComponent) PlayerMove = transform.AddComponent<PlayerMove>();
            if (RotationComponent) PlayerRotation = transform.AddComponent<PlayerRotation>();
            if (GravityComponent) PlayerGravity = transform.AddComponent<PlayerGravity>();
            if (JumpComponent) PlayerJump = transform.AddComponent<PlayerJump>();
            if (SlideComponent) PlayerSlide = transform.AddComponent<PlayerSlide>();

            _modules = GetComponents<IPlayerModule>();
            playerAnimator = GetComponentInChildren<PlayerAnimator>();
        }


        private void Update()
        {
            CheckLog();
        }

        private void FixedUpdate()
        {
            CalculateRawMoveDirection();
            CheckGrounded();
            CheckWall();

            if (IsOnValidGround && RetainAirMovement == true)
            {
                RetainAirMovement = false;
            }

            Move();
        }

        private void CalculateRawMoveDirection()
        {
            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            RawMoveDirection = (forward * _playerInput.MoveInput.y + right * _playerInput.MoveInput.x).normalized;
        }

        private void CheckGrounded()
        {
            IsFrontGrounded = Physics.SphereCast(
                groundCheckOffsets[0].transform.position,
                sphereCastRadius,
                Vector3.down,
                out RaycastHit hit1,
                sphereCastMaxDistance,
                groundLayerMask
            );
            
            IsMiddleGrounded = Physics.SphereCast(
                groundCheckOffsets[1].transform.position,
                sphereCastRadius,
                Vector3.down,
                out RaycastHit hit2,
                sphereCastMaxDistance,
                groundLayerMask
            );
            
            IsBackGrounded = Physics.SphereCast(
                groundCheckOffsets[2].transform.position,
                sphereCastRadius,
                Vector3.down,
                out RaycastHit hit3,
                sphereCastMaxDistance,
                groundLayerMask
            );
            
            int groundedCounter = (IsFrontGrounded?1:0) + (IsMiddleGrounded?1:0) + (IsBackGrounded?1:0);
            IsGrounded = groundedCounter >= 1;

            if (IsGrounded)
            {
                ApproximatedGroundNormal = Vector3.up;
                ApproximatedGroundNormal = ((
                    (IsFrontGrounded?hit1.normal:Vector3.zero) 
                    + (IsMiddleGrounded?hit2.normal:Vector3.zero) 
                    + (IsBackGrounded?hit3.normal:Vector3.zero)) / 3).normalized;
                
                float approximatedSlopeAngle = Vector3.Angle(Vector3.up, ApproximatedGroundNormal);
                IsOnValidGround = approximatedSlopeAngle <= MovementData.maxSlopeAngle;
                IsSliding = !IsOnValidGround;
            }
            else
            {
                IsOnValidGround = false;
                IsSliding = false;
            }
        }

        private void CheckWall()
        {
            IsAgainstWall = false;

            if (RawMoveDirection.sqrMagnitude > float.Epsilon)
            {
                if (Physics.CapsuleCast(
                        transform.position,
                        transform.position + (Vector3.up * playerHeight),
                        playerRadius,
                        RawMoveDirection,
                        out RaycastHit hit,
                        wallCheckDistance,
                        wallLayerMask
                    ))
                {
                    if (Vector3.Angle(Vector3.up, hit.normal) > MovementData.wallAngle)
                    {
                        IsAgainstWall = true;
                        WallHit = hit;
                    }
                }
            }
        }

        private void Move()
        {
            Vector3 finalVelocity = Vector3.zero;
            foreach (var module in _modules)
            {
                finalVelocity += module.CalculateVelocity(this);
            }

            CurrentVelocity = finalVelocity;

            Vector3 horizontalVelocity = new Vector3(CurrentVelocity.x, 0, CurrentVelocity.z);
            Vector3 verticalVelocity = new Vector3(0, CurrentVelocity.y, 0);
            Rb.linearVelocity = horizontalVelocity + verticalVelocity;
        }


        //------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        private void CheckLog()
        {
            DebugExtension.ColorLog($"IsGrounded : {IsOnValidGround}", "cyan");
            DebugExtension.ColorLog($"IsAgainstWall : {IsAgainstWall}", "red");
        }

        private void OnDrawGizmos()
        {
            // 기존 CheckGrounded Gizmo 코드
            bool isHit1 = Physics.SphereCast(
                groundCheckOffsets[0].transform.position,
                sphereCastRadius,
                Vector3.down,
                out RaycastHit hit1,
                sphereCastMaxDistance,
                groundLayerMask
            );

            Gizmos.color = isHit1 ? Color.green : Color.red;
            Vector3 startCenter1 = groundCheckOffsets[0].transform.position;
            float distance1 = isHit1 ? hit1.distance : sphereCastMaxDistance;
            Vector3 endCenter1 = startCenter1 + Vector3.down * distance1;

            Gizmos.DrawWireSphere(startCenter1, sphereCastRadius);
            Gizmos.DrawWireSphere(endCenter1, sphereCastRadius);
            Gizmos.DrawLine(startCenter1, endCenter1);

            if (isHit1)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(hit1.point, 0.1f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(hit1.point, hit1.point + hit1.normal * 0.5f);
            }
            
            
            
            
            bool isHit2 = Physics.SphereCast(
                groundCheckOffsets[1].transform.position,
                sphereCastRadius,
                Vector3.down,
                out RaycastHit hit2,
                sphereCastMaxDistance,
                groundLayerMask
            );

            Gizmos.color = isHit2 ? Color.green : Color.red;
            Vector3 startCenter2 = groundCheckOffsets[1].transform.position;
            float distance2 = isHit2 ? hit2.distance : sphereCastMaxDistance;
            Vector3 endCenter2 = startCenter2 + Vector3.down * distance2;

            Gizmos.DrawWireSphere(startCenter2, sphereCastRadius);
            Gizmos.DrawWireSphere(endCenter2, sphereCastRadius);
            Gizmos.DrawLine(startCenter2, endCenter2);

            if (isHit2)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(hit2.point, 0.1f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(hit2.point, hit2.point + hit2.normal * 0.5f);
            }
            
            
            
            bool isHit3 = Physics.SphereCast(
                groundCheckOffsets[2].transform.position,
                sphereCastRadius,
                Vector3.down,
                out RaycastHit hit3,
                sphereCastMaxDistance,
                groundLayerMask
            );

            Gizmos.color = isHit3 ? Color.green : Color.red;
            Vector3 startCenter = groundCheckOffsets[2].transform.position;
            float distance = isHit3 ? hit3.distance : sphereCastMaxDistance;
            Vector3 endCenter = startCenter + Vector3.down * distance;

            Gizmos.DrawWireSphere(startCenter, sphereCastRadius);
            Gizmos.DrawWireSphere(endCenter, sphereCastRadius);
            Gizmos.DrawLine(startCenter, endCenter);

            if (isHit3)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(hit3.point, 0.1f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(hit3.point, hit3.point + hit3.normal * 0.5f);
            }

            // CheckWall Gizmo 코드 추가
            if (RawMoveDirection.sqrMagnitude > float.Epsilon)
            {
                // CapsuleCast 시각화
                Vector3 capsuleStart = transform.position;
                Vector3 capsuleEnd = transform.position + (Vector3.up * playerHeight);
                Vector3 castDirection = RawMoveDirection;

                // 실제 CheckWall과 동일한 CapsuleCast 수행
                bool wallHit = Physics.CapsuleCast(
                    capsuleStart,
                    capsuleEnd,
                    playerRadius,
                    castDirection,
                    out RaycastHit wallHitInfo,
                    wallCheckDistance,
                    wallLayerMask
                );

                // 캡슐 색상 설정 (벽 충돌 여부에 따라)
                if (wallHit && Vector3.Angle(Vector3.up, wallHitInfo.normal) > MovementData.wallAngle)
                {
                    Gizmos.color = Color.red; // 벽에 충돌
                }
                else if (wallHit)
                {
                    Gizmos.color = Color.red + Color.yellow; // 충돌했지만 벽 각도가 아님
                }
                else
                {
                    Gizmos.color = Color.cyan; // 충돌 없음
                }

                // 시작 위치의 캡슐 그리기
                DrawCapsule(capsuleStart, capsuleEnd, playerRadius);

                // 이동 방향 표시
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, transform.position + castDirection * wallCheckDistance);

                // 끝 위치의 캡슐 그리기
                Vector3 endCapsuleStart = capsuleStart + castDirection * wallCheckDistance;
                Vector3 endCapsuleEnd = capsuleEnd + castDirection * wallCheckDistance;

                Gizmos.color = Color.white;
                DrawCapsule(endCapsuleStart, endCapsuleEnd, playerRadius);

                // 벽 충돌 시 충돌 지점과 법선 표시
                if (wallHit)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(wallHitInfo.point, 0.1f);

                    // 법선 벡터 표시
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(wallHitInfo.point, wallHitInfo.point + wallHitInfo.normal * 0.5f);

                    // 각도 정보 표시용 (벽 각도 체크)
                    float wallAngle = Vector3.Angle(Vector3.up, wallHitInfo.normal);
                    Gizmos.color = wallAngle > MovementData.wallAngle ? Color.red : Color.green;
                    Gizmos.DrawLine(wallHitInfo.point, wallHitInfo.point + Vector3.up * 0.3f);
                }
            }
        }

// 캡슐 그리기 헬퍼 함수
        private void DrawCapsule(Vector3 start, Vector3 end, float radius)
        {
            // 캡슐의 중심축
            Vector3 center = (start + end) / 2f;
            float height = Vector3.Distance(start, end);

            // 상단과 하단 원 그리기
            Gizmos.DrawWireSphere(start, radius);
            Gizmos.DrawWireSphere(end, radius);

            // 캡슐의 측면 선 그리기
            Vector3 forward = Vector3.forward * radius;
            Vector3 right = Vector3.right * radius;

            // 4개의 측면 선
            Gizmos.DrawLine(start + forward, end + forward);
            Gizmos.DrawLine(start - forward, end - forward);
            Gizmos.DrawLine(start + right, end + right);
            Gizmos.DrawLine(start - right, end - right);
        }
#endif
    }
}