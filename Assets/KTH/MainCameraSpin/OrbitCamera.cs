using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] Transform focus = default;  // 회전 중심 오브젝트
    [SerializeField, Range(0f,1000f)] float distance = 100f;  // 거리
    [SerializeField, Range(1f, 360f)] float rotationSpeed = 90f;  // 회전 속도

    Vector2 orbitAngles = new Vector2(45f, 0f);  // X: 수직각, Y: 수평각
    Vector3 focusPoint;

    void Awake()
    {
        focusPoint = focus.position;
        transform.localRotation = Quaternion.Euler(orbitAngles);
    }

    void LateUpdate()
    {
        UpdateFocusPoint();
        AutomaticRotation();

        Quaternion lookRotation = Quaternion.Euler(orbitAngles);
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;

        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    void UpdateFocusPoint()
    {
        focusPoint = focus.position;
    }

    void AutomaticRotation()
    {
        orbitAngles.y += rotationSpeed * Time.unscaledDeltaTime;

        // 수평 회전 각도를 0-360 범위로 유지
        if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }
}
