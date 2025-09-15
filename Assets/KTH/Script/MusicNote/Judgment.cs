using System.Collections.Generic;
using UnityEngine;

public class Judgment : MonoBehaviour
{
    // 판정선 안에 들어와 있는 노트들을 저장하는 리스트
    private List<NoteController> notesInJudgmentZone = new List<NoteController>();

    // 어떤 키가 어떤 음계를 담당할지 설정
    private Dictionary<KeyCode, string> keyToNoteMap;

    void Awake()
    {
        // 키와 노트 이름을 연결합니다.
        keyToNoteMap = new Dictionary<KeyCode, string>
        {
            { KeyCode.A, "C" },
            { KeyCode.S, "D" },
            { KeyCode.D, "E" },
            { KeyCode.F, "F" },
            { KeyCode.G, "G" },
            { KeyCode.H, "A" },
            { KeyCode.J, "B" },
            { KeyCode.K, "C_H" },
            { KeyCode.L, "A_H" }
        };
    }

    void Update()
    {
        // 설정된 모든 키에 대해 입력을 확인
        foreach (var entry in keyToNoteMap)
        {
            if (Input.GetKeyDown(entry.Key))
            {
                CheckNote(entry.Value); // 키에 해당하는 노트 이름으로 판정
            }
        }
    }

    private void CheckNote(string targetNoteName)
    {
        // 판정선 안에 있는 모든 노트를 확인
        for (int i = 0; i < notesInJudgmentZone.Count; i++)
        {
            NoteController note = notesInJudgmentZone[i];

            // 누른 키와 노트의 이름이 일치하는가?
            if (note.noteName == targetNoteName)
            {
                Debug.Log("Hit! - " + targetNoteName);

                // SoundManager를 통해 소리 재생
                SoundManager.instance.PlaySFX(note.noteName);

                // 판정된 노트는 리스트에서 제거하고 파괴
                notesInJudgmentZone.RemoveAt(i);
                Destroy(note.gameObject);

                return; // 한 번에 하나의 노트만 판정
            }
        }
    }

    // 노트가 판정선에 들어왔을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        NoteController note = other.GetComponent<NoteController>();
        if (note != null)
        {
            notesInJudgmentZone.Add(note);
        }
    }

    // 노트가 판정선을 벗어났을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        NoteController note = other.GetComponent<NoteController>();
        if (note != null)
        {
            notesInJudgmentZone.Remove(note);
        }
    }
}