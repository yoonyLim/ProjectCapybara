using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

#if UNITY_EDITOR

[CustomEditor(typeof(SplineGroundSnapper))] // 이 에디터가 어떤 컴포넌트를 대상으로 할지 지정
public class SplineGroundSnapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SplineGroundSnapper snapper = (SplineGroundSnapper)target;

        if (GUILayout.Button("Snap To Ground"))
        {
            SplineContainer splineContainer = snapper.GetComponent<SplineContainer>();
            if (splineContainer != null)
            {
                Undo.RecordObject(splineContainer, "Spline Ground Snap");
                snapper.SnapKnotsToGround();
                EditorUtility.SetDirty(splineContainer);
                SceneView.RepaintAll();
            }
        }

        if (GUILayout.Button("Reset Y"))
        {
            SplineContainer splineContainer = snapper.GetComponent<SplineContainer>();
            if (splineContainer != null)
            {
                Undo.RecordObject(splineContainer, "Spline Ground Snap");
                snapper.ResetKnotsY();
                EditorUtility.SetDirty(splineContainer);
                SceneView.RepaintAll();
            }
        }
    }
}
#endif
