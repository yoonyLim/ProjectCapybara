using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class SplineFollower : MonoBehaviour
{

    

    [Header("회전 설정")]
    // [SerializeField] private float rotationSpeed = 50f; // <-- 제거: 랜덤으로 설정됩니다.
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("프리팹 설정")] // <-- 추가
    [SerializeField] private GameObject[] visualPrefabs; // <-- 추가: 여기에 랜덤으로 생성할 프리팹들을 등록하세요.

    private SplineContainer splineContainer;
    private float distanceAlongSpline = 0f;
    private float currentSpinAngle = 0f;
    private float splineLength;
    private float rotationSpeed;
    private float moveSpeed;
    void Start()
    {
        splineContainer = GetComponentInParent<SplineContainer>();

        if (splineContainer == null)
        {
            Debug.LogError("Spline Container가 설정되지 않았습니다!");
            this.enabled = false;
            return;
        }

        splineLength = splineContainer.Spline.GetLength();

        if (splineLength <= 0.01f)
        {
            Debug.LogWarning("스플라인 길이가 0에 가깝습니다.");
            this.enabled = false;
            return;
        }

        InitializeFollower();
    }


    void InitializeFollower()
    {
        rotationSpeed = UnityEngine.Random.Range(50f, 80f);
        moveSpeed = UnityEngine.Random.Range(80f, 120f);
        // 2. 랜덤 프리팹 생성
        SpawnRandomPrefab();
    }

    /// <summary>
    /// 등록된 프리팹 중 하나를 랜덤으로 선택하여 자식 오브젝트로 생성합니다.
    /// </summary>
    void SpawnRandomPrefab()
    {
        // 프리팹이 등록되어 있지 않으면 함수 종료
        if (visualPrefabs == null || visualPrefabs.Length == 0)
        {
            Debug.LogWarning("등록된 visualPrefabs가 없습니다.");
            return;
        }

        // 기존에 생성된 자식 (프리팹)이 있다면 모두 삭제
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 0과 (배열 길이 - 1) 사이의 랜덤 인덱스 선택
        int randomIndex = UnityEngine.Random.Range(0, visualPrefabs.Length);
        GameObject prefabToSpawn = visualPrefabs[randomIndex];

        if (prefabToSpawn != null)
        {
            // 이 SplineFollower 오브젝트의 자식으로 프리팹을 인스턴스화합니다.
            // 부모(this.transform)를 지정하여 생성하면 월드 좌표가 아닌 로컬 좌표계로 자동 설정됩니다.
            GameObject spawnedVisual = Instantiate(prefabToSpawn, transform);

            // 자식 오브젝트의 로컬 위치와 회전을 초기화 (부모의 정중앙에 위치)
            spawnedVisual.transform.localPosition = Vector3.zero;
            spawnedVisual.transform.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        if (splineContainer == null || splineLength <= 0.01f) return;

        float t = Mathf.Clamp01(distanceAlongSpline / splineLength);

        // 스플라인의 t 위치에서의 값 평가
        splineContainer.Spline.Evaluate(t, out float3 localPosition, out float3 localTangent, out float3 localUpVector);

        // 로컬 -> 월드 좌표로 변환
        Vector3 worldPosition = splineContainer.transform.TransformPoint(localPosition);
        Vector3 worldTangent = splineContainer.transform.TransformDirection(localTangent);
        Vector3 worldUpVector = splineContainer.transform.TransformDirection(localUpVector);

        // 위치 적용
        transform.position = worldPosition;

        // 회전 계산 (경로 방향)
        Quaternion pathRotation;
        if (worldTangent != Vector3.zero && worldUpVector != Vector3.zero)
        {
            pathRotation = Quaternion.LookRotation(worldTangent, worldUpVector);
        }
        else
        {
            pathRotation = transform.rotation;
        }

        // 추가 회전 (자체 스핀) - 랜덤 설정된 rotationSpeed 사용
        currentSpinAngle += rotationSpeed * Time.deltaTime;
        Quaternion spinRotation = Quaternion.AngleAxis(currentSpinAngle, rotationAxis);

        // 최종 회전 적용
        transform.rotation = pathRotation * spinRotation;

        // --- 수정 및 추가된 부분 ---

        // 1. 스플라인을 따라 거리 증가
        distanceAlongSpline += moveSpeed * Time.deltaTime;

        // 2. 스플라인 끝에 도달했는지 확인
        if (distanceAlongSpline >= splineLength)
        {
            // 3. 끝에 도달하면 "반복" (리셋)
            distanceAlongSpline = 0f; // 거리 0으로 리셋
            InitializeFollower();     // 회전 속도 및 프리팹 재생성
        }
    }
}