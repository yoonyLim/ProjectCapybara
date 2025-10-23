using UnityEngine;

public class CloudWandering : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float directionChangeInterval = 10f;

    [Header("Wander Bounds (Local Space)")]
    [SerializeField] private Vector3 minBounds = new Vector3(-100, -100, -100);
    [SerializeField] private Vector3 maxBounds = new Vector3(100, 100, 100);

    private Vector3 currentVelocity;
    private float timeSinceDirectionChange;

    void Start()
    {
        SetNewRandomDirection();
    }

    void Update()
    {
        timeSinceDirectionChange += Time.deltaTime;
        if (timeSinceDirectionChange >= directionChangeInterval)
        {
            SetNewRandomDirection();
        }

        // [변경됨] 로컬 위치 기준 계산
        Vector3 newLocalPosition = transform.localPosition + currentVelocity * moveSpeed * Time.deltaTime;

        // [변경됨] 로컬 좌표 기준 Clamp
        newLocalPosition.x = Mathf.Clamp(newLocalPosition.x, minBounds.x, maxBounds.x);
        newLocalPosition.y = Mathf.Clamp(newLocalPosition.y, minBounds.y, maxBounds.y);
        newLocalPosition.z = Mathf.Clamp(newLocalPosition.z, minBounds.z, maxBounds.z);

        // [변경됨] 로컬 위치 설정
        transform.localPosition = newLocalPosition;

        bool hitWall = (newLocalPosition.x == minBounds.x || newLocalPosition.x == maxBounds.x ||
                        newLocalPosition.y == minBounds.y || newLocalPosition.y == maxBounds.y ||
                        newLocalPosition.z == minBounds.z || newLocalPosition.z == maxBounds.z);

        if (hitWall)
        {
            SetNewRandomDirection();
        }
    }

    void SetNewRandomDirection()
    {
        currentVelocity = Random.onUnitSphere;
        timeSinceDirectionChange = 0f;
    }
}