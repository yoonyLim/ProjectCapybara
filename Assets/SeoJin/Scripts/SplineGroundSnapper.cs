using UnityEngine;
using UnityEngine.Splines;

public class SplineGroundSnapper : MonoBehaviour
{
    private SplineContainer splineContainer;
    
    public float yOffset = 0.5f;
    public LayerMask groundLayer;

    public void ResetKnotsY()
    {
        splineContainer = GetComponent<SplineContainer>();

        for (int i = 0; i < splineContainer.Spline.Count; i++)
        {
            var knot = splineContainer.Spline[i];

            Vector3 knotWorldPosition = splineContainer.transform.TransformPoint(knot.Position);
            Vector3 targetLocalPosition = splineContainer.transform.InverseTransformPoint(new Vector3(knotWorldPosition.x, transform.position.y, knotWorldPosition.z));

            knot.Position = targetLocalPosition;
            splineContainer.Spline[i] = knot;
        }
    }
    
    public void SnapKnotsToGround()
    {
        float currentYpos = transform.position.y;
        
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit1, Mathf.Infinity, groundLayer))
        {
            transform.position  = new Vector3(transform.position.x, hit1.point.y + yOffset, transform.position.z);
        }
        
        splineContainer = GetComponent<SplineContainer>();
        
        for (int i = 0; i < splineContainer.Spline.Count; i++)
        {
            // 현재 점을 가져옵니다.
            var knot = splineContainer.Spline[i];

            // 점의 월드 좌표를 계산합니다. (스플라인의 위치+회전+크기를 반영)
            Vector3 knotWorldPosition = splineContainer.transform.TransformPoint(knot.Position);

            // 해당 월드 좌표에서 아래 방향으로 Raycast를 쏩니다.
            if (Physics.Raycast(knotWorldPosition + (Vector3.up * currentYpos), Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                // Raycast가 groundLayer에 부딪혔다면
                // 부딪힌 지점의 월드 좌표를 다시 스플라인의 로컬 좌표로 변환합니다.
                Vector3 targetLocalPosition = splineContainer.transform.InverseTransformPoint(hit.point);

                // 점의 위치를 업데이트합니다.
                // 핸들(In/Out Tangent)의 상대적 위치는 유지하기 위해 Position만 바꿉니다.
                knot.Position = targetLocalPosition + Vector3.up * (yOffset);

                // 변경된 점의 정보를 다시 스플라인에 적용합니다. (중요!)
                splineContainer.Spline[i] = knot;
            }
        }
    }
}
