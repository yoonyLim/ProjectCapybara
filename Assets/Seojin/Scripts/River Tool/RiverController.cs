using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Tools/River Controller")]
public class RiverController : MonoBehaviour
{
    [SerializeField]
    public List<RiverKeypoint> keypoints = new List<RiverKeypoint>();

    [SerializeField] public float globalWidth = 1f;
    [SerializeField] private Material riverMaterial;

    [Header("Spline Settings")]
    public bool isClosedLoop = false;

    public bool manualRotationMode = false;
    
    [Tooltip("두 키포인트 사이가 얼마나 부드러울지 결정함")]
    [SerializeField, Range(1, 30)]
    public int segmentsPerKeyPoint = 10;

    [Tooltip("스플라인의 텐션")]
    [SerializeField, Range(0f, 1f)]
    public float tension = 0.5f;

    [System.Serializable]
    public struct RiverKeypoint
    {
        public Vector3 position;
        public Quaternion rotation;
        public float width;

        public Vector3 leftBank;
        public Vector3 rightBank;

        public RiverKeypoint(Vector3 position, float width = 1f)
        {
            this.position = position;
            this.width = width;
            this.rotation = Quaternion.identity;
            this.leftBank = position;
            this.rightBank = position;
        }
    }

    private void OnValidate()
    {
        if (keypoints != null && keypoints.Count > 1)
        {
            UpdateBankPoints();
            GenerateRiverMesh();
        }
    }

    // KeyPoint의 접선과 수직한 선분의 양 끝점 Update
    public void UpdateBankPoints()
    {
        if (keypoints == null || keypoints.Count < 2) return;

        for (int i = 0; i < keypoints.Count; i++)
        {
            RiverKeypoint kp = keypoints[i];
            Vector3 posCurrent = GetPoint(i);

            // 1. 회전값 계산 (이전과 동일)
            if (!manualRotationMode)
            {
                Vector3 posPrev = GetPoint(i - 1);
                Vector3 posNext = GetPoint(i + 1);
                Vector3 dirToPrev = (posCurrent - posPrev).normalized;
                Vector3 dirToNext = (posNext - posCurrent).normalized;

                if (!isClosedLoop && i == 0) dirToPrev = dirToNext;
                if (!isClosedLoop && i == keypoints.Count - 1) dirToNext = dirToPrev;
            
                Vector3 forwardDir = (dirToPrev + dirToNext).normalized;
                if(forwardDir == Vector3.zero) forwardDir = dirToNext;

                kp.rotation = Quaternion.LookRotation(forwardDir, Vector3.up);
            }
        
            // 2. 너비 및 강둑 위치 계산
            Vector3 rightDir = kp.rotation * Vector3.right;
            float currentWidth = kp.width > 0.01f ? kp.width : 1f;
            float finalHalfWidth = currentWidth * globalWidth / 2f;

            // ★★★ 핵심 수정 사항 ★★★
            // 너비 보정은 닫힌 루프거나, 열린 루프의 중간 포인트들일 때만 적용합니다.
            if (isClosedLoop || (i > 0 && i < keypoints.Count - 1))
            {
                Vector3 posNext = GetPoint(i + 1);
                Vector3 dirToNext = (posNext - posCurrent).normalized;
            
                float dot = Vector3.Dot(dirToNext, rightDir);
                float widthCorrection = 1f / Mathf.Max(0.2f, Mathf.Abs(dot)); // 0.1 -> 0.2로 안정성 소폭 상향
            
                finalHalfWidth *= Mathf.Clamp(widthCorrection, 1, 2.5f);
            }
            // ★★★ 여기까지 ★★★

            kp.leftBank = posCurrent - rightDir * finalHalfWidth;
            kp.rightBank = posCurrent + rightDir * finalHalfWidth;
        
            keypoints[i] = kp;
        }
    }

    private void Reset()
    {
        keypoints = new List<RiverKeypoint>
        {
            new RiverKeypoint(transform.position + new Vector3(0, 0, 0), 1.5f),
            new RiverKeypoint(transform.position + new Vector3(0, 0, 10), 2f),
            new RiverKeypoint(transform.position + new Vector3(5, 0, 20), 2.5f),
            new RiverKeypoint(transform.position + new Vector3(-5, 0, 30), 2f)
        };
        UpdateBankPoints(); 
    }
    
    private void OnDrawGizmos()
    {
        if (keypoints == null || keypoints.Count < 2) return;

        // Draw Spline Curve
        Gizmos.color = Color.green;
        for (int i = 0; i < keypoints.Count; i++)
        {
            if (!isClosedLoop && i == keypoints.Count - 1) break;
            DrawSplineSegment(i);
        }

        // Draw Keypoints
        for (int i = 0; i < keypoints.Count; i++)
        {
            RiverKeypoint kp = keypoints[i]; 

            // Transform
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(kp.position, 0.5f);

            // Direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(kp.position, kp.rotation * Vector3.forward * 2f);

            // Width
            Gizmos.color = Color.red;
            Gizmos.DrawLine(kp.leftBank, kp.rightBank);
        }
    }

    #region Cardinal Spline
    // 접선 방향 계산하기
    private Vector3 CalculateTangent(int index)
    {
        if (keypoints.Count < 2) return Vector3.forward;
        Vector3 previousPoint = GetPoint(index - 1);
        Vector3 nextPoint = GetPoint(index + 1);
        Vector3 tangent = (nextPoint - previousPoint).normalized;
        if (tangent == Vector3.zero)
        {
            if (index < keypoints.Count - 1)
                tangent = (GetPoint(index + 1) - GetPoint(index)).normalized;
            else
                tangent = (GetPoint(index) - GetPoint(index - 1)).normalized;
        }
        return tangent;
    }
    
    // Spline Segment로 그리기
    private void DrawSplineSegment(int pointIndex)
    {
        Vector3 p0 = GetPoint(pointIndex - 1);
        Vector3 p1 = GetPoint(pointIndex);
        Vector3 p2 = GetPoint(pointIndex + 1);
        Vector3 p3 = GetPoint(pointIndex + 2);
        Vector3 lastPoint = p1;
        for (int j = 1; j <= segmentsPerKeyPoint; j++)
        {
            float t = (float)j / segmentsPerKeyPoint;
            Vector3 newPoint = GetPointOnCardinalSpline(t, p0, p1, p2, p3);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }

    // index기반으로 위치 가져오기
    private Vector3 GetPoint(int index)
    {
        if (isClosedLoop)
        {
            if (index < 0) index += keypoints.Count;
            return keypoints[index % keypoints.Count].position;
        }
        else
        {
            return keypoints[Mathf.Clamp(index, 0, keypoints.Count - 1)].position;
        }
    }

    // Cardinal Spline 계산
    public Vector3 GetPointOnCardinalSpline(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // 기준이 되는 t 값 (0~1)
        float t2 = t * t;
        float t3 = t2 * t;
        
        // Hermite Basis Formula 가중치 계산
        float h1 = 2 * t3 - 3 * t2 + 1;
        float h2 = -2 * t3 + 3 * t2;
        float h3 = t3 - 2 * t2 + t;
        float h4 = t3 - t2;
        
        Vector3 tangent1 = (p2 - p0) * (1f - tension) / 2f; // p0의 접선
        Vector3 tangent2 = (p3 - p1) * (1f - tension) / 2f; // p1의 접선
        
        return (h1 * p1) + (h2 * p2) + (h3 * tangent1) + (h4 * tangent2);
    }
    
    public Vector3 GetPointOnSpline(int keypointIndex, float t)
    {
        Vector3 p0 = GetPoint(keypointIndex - 1);
        Vector3 p1 = GetPoint(keypointIndex);
        Vector3 p2 = GetPoint(keypointIndex + 1);
        Vector3 p3 = GetPoint(keypointIndex + 2);
        return GetPointOnCardinalSpline(t, p0, p1, p2, p3);
    }

    public Vector3 GetTangentOnSpline(int keypointIndex, float t)
    {
        Vector3 p0 = GetPoint(keypointIndex - 1);
        Vector3 p1 = GetPoint(keypointIndex);
        Vector3 p2 = GetPoint(keypointIndex + 1);
        Vector3 p3 = GetPoint(keypointIndex + 2);

        float next_t = t + 0.01f;
        Vector3 currentPoint = GetPointOnCardinalSpline(t, p0, p1, p2, p3);
        Vector3 nextPoint = GetPointOnCardinalSpline(next_t, p0, p1, p2, p3);

        return (nextPoint - currentPoint).normalized;
    }


    [ContextMenu("Generate River Mesh")]
    public void GenerateRiverMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
    
        if (riverMaterial != null)
        {
            meshRenderer.sharedMaterial = riverMaterial;
        }

        RiverMeshGenerator.GenerateMesh(this, meshFilter);
    }
    #endregion
}