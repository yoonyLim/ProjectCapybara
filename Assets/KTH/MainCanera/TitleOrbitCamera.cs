using UnityEngine;

/// <summary>
/// [수정됨] 1단계: 'target'을 중심으로 카메라를 자동 회전시킵니다.
/// 인스펙터에서 설정한 '거리'와 '높이' 변수만 사용해 위치를 계산합니다.
/// </summary>
public class TitleOrbitCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("카메라가 바라보며 회전할 중심 대상입니다.")]
    public Transform target;

    [Header("Orbit Settings")]
    [Tooltip("타겟으로부터 수평으로 떨어질 거리입니다.")]
    public float distance = 10.0f;

    [Tooltip("타겟의 Y축 위치(발)를 기준으로 카메라가 얼마나 높이 있을지 정합니다.")]
    public float height = 5.0f;

    // 'initialAngle' 및 'currentYAngle' 변수 제거

    [Tooltip("1초에 회전할 각도(속도)입니다.")]
    public float rotationSpeed = 10.0f;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("TitleOrbitCamera: Target이 할당되지 않았습니다!", this);
            this.enabled = false;
            return;
        }

        // 1. 초기 위치 설정 (타겟 뒤, 거리/높이 적용)
        // 씬 뷰의 위치 대신, 이 값으로 강제 설정됩니다.
        Vector3 startPosition = target.position - (Vector3.forward * distance) + (Vector3.up * height);
        transform.position = startPosition;

        // 2. 타겟을 즉시 바라보게 함 (X축 각도 자동 계산)
        transform.LookAt(target.position);
    }

    // LateUpdate는 모든 Update가 끝난 후 호출되므로 카메라 이동에 적합합니다.
    void LateUpdate()
    {
        if (target == null) return;

        // 3. 타겟을 중심으로 Y축 회전
        transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);

        // 4. [중요] 회전 후에도 계속 타겟을 바라보도록 강제 (X축 각도 자동 조절)
        transform.LookAt(target.position);
    }
}