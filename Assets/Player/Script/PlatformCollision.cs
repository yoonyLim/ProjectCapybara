using UnityEngine;

public class PlatformCollision : MonoBehaviour
{
    private string playerTag = "Player";
    [SerializeField] private Transform platform; // 인스펙터에서 플랫폼 오브젝트를 연결해주세요.

    // PlatformPositionComposer 참조를 유지합니다.
    private PlatformPositionComposer movingPlatform;

    // PlayerController 참조를 저장할 변수를 추가합니다.
    private PlayerController playerController;

    void Start()
    {
        // PlatformPositionComposer 컴포넌트를 가져옵니다.
        movingPlatform = platform.GetComponent<PlatformPositionComposer>();
        if (movingPlatform == null)
        {
            Debug.LogError("움직이는 플랫폼에서 PlatformPositionComposer 스크립트를 찾을 수 없습니다!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // 플레이어가 플랫폼에 닿으면 PlayerController 컴포넌트를 저장합니다.
            playerController = other.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // playerController가 있고, movingPlatform이 연결되어 있을 때만 실행
        if (playerController != null && movingPlatform != null)
        {
            // ✨핵심✨: DeltaPosition(위치 변화량)을 시간으로 나누어 속도(Velocity)를 계산합니다.
            Vector3 platformVel = movingPlatform.DeltaPosition / Time.fixedDeltaTime;

            // 계산된 속도를 PlayerController에 전달합니다.
            playerController.platformVelocity = platformVel;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // 플레이어가 플랫폼에서 벗어나면, 저장된 속도를 0으로 초기화합니다.
            if (playerController != null)
            {
                playerController.platformVelocity = Vector3.zero;
            }
            // 참조를 초기화합니다.
            playerController = null;
        }
    }
}