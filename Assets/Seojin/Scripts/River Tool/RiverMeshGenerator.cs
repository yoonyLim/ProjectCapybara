// RiverMeshGenerator.cs 파일의 내용을 아래 코드로 완전히 교체하세요.

using UnityEngine;
using System.Collections.Generic;

public static class RiverMeshGenerator
{
    public static void GenerateMesh(RiverController controller, MeshFilter meshFilter)
    {
        if (controller == null || meshFilter == null) return;

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh { name = "River Mesh" };
            meshFilter.sharedMesh = mesh;
        }
        else
        {
            mesh.Clear();
        }

        var keypoints = controller.keypoints;
        if (keypoints.Count < 2) return;

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();
        float distance = 0;

        // 첫 번째 키포인트의 정점을 먼저 추가합니다.
        vertices.Add(keypoints[0].leftBank);
        vertices.Add(keypoints[0].rightBank);
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        
        int vertCount = 2;

        // 키포인트 사이를 스플라인으로 보간하며 정점을 추가합니다.
        for (int i = 0; i < keypoints.Count - 1; i++)
        {
            RiverController.RiverKeypoint p1 = keypoints[i];
            RiverController.RiverKeypoint p2 = keypoints[i + 1];

            // --- 수정된 부분 ---
            // 키포인트에 계산된 실제 강둑 너비(절반)를 가져옵니다.
            float p1_CorrectedHalfWidth = Vector3.Distance(p1.position, p1.rightBank);
            float p2_CorrectedHalfWidth = Vector3.Distance(p2.position, p2.rightBank);
            // --- 여기까지 ---

            for (int j = 1; j <= controller.segmentsPerKeyPoint; j++)
            {
                float t = (float)j / controller.segmentsPerKeyPoint;

                Vector3 pointOnSpline = controller.GetPointOnSpline(i, t);
                Quaternion interpolatedRot = Quaternion.Slerp(p1.rotation, p2.rotation, t);
                
                // --- 수정된 부분 ---
                // 단순 width 속성 대신, 보정된 실제 너비를 보간합니다.
                float finalHalfWidth = Mathf.Lerp(p1_CorrectedHalfWidth, p2_CorrectedHalfWidth, t);
                // --- 여기까지 ---

                Vector3 rightDir = interpolatedRot * Vector3.right;

                vertices.Add(pointOnSpline - rightDir * finalHalfWidth);
                vertices.Add(pointOnSpline + rightDir * finalHalfWidth);
                
                // 이전 정점과의 거리를 계산하여 V좌표를 자연스럽게 늘려줌
                distance += Vector3.Distance(vertices[vertCount - 2], vertices[vertCount]);
                uvs.Add(new Vector2(0, distance * 0.1f)); // 0.1f는 텍스처 타일링을 위한 임의의 스케일값
                uvs.Add(new Vector2(1, distance * 0.1f));

                // 삼각형 구성
                int bl = vertCount - 2;
                int br = vertCount - 1;
                int tl = vertCount;
                int tr = vertCount + 1;
                
                triangles.Add(bl); triangles.Add(tl); triangles.Add(br);
                triangles.Add(tl); triangles.Add(tr); triangles.Add(br);

                vertCount += 2;
            }
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}