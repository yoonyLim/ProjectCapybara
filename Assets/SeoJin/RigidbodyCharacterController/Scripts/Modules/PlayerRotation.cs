/*
using UnityEngine;

namespace Moko
{
    public class PlayerRotation : MonoBehaviour
    {
        private CharacterMotor motor;

        private void Awake()
        {
            motor = GetComponent<CharacterMotor>();
        }

        private void Update()
        {
            motor.Rb.angularVelocity = Vector3.zero;

            Vector3 lookDirection = motor.RawMoveDirection;
            if (lookDirection.sqrMagnitude < float.Epsilon) return;

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            Quaternion newRotation = Quaternion.Slerp(
                motor.Rb.rotation,
                targetRotation,
                motor.MovementData.rotationSpeed * Time.fixedDeltaTime
            );

            motor.Rb.MoveRotation(newRotation);
        }
    }
}
*/


using UnityEngine;

namespace Moko
{
    public class PlayerRotation : MonoBehaviour
    {
        private CharacterMotor motor;

        [Header("Rotation Speeds")]
        [Tooltip("경사면에 맞춰 정렬되는 속도")]
        public float alignmentSpeed = 15f;

        [Tooltip("공중에서 다시 정면으로 돌아오는 속도")]
        public float airAlignmentSpeed = 8f;

        private void Awake()
        {
            motor = GetComponent<CharacterMotor>();
        }

        // Update가 아닌 FixedUpdate를 사용해야 합니다.
        private void FixedUpdate()
        {
            motor.Rb.angularVelocity = Vector3.zero;

            // --- 1. 목표 'Up' 벡터(수직 정렬) 결정 ---
            Vector3 targetUp;
            float currentSpeed;

            if (motor.IsOnValidGround)
            {
                // 땅 위: 경사면의 Normal을 Up으로 설정
                targetUp = motor.ApproximatedGroundNormal;
                currentSpeed = alignmentSpeed;
            }
            else
            {
                // 공중 또는 미끄러짐: 월드 Up을 Up으로 설정
                targetUp = Vector3.up;
                currentSpeed = airAlignmentSpeed;
            }

            // --- 2. 목표 'Forward' 벡터(바라볼 방향) 결정 ---
            Vector3 desiredForward = motor.RawMoveDirection;
            
            // 입력이 없으면 현재 바라보는 방향을 유지
            if (desiredForward.sqrMagnitude < float.Epsilon)
            {
                desiredForward = transform.forward;
            }

            // --- 3. 두 벡터를 조합하여 최종 회전값 계산 ---

            // '앞' 방향을 '위' 방향이 정의하는 평면에 투영합니다.
            Vector3 targetForward = Vector3.ProjectOnPlane(desiredForward, targetUp);

            // 엣지 케이스: 경사면이 너무 가파르거나 입력이 0에 가까워 targetForward가 0이 될 때
            if (targetForward.sqrMagnitude < 0.01f)
            {
                // 현재 '오른쪽' 방향을 대신 투영하여 앞 방향을 찾습니다.
                targetForward = Vector3.ProjectOnPlane(transform.right, targetUp);
                
                // 이것도 실패하면 (예: 완전 수직) 더 이상 회전하지 않음
                if (targetForward.sqrMagnitude < 0.01f)
                {
                    return;
                }
            }

            // 최종 목표 회전값 생성
            Quaternion targetRotation = Quaternion.LookRotation(targetForward.normalized, targetUp);

            // 부드럽게 회전 적용
            Quaternion smoothedRotation = Quaternion.Slerp(
                motor.Rb.rotation,
                targetRotation,
                motor.MovementData.rotationSpeed * Time.fixedDeltaTime // FixedUpdate에서는 Time.fixedDeltaTime 사용
            );

            motor.Rb.MoveRotation(smoothedRotation);
        }
    }
}