using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    // 마지막으로 활성화된 체크포인트의 위치를 모든 스크립트가 공유하도록 static으로 선언
    public static Vector3 LastActivatedRespawnPpointPosition { get; private set; }

    // 체크포인트가 활성화되었는지 여부 (중복 활성화 방지)
    private bool isActivated = false;

    // 플레이어가 처음 생성될 위치를 지정하기 위한 옵션
    [SerializeField] private bool isStartingPoint = false;

    void Awake()
    {
        // 이 체크포인트가 시작 지점이라면, 게임 시작 시 이곳을 마지막 활성 지점으로 설정
        if (isStartingPoint)
        {
            LastActivatedRespawnPpointPosition = transform.position;
            isActivated = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 충돌했고, 아직 활성화되지 않았다면
        if (other.CompareTag("Player") && !isActivated)
        {
            Debug.Log($"체크포인트 활성화: {gameObject.name}");
            LastActivatedRespawnPpointPosition = transform.position;
            isActivated = true;

        }
    }
}