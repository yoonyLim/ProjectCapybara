using UnityEngine;
using UnityEditor;
using System.Linq; // LINQ를 사용하기 위해 추가
using System.Collections.Generic;

// PlaySound 클래스의 인스펙터를 커스터마이징하겠다고 선언
[CustomEditor(typeof(PlaySound))]
public class PlaySoundEditor : Editor
{
    private string[] bgmNames;
    private string[] sfxNames;

    // 인스펙터가 활성화될 때 한 번 호출
    private void OnEnable()
    {
        // === 변경된 부분: SoundManager를 찾는 대신 Resources 폴더를 직접 읽어옵니다 ===

        // 1. Resources/BGM 폴더에서 모든 AudioClip을 불러옵니다.
        AudioClip[] bgmClips = Resources.LoadAll<AudioClip>("BGM");
        // 2. 불러온 AudioClip 배열에서 이름(name)만 추출하여 문자열 배열로 만듭니다.
        bgmNames = bgmClips.Select(clip => clip.name).ToArray();

        // 3. Resources/SFX 폴더에서 모든 AudioClip을 불러옵니다.
        AudioClip[] sfxClips = Resources.LoadAll<AudioClip>("SFX");
        // 4. 불러온 AudioClip 배열에서 이름(name)만 추출하여 문자열 배열로 만듭니다.
        sfxNames = sfxClips.Select(clip => clip.name).ToArray();
    }

    // 인스펙터 GUI를 그리는 함수
    public override void OnInspectorGUI()
    {
        // 원본 인스펙터 변경을 시작
        serializedObject.Update();

        // 현재 편집 중인 PlaySound 스크립트를 가져옴
        PlaySound playSound = (PlaySound)target;

        // Resources 폴더에 사운드가 없는 경우 경고 메시지를 표시
        if (bgmNames.Length == 0 && sfxNames.Length == 0)
        {
            EditorGUILayout.HelpBox("Resources/BGM 또는 Resources/SFX 폴더에 오디오 클립이 없습니다.", MessageType.Warning);
        }
        else
        {
            // 커스텀 리스트 UI를 그리는 함수 호출
            DrawSoundList("BGM 재생 목록", playSound.bgmNameList, bgmNames);
            DrawSoundList("2D SFX 재생 목록", playSound.sfxNameList, sfxNames);
            DrawSoundList("3D SFX 재생 목록", playSound.sfx3DNameList, sfxNames);
        }

        // 변경된 내용을 저장
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }

    // 드롭다운 메뉴와 추가/삭제 버튼이 있는 리스트 UI를 그리는 헬퍼 함수
    private void DrawSoundList(string title, List<string> soundNameList, string[] availableNames)
    {
        if (availableNames == null || availableNames.Length == 0) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        // 리스트에 등록된 사운드들을 드롭다운 메뉴로 표시
        for (int i = 0; i < soundNameList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal(); // 가로 정렬 시작

            int currentIndex = System.Array.IndexOf(availableNames, soundNameList[i]);
            if (currentIndex < 0) currentIndex = 0; // 선택된 사운드가 목록에 없으면 0번으로 초기화

            // 드롭다운 메뉴(Popup)를 생성
            int selectedIndex = EditorGUILayout.Popup(currentIndex, availableNames);
            soundNameList[i] = availableNames[selectedIndex];

            // "-" 버튼을 눌러 리스트에서 제거
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                soundNameList.RemoveAt(i);
                // 리스트가 변경되었으므로 for 루프를 빠져나가 다음 GUI 프레임에서 다시 그리도록 함
                break;
            }
            EditorGUILayout.EndHorizontal(); // 가로 정렬 끝
        }

        // "+" 버튼을 눌러 리스트에 새 항목 추가
        if (GUILayout.Button("사운드 추가 (+)"))
        {
            soundNameList.Add(availableNames[0]); // 기본값으로 첫 번째 사운드를 추가
        }
    }
}