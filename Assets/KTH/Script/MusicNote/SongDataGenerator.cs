using UnityEngine;

public class SongDataGenerator : MonoBehaviour
{
    [Tooltip("데이터를 채워넣을 SongData 에셋")]
    public SongData targetSongData;

    void Start()
    {
        if (targetSongData == null)
        {
            Debug.LogError("Target Song Data가 연결되지 않았습니다!");
            return;
        }

        targetSongData.notes.Clear();

        // '생일 축하 노래' 악보 데이터를 CDEFGAB 이름으로 추가합니다.
        targetSongData.notes.Add(new NoteData { noteName = "C", timeToAppear = 1.0f });
        targetSongData.notes.Add(new NoteData { noteName = "C", timeToAppear = 1.4f });
        targetSongData.notes.Add(new NoteData { noteName = "D", timeToAppear = 2.0f });
        targetSongData.notes.Add(new NoteData { noteName = "C", timeToAppear = 2.6f });
        targetSongData.notes.Add(new NoteData { noteName = "F", timeToAppear = 3.2f });
        targetSongData.notes.Add(new NoteData { noteName = "E", timeToAppear = 4.0f });
        targetSongData.notes.Add(new NoteData { noteName = "C", timeToAppear = 5.0f });
        targetSongData.notes.Add(new NoteData { noteName = "C", timeToAppear = 5.4f });
        targetSongData.notes.Add(new NoteData { noteName = "D", timeToAppear = 6.0f });
        targetSongData.notes.Add(new NoteData { noteName = "C", timeToAppear = 6.6f });
        targetSongData.notes.Add(new NoteData { noteName = "G", timeToAppear = 7.2f });
        targetSongData.notes.Add(new NoteData { noteName = "F", timeToAppear = 8.0f });
        targetSongData.notes.Add(new NoteData { noteName = "C", timeToAppear = 9.0f });
        targetSongData.notes.Add(new NoteData { noteName = "C", timeToAppear = 9.4f });
        targetSongData.notes.Add(new NoteData { noteName = "C_H", timeToAppear = 10.0f });
        targetSongData.notes.Add(new NoteData { noteName = "A", timeToAppear = 10.6f });
        targetSongData.notes.Add(new NoteData { noteName = "F", timeToAppear = 11.2f });
        targetSongData.notes.Add(new NoteData { noteName = "E", timeToAppear = 11.8f });
        targetSongData.notes.Add(new NoteData { noteName = "D", timeToAppear = 12.6f });
        targetSongData.notes.Add(new NoteData { noteName = "A_H", timeToAppear = 13.6f });
        targetSongData.notes.Add(new NoteData { noteName = "A_H", timeToAppear = 14.0f });
        targetSongData.notes.Add(new NoteData { noteName = "A", timeToAppear = 14.6f });
        targetSongData.notes.Add(new NoteData { noteName = "F", timeToAppear = 15.2f });
        targetSongData.notes.Add(new NoteData { noteName = "G", timeToAppear = 15.8f });
        targetSongData.notes.Add(new NoteData { noteName = "F", timeToAppear = 16.6f });

        Debug.Log(targetSongData.name + "에 CDEFGAB 노트 데이터가 성공적으로 기록되었습니다!");

        gameObject.SetActive(false);
    }
}