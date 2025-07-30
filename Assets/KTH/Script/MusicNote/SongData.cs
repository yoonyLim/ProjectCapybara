using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New SongData", menuName = "Rhythm Game/Song Data")]
public class SongData : ScriptableObject
{
    [Tooltip("노트가 생성되어 판정선까지 가는 데 걸리는 시간(초)")]
    public float noteSpeed = 2.0f;

    [Tooltip("곡의 전체 악보 데이터")]
    public List<NoteData> notes;
}