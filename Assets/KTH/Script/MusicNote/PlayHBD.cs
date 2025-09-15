using UnityEngine;
using System.Collections.Generic; // Dictionary를 사용하기 위해 추가

public class PlayHBD : MonoBehaviour
{
    [Header("핵심 데이터")]
    public SongData songData;

    [Header("노트 생성 설정")]
    public GameObject notePrefab;
    public Transform spawnPoint;
    public Transform notesParent;

    [Header("게임플레이 설정")]
    public Transform judgmentLine;
    [Tooltip("음계 한 칸당 Y축 높이 차이")]
    public float yPositionStep = 75f; // <<-- 새로 추가된 변수

    // --- 새로 추가된 부분 ---
    private Dictionary<string, int> noteHeightMap;
    // ----------------------

    private int nextNoteIndex = 0;
    private float songStartTime;
    private bool isGameRunning = false;

    // --- 새로 추가된 Awake 함수 ---
    void Awake()
    {
        // 게임이 시작될 때 음계별 높이 정보를 미리 만들어 둡니다.
        noteHeightMap = new Dictionary<string, int>
        {
            { "C", 0 }, { "D", 1 }, { "E", 2 }, { "F", 3 }, { "G", 4 },
            { "A", 5 }, { "B", 6 }, { "C_H", 7 }, { "A_H", 12 }
        };
    }
    // -------------------------

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartGame();
        }

        if (isGameRunning)
        {
            ProcessSong();
        }
    }

    public void StartGame()
    {
        if (songData == null || notePrefab == null)
        {
            Debug.LogError("Song Data 또는 Note Prefab이 연결되지 않았습니다!");
            return;
        }
        songStartTime = Time.time;
        nextNoteIndex = 0;
        isGameRunning = true;
    }

    private void ProcessSong()
    {
        float currentTime = Time.time - songStartTime;

        if (nextNoteIndex < songData.notes.Count && currentTime >= songData.notes[nextNoteIndex].timeToAppear)
        {
            SpawnNote(songData.notes[nextNoteIndex]);
            nextNoteIndex++;
        }
    }

    // PlayHBD.cs 스크립트의 SpawnNote 함수만 수정합니다.
    private void SpawnNote(NoteData noteData)
    {
        Vector3 spawnPosition = spawnPoint.position;

        if (noteHeightMap.TryGetValue(noteData.noteName, out int heightIndex))
        {
            spawnPosition.y += heightIndex * yPositionStep;
        }
        else
        {
            Debug.LogWarning("noteHeightMap에 없는 노트 이름입니다: " + noteData.noteName);
        }

        GameObject noteObject = Instantiate(notePrefab, spawnPosition, Quaternion.identity, notesParent);
        noteObject.name = "Note_" + noteData.noteName;

        NoteController noteController = noteObject.GetComponent<NoteController>();
        if (noteController != null)
        {
            float distance = Vector3.Distance(spawnPoint.position, judgmentLine.position);
            noteController.speed = distance / songData.noteSpeed;
            noteController.noteName = noteData.noteName; // <<-- 이 줄을 추가하세요!
        }
    }
}