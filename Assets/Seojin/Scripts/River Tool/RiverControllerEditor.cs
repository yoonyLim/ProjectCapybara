using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RiverController))]
public class RiverControllerEditor : Editor
{
    private SerializedProperty keypointsProp;
    private int selectedIndex = -1;

    private void OnEnable()
    {
        keypointsProp = serializedObject.FindProperty("keypoints");
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    // OnInspectorGUI를 수정하여 버튼을 추가합니다.
    public override void OnInspectorGUI()
    {
        // SerializedObject의 최신 상태를 반영합니다.
        serializedObject.Update();

        // 기본 인스펙터 필드(Global Width, Is Closed Loop 등)를 그립니다.
        DrawDefaultInspector();

        // --- 키포인트 컨트롤 UI (새로운 부분) ---
        // 키포인트가 선택되었을 때만 컨트롤 UI를 표시합니다.
        if (selectedIndex != -1 && selectedIndex < keypointsProp.arraySize)
        {
            EditorGUILayout.Space(10); // 여백 추가
            EditorGUILayout.HelpBox($"Editing Keypoint: {selectedIndex}", MessageType.Info);

            // 버튼들을 가로로 나란히 배치하기 위해 BeginHorizontal 사용
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Insert Before"))
                {
                    keypointsProp.InsertArrayElementAtIndex(selectedIndex);
                    selectedIndex++; // 기존에 선택했던 포인트를 계속 따라가도록 인덱스 조정
                }

                if (GUILayout.Button("Insert After"))
                {
                    keypointsProp.InsertArrayElementAtIndex(selectedIndex + 1);
                }
            }
            EditorGUILayout.EndHorizontal();

            // 삭제 버튼은 눈에 띄게 따로 배치
            GUI.backgroundColor = new Color(1, 0.6f, 0.6f);
            if (GUILayout.Button("Delete Selected Keypoint"))
            {
                keypointsProp.DeleteArrayElementAtIndex(selectedIndex);
                selectedIndex = -1; // 삭제 후 선택 해제
            }

            GUI.backgroundColor = Color.white; // 다른 UI를 위해 GUI 색상 원래대로 복원
        }

        // 모든 변경 사항을 실제 컴포넌트에 적용합니다.
        serializedObject.ApplyModifiedProperties();
    }

    // OnSceneGUI는 이제 핸들을 그리는 역할만 합니다.
    private void OnSceneGUI(SceneView sceneView)
    {
        serializedObject.Update();

        for (int i = 0; i < keypointsProp.arraySize; i++)
        {
            SerializedProperty kpProp = keypointsProp.GetArrayElementAtIndex(i);
            SerializedProperty posProp = kpProp.FindPropertyRelative("position");
            Vector3 currentPos = posProp.vector3Value;

            if (i == selectedIndex)
            {
                Handles.color = Color.yellow;
                Handles.Label(currentPos + Vector3.up * 0.7f, $"Keypoint: {i}");

                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(currentPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    posProp.vector3Value = newPos;
                }

                DrawWidthHandles(i);
            }
            else
            {
                Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
                if (Handles.Button(currentPos, Quaternion.identity, 0.5f, 0.5f, Handles.SphereHandleCap))
                {
                    selectedIndex = i;
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    // 이하는 변경 없음
    // RiverControllerEditor.cs 파일의 DrawWidthHandles 메소드를 아래 코드로 교체하세요.

    void DrawWidthHandles(int index)
    {
        SerializedProperty kpProp = keypointsProp.GetArrayElementAtIndex(index);
        SerializedProperty posProp = kpProp.FindPropertyRelative("position");
        // SerializedProperty rotProp = kpProp.FindPropertyRelative("rotation"); // 이 줄은 더 이상 필요 없습니다.
        SerializedProperty widthProp = kpProp.FindPropertyRelative("width");

        RiverController controller = (RiverController)target;
        Vector3 centerPos = posProp.vector3Value;
    
        // --- 핵심 변경 ---
        // SerializedObject 대신, Controller 인스턴스에서 직접 최신 rotation 값을 가져옵니다.
        // 이렇게 하면 OnValidate -> UpdateBankPoints를 통해 계산된 최신 방향을 즉시 반영할 수 있습니다.
        if (index >= controller.keypoints.Count) return; // 예외 처리
        Quaternion rotation = controller.keypoints[index].rotation;
        // --- 여기까지 ---

        float globalWidth = controller.globalWidth;
        Vector3 rightDir = rotation * Vector3.right;

        float handleSize = HandleUtility.GetHandleSize(centerPos) * 0.15f;
        Handles.color = new Color(1, 0.5f, 0.5f);

        float currentHalfWidth = (widthProp.floatValue * globalWidth) / 2f;
        Vector3 uncorrectedLeftBankPos = centerPos - rightDir * currentHalfWidth;
        Vector3 uncorrectedRightBankPos = centerPos + rightDir * currentHalfWidth;

        // --- 왼쪽 핸들 ---
        EditorGUI.BeginChangeCheck();
        Vector3 newLeftPos = Handles.Slider(uncorrectedLeftBankPos, -rightDir, handleSize, Handles.SphereHandleCap, 0);
        if (EditorGUI.EndChangeCheck())
        {
            float newHalfWidth = Vector3.Dot(newLeftPos - centerPos, -rightDir);
            widthProp.floatValue = Mathf.Max(0.01f, (newHalfWidth * 2) / globalWidth);
        }

        // --- 오른쪽 핸들 ---
        EditorGUI.BeginChangeCheck();
        Vector3 newRightPos = Handles.Slider(uncorrectedRightBankPos, rightDir, handleSize, Handles.SphereHandleCap, 0);
        if (EditorGUI.EndChangeCheck())
        {
            float newHalfWidth = Vector3.Dot(newRightPos - centerPos, rightDir);
            widthProp.floatValue = Mathf.Max(0.01f, (newHalfWidth * 2) / globalWidth);
        }
    }
}